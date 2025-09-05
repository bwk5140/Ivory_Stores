namespace MethaWebsite.Data.ResponseModel
{
    using MethaWebsite.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Win32;
    using Mono.TextTemplating;
    using System.Diagnostics;
    using System.Linq;

    public sealed class ResponseEngine
    {
        private readonly IIntentAnchorProvider _intentAnchorProvider;
        private readonly ISlotExtractor _slotExtractor;
        private readonly ISlotResolver _slotResolver;
        private readonly IResponseValidator _validator;
        private readonly IClarificationStrategy _clarificationStrategy;
        private readonly ITemplateProvider _templateProvider;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly ILogger<ResponseEngine> _logger;
        private readonly ResponseEngineOptions _options;
        private readonly SlotFillerRegistry registry;
        private readonly IConversationStore _contextStore;
        private readonly IConversationStateStore _stateStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIntentRecognizer _intentRecognizer;
        private readonly ConversationManager _conversationManager;
        private readonly IReadOnlyList<string>? issues;
        private Stopwatch? _stopwatch;

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

        public ResponseEngine(
            IIntentAnchorProvider intentAnchorProvider,
            ISlotExtractor slotExtractor,
            ISlotResolver slotResolver,
            IResponseValidator validator,
            IClarificationStrategy clarificationStrategy,
            ITemplateProvider templateProvider,
            ITemplateRenderer templateRenderer,
            ILogger<ResponseEngine> logger,
            IOptions<ResponseEngineOptions> options,
            SlotFillerRegistry registry,
            IConversationStore conversationStore,
            IConversationStateStore conversationStateStore,
            IIntentRecognizer intentRecognizer,
            ConversationManager conversationManager)
        {
            _intentAnchorProvider = intentAnchorProvider;
            _slotExtractor = slotExtractor;
            _slotResolver = slotResolver;
            _validator = validator;
            _clarificationStrategy = clarificationStrategy;
            _templateProvider = templateProvider;
            _templateRenderer = templateRenderer;
            _logger = logger;
            _options = options.Value;
            this.registry = registry;
            _contextStore = conversationStore;
            _stateStore = conversationStateStore;
            _intentRecognizer = intentRecognizer;
            _conversationManager = conversationManager;
        }
        public ResponseResult GenerateResponse(ConversationAction action, ResponseRequest request, string conversationId)
        {
            var templates = _templateProvider.GetTemplates(action.AnchorId, request.Locale);
            var chosenTemplate = TemplateChooser.Choose(templates, action.FilledSlots, _logger, _stateStore, conversationId, request);

            _logger.LogDebug(LogEvents.TemplateChosen,
                "Chosen template {TemplateId} for anchor {AnchorId}",
                chosenTemplate?.TemplateId, action.AnchorId);

            string response = "";
            if (chosenTemplate is not null)
            {
                var filler = registry.GetFiller(chosenTemplate.AnchorId);
                response = chosenTemplate?.Text;
                var filledSlots = filler.FillSlots(action.FilledSlots, conversationId);
                foreach (var kvp in filledSlots)
                {
                    response = response.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
            }

            var text = string.IsNullOrWhiteSpace(action.ClarificationText) ?  _templateRenderer.Render(response ?? "", action.FilledSlots) : action.ClarificationText;

            _stopwatch.Stop();
            _logger.LogInformation(LogEvents.EngineCompleted,
                "Completed response in {ElapsedMs} ms", _stopwatch.ElapsedMilliseconds);

            return new ResponseResult
            {
                Text = text,
                NeedsClarification = action.Type is ConversationActionType.Clarify or ConversationActionType.Fallback or ConversationActionType.PromptForSlot,
                TemplateId = chosenTemplate?.TemplateId,
                AnchorId = action.AnchorId,
                Slots = action.FilledSlots,
                Issues = action.Type == (ConversationActionType.Clarify | ConversationActionType.Fallback | ConversationActionType.PromptForSlot) ? issues : Array.Empty<string>()
            };
        }
        public ConversationAction Generate(ResponseRequest request, string conversationId)
        {
            var state = _stateStore.GetState(conversationId);
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["IntentId"] = request.IntentId,
                ["IntentConfidence"] = request.IntentConfidence,
                ["Locale"] = request.Locale
            });

            _stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(LogEvents.EngineStart,
                "Generating response for intent {IntentId} (confidence: {Confidence})",
                request.IntentId, request.IntentConfidence);

            var anchorId = _intentAnchorProvider.GetAnchorForIntent(request.IntentId);
            var anchor = anchorId is not null
                ? _intentAnchorProvider.GetAnchorDefinition(anchorId)
                : null;

            var minConf = _options.GlobalMinIntentConfidence ?? anchor?.MinIntentConfidence ?? 0.0;
            if (anchor is null || request.IntentConfidence < minConf)
            {
                var level = _options.MissingAnchorLogLevel;
                _logger.Log(level, LogEvents.AnchorMissingOrLowConf,
                    "Anchor missing or low confidence. AnchorId={AnchorId}, RequiredMin={Min}, Actual={Actual}",
                    anchorId, minConf, request.IntentConfidence);

                return new ConversationAction
                {
                    Type = ConversationActionType.Fallback,
                    ClarificationText = _options.LowConfidenceFallbackText ?? "I’m not sure what you meant — could you rephrase?"
                };
            }

            _logger.LogDebug(LogEvents.AnchorResolved, "Resolved anchor {AnchorId}", anchor.AnchorId);

            var extractedSlots = _slotExtractor.Extract(request, anchor, state);
            _logger.LogDebug(LogEvents.SlotsExtracted,
                "Extracted {Count} slots: {Names}",
                extractedSlots.Count,
                string.Join(", ", extractedSlots.Keys));
            if (state is null)
            {
                List<string> PendingSlots = new List<string>();
                foreach (var slot in anchor.Slots.Where(s => s.Required).ToList())
                {
                    PendingSlots.Add(slot.Name);
                }
                state = new ConversationState
                {
                    AnchorId = anchor.AnchorId,
                    PendingSlots = PendingSlots,
                    LastUpdated = DateTime.UtcNow
                };
                _stateStore.SaveState(conversationId, state);
            }


            var resolvedSlots = _slotResolver.Resolve(request, anchor, extractedSlots);
            
            state!.History.Add(new ConversationTurn
            {
                UserInput = request.Utterance,
                SlotChanges = resolvedSlots.Where(kv => kv.Value.IsResolved).ToDictionary(kv => kv.Key, kv => kv.Value),
                PromptedSlot = state.LastPromptedSlot,
                Timestamp = DateTime.UtcNow
            });

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                // Optionally log slot values (mask if needed)
                var parts = resolvedSlots.Select(kv =>
                {
                    var value = _options.LogSlotValues ? kv.Value.Value : Mask(kv.Value.Value);
                    return $"{kv.Key}={value} (conf {kv.Value.Confidence:0.00}, resolved={kv.Value.IsResolved})";
                });
                _logger.LogTrace(LogEvents.SlotsResolved, "Resolved slots: {Slots}", string.Join("; ", parts));
            }
            
            var action = _conversationManager.HandleMessage(conversationId, request, anchor);
            _stopwatch.Stop();
            return action;
        }

        private static string? Mask(string? value) =>
            value is null ? null : new string('•', Math.Min(value.Length, 6));

        private static class TemplateChooser
        {
            public static Template? Choose(IReadOnlyList<Template> templates,
                                           IReadOnlyDictionary<string, SlotValue> slots,
                                           ILogger logger, IConversationStateStore stateStore, string conversationId,
                                           ResponseRequest request)
            {
                var state = stateStore.GetState(conversationId);
                // Simple heuristic: prefer templates whose Conditions are satisfied
                List<Template> SatisfiedTemplates = new List<Template>();
                foreach (var t in templates)
                {
                    if (t.Conditions is null || t.Conditions.Count == 0)
                        continue;

                    var ok = t.Conditions.All(c =>
                        c.StartsWith("has:", StringComparison.OrdinalIgnoreCase)
                            ? slots.ContainsKey(c.Substring(4))
                            : true);

                    if (ok) SatisfiedTemplates.Add(t);
                }
                Random rand = new Random();
                if (SatisfiedTemplates.Count <= 0)
                {
                    logger.LogDebug("No condition-matching template found; falling back to random template with no conditions.");
                    SatisfiedTemplates = templates.Where(t => t.Conditions is null).ToList();
                    var chosenTemplate = SatisfiedTemplates.Any() ? SatisfiedTemplates[rand.Next(SatisfiedTemplates.Count())] : templates.FirstOrDefault();
                    return chosenTemplate;
                }
                else
                {
                    if (state.PendingConfirmations.Any())
                    {
                        var chosenTemplate = SatisfiedTemplates.FirstOrDefault(t => t.Conditions.FirstOrDefault(c =>
                                c.Contains(state.PendingConfirmations.First().SlotName, StringComparison.OrdinalIgnoreCase) || c.Contains(request.Utterance, StringComparison.OrdinalIgnoreCase)) is not null);

                        return chosenTemplate ?? SatisfiedTemplates[rand.Next(SatisfiedTemplates.Count())];

                    }
                    else
                    {
                        var chosenTemplate = SatisfiedTemplates.FirstOrDefault(t => t.Conditions.FirstOrDefault(c =>
                                    c.Contains(state.LastPromptedSlot == null? "" : state.LastPromptedSlot, StringComparison.OrdinalIgnoreCase)) is not null);
                        return chosenTemplate ?? SatisfiedTemplates[rand.Next(SatisfiedTemplates.Count())];
                    }
                }
            }
        }
    }
}
