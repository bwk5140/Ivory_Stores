using Google.Protobuf.WellKnownTypes;
using MethaWebsite.Services;
using static System.Net.Mime.MediaTypeNames;

namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationManager
    {
        private readonly IIntentRecognizer _intentRecognizer;
        private readonly ISlotExtractor _slotExtractor;
        private readonly IConversationStateStore _stateStore;
        private readonly IIntentAnchorProvider _intentAnchorProvider;
        private readonly IResponseValidator _responseValidator;
        private readonly IClarificationStrategy _clarificationStrategy;
        private readonly ISlotResolver _slotResolver;
        private readonly ILogger<ConversationManager> _logger;
        private readonly IResponseValidator _validator;
        private IReadOnlyList<string>? issues;

        public ConversationManager(IIntentRecognizer intentRecognizer,
                                   ISlotExtractor slotExtractor,
                                   IConversationStateStore stateStore,
                                   IIntentAnchorProvider intentAnchorProvider,
                                   IResponseValidator responseValidator,
                                   IClarificationStrategy clarificationStrategy,
                                   IResponseValidator validator,
                                   ISlotResolver slotResolver,
                                   ILogger<ConversationManager> logger)
        {
            _intentRecognizer = intentRecognizer;
            _stateStore = stateStore;
            _slotExtractor = slotExtractor;
            _intentAnchorProvider = intentAnchorProvider;
            _responseValidator = responseValidator;
            _clarificationStrategy = clarificationStrategy;
            _validator = validator;
            _slotResolver = slotResolver;
            _logger = logger;
        }
        private static class LogEvents
        {
            public static readonly EventId EngineStart = new(1000, nameof(EngineStart));
            public static readonly EventId AnchorResolved = new(1001, nameof(AnchorResolved));
            public static readonly EventId AnchorMissingOrLowConf = new(1002, nameof(AnchorMissingOrLowConf));
            public static readonly EventId SlotsExtracted = new(1003, nameof(SlotsExtracted));
            public static readonly EventId SlotsResolved = new(1004, nameof(SlotsResolved));
            public static readonly EventId ValidationIssues = new(1005, nameof(ValidationIssues));
            public static readonly EventId TemplateChosen = new(1006, nameof(TemplateChosen));
            public static readonly EventId EngineCompleted = new(1007, nameof(EngineCompleted));
        }
        public ConversationAction HandleMessage(string conversationId, ResponseRequest request, AnchorDefinition? currentAnchor)
        {
            var state = _stateStore.GetState(conversationId);
            ResetStateIfExpired(state);

            // 1) Recognize intent and resolve candidate anchor
            var intentResult = _intentRecognizer.RecognizeIntent(request.Utterance);
            var confidence = GetIntentConfidence(intentResult.Score);
            var candidateAnchor = currentAnchor;
            if (!string.IsNullOrEmpty(state.AnchorId))
            {
                var currentAnchorId = _intentAnchorProvider.GetAnchorForIntent(state.AnchorId);
                currentAnchor = currentAnchorId is not null
                    ? _intentAnchorProvider.GetAnchorDefinition(currentAnchorId)
                    : currentAnchor;
            }
            // 2) Extract slots ONCE (relative to the current anchor by default)
            var extractedSlots = _slotExtractor.Extract(request, currentAnchor, state);

            // 3) Signals
            bool respondedToPrompt = state.LastPromptedSlot != null && extractedSlots.ContainsKey(state.LastPromptedSlot);
            bool slotChanged = HasAnyRelevantSlotChanged(extractedSlots, state, currentAnchor);
            bool slotMatchesDomain = SlotMatchesAnchorDomain(extractedSlots, currentAnchor);

            // Optional: confirmation lock (if you implement it)
            bool confirmationLocked = state.PendingConfirmations.Any();

            // 4) Decide active anchor
            AnchorDefinition activeAnchor = currentAnchor;

            if (respondedToPrompt || confirmationLocked)
            {
                // User is cooperating with our prompt → don't switch
                activeAnchor = currentAnchor;
            }
            else if (slotChanged && slotMatchesDomain)
            {
                // Slot-driven override: keep current flow even with low intent confidence
                activeAnchor = currentAnchor;
            }
            else if (ShouldSwitchAnchor(state, currentAnchor, candidateAnchor, confidence))
            {
                // Confident new intent, not mid-fill → switch and init pending
                SwitchAnchor(state, candidateAnchor);
                activeAnchor = candidateAnchor;
                // Re-extract slots if templates differ significantly
                extractedSlots = _slotExtractor.Extract(request, activeAnchor, state);
            }
            else
            {
                if (activeAnchor.AnchorId == state.AnchorId)
                {
                    SwitchAnchor(state, candidateAnchor);
                    activeAnchor = candidateAnchor;
                    // Re-extract slots if templates differ significantly
                    extractedSlots = _slotExtractor.Extract(request, activeAnchor, state);
                }
            }

            // 5) Update state using the extraction we already have
            state.LastUpdated = DateTime.UtcNow;
            UpdateSlots(state, extractedSlots, activeAnchor);
            _stateStore.SaveState(conversationId, state);

            // 6) Next action
            return GetNextAction(state, activeAnchor, request);
        }
        private string DetectIssues(ResponseRequest request, AnchorDefinition? anchor, IReadOnlyDictionary<string, SlotValue> extractedSlots)
        {
            var resolvedSlots = _slotResolver.Resolve(request, anchor, extractedSlots);
            issues = _validator.Validate(request, anchor, resolvedSlots);

            if (issues.Count > 0)
            {
                _logger.LogInformation(LogEvents.ValidationIssues,
                    "Validation returned {Count} issue(s): {Issues}", issues.Count, string.Join(" | ", issues));

                var clarification = _clarificationStrategy.BuildClarification(request, anchor, issues);
                return clarification;
            }
            return string.Empty;
        }
        private void UpdateSlots(ConversationState state,
                         IReadOnlyDictionary<string, SlotValue> extracted,
                         AnchorDefinition anchor)
        {
            foreach (var slot in anchor.Slots)
            {
                if (!extracted.TryGetValue(slot.Name, out var newVal))
                    continue;

                if (!state.FilledSlots.TryGetValue(slot.Name, out var oldVal) ||
                    !SlotEquals(oldVal, newVal))
                {
                    state.FilledSlots[slot.Name] = newVal;
                    state.PendingSlots.Remove(slot.Name);
                }
            }

            // Initialize pending if empty and anchor requires more
            if (state.PendingSlots == null || state.PendingSlots.Count == 0)
            {
                state.PendingSlots = anchor.Slots
                    .Where(s => s.Required && !state.FilledSlots.ContainsKey(s.Name))
                    .Select(s => s.Name)
                    .ToList();
            }
        }

        private static bool SlotEquals(SlotValue a, SlotValue b)
        {
            // Compare on canonical value; adapt to your SlotValue shape
            string ca = Canonical(a);
            string cb = Canonical(b);
            return StringComparer.OrdinalIgnoreCase.Equals(ca, cb);
        }

        private static string Canonical(SlotValue v)
        {
            // Prefer normalized; fall back to raw/value/ToString
            return  v?.Value ?? "";
        }

        private bool HasAnyRelevantSlotChanged(IReadOnlyDictionary<string, SlotValue> extracted,ConversationState state, AnchorDefinition anchor)
        {
            var relevant = new HashSet<string>(anchor.Slots.Select(s => s.Name));
            foreach (var (name, newVal) in extracted)
            {
                if (!relevant.Contains(name)) continue;

                if (!state.FilledSlots.TryGetValue(name, out var oldVal))
                    return true;

                if (!SlotEquals(oldVal, newVal))
                    return true;
            }
            return false;
        }

        private bool SlotMatchesAnchorDomain(IReadOnlyDictionary<string, SlotValue> extracted, AnchorDefinition anchor)
        {
            var relevant = new HashSet<string>(anchor.Slots.Select(s => s.Name));
            return extracted.Keys.Any(k => relevant.Contains(k));
        }

        private bool ShouldSwitchAnchor(ConversationState state,AnchorDefinition currentAnchor,AnchorDefinition candidateAnchor,
            double intentConfidence)
        {
            if (candidateAnchor == null) return false;
            if (currentAnchor == null) return true; // cold start

            // Don’t switch to the same anchor
            if (currentAnchor.AnchorId == candidateAnchor.AnchorId) return false;

            // Confidence guard
            return intentConfidence >= candidateAnchor.MinIntentConfidence;
        }

        private void SwitchAnchor(ConversationState state, AnchorDefinition newAnchor)
        {
            state.AnchorId = newAnchor.AnchorId;

            // Preserve global slots
            var globalSlotNames = newAnchor.Slots
                .Where(s => s.IsGlobal)
                .Select(s => s.Name)
                .ToHashSet();

            state.FilledSlots = state.FilledSlots
                .Where(kvp => globalSlotNames.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            state.PendingSlots = newAnchor.Slots
                .Where(s => s.Required && !state.FilledSlots.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();

            state.LastPromptedSlot = null;
            if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
        }
        private void ResetStateIfExpired(ConversationState state)
        {
            var ttl = TimeSpan.FromMinutes(5);
            if (DateTime.UtcNow - state.LastUpdated <= ttl) return;

            //state.AnchorId = null;
            state.FilledSlots.Clear();
            state.PendingSlots.Clear();
            state.LastPromptedSlot = null;
            if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
        }

        private ConversationAction GetNextAction(ConversationState state, AnchorDefinition anchor, ResponseRequest request)
        {
            var extractedSlots = _slotExtractor.Extract(request, anchor, state);
            var clarification = DetectIssues(request, anchor, extractedSlots);
            var resolvedSlots = _slotResolver.Resolve(request, anchor, state.FilledSlots);
            if (state.PendingSlots.Any())
            {
                var next = state.PendingSlots.First();
                state.LastPromptedSlot = next;
                return ConversationAction.PromptForSlot(clarification, next, anchor.AnchorId, state.FilledSlots);
            }
            if (!string.IsNullOrWhiteSpace(clarification))
            {
                return ConversationAction.Clarify(clarification, anchor.AnchorId, resolvedSlots);
            }
            return ConversationAction.ExecuteAnchor(anchor.AnchorId, state.FilledSlots);
        }
        public float GetIntentConfidence(float[] scores, float threshold = 0.6f)
        {
            if (scores == null || scores.Length == 0)
                throw new ArgumentException("Score array must not be empty.");

            // Find top intent
            int topIndex = Array.IndexOf(scores, scores.Max());
            float topScore = scores[topIndex];

            // Find second-best score for margin-based calibration
            float secondBest = scores
                .Where((score, index) => index != topIndex)
                .DefaultIfEmpty(0f)
                .Max();

            float margin = topScore - secondBest;
            if (margin < 0.4f)
                return topScore;

            return (topScore * margin);
        }
    }
}
