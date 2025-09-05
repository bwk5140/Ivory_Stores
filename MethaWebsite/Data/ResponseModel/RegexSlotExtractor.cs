namespace MethaWebsite.Data.ResponseModel
{
    public sealed class RegexSlotExtractor : ISlotExtractor
    {
        public IReadOnlyDictionary<string, SlotValue> Extract(ResponseRequest request, AnchorDefinition anchor, ConversationState state)
        {
            var dict = new Dictionary<string, SlotValue>(StringComparer.OrdinalIgnoreCase);

            // 1) Seed from provided entities
            if (request.Entities != null && request.Entities.Any())
            {
                foreach (var (k, v) in request.Entities)
                    dict[k] = new SlotValue { Name = k, Value = v, Confidence = 0.9, IsResolved = false };
            }

            // 2) Regex-based extraction per slot definition
            if (state is not null && state.PendingConfirmations.Any())
            {
                List<SlotDefinition> slots = new List<SlotDefinition>();
                foreach (var pendingConfirmation in state.PendingConfirmations)
                {
                    slots.AddRange(anchor.Slots.Where(s => s.Name == pendingConfirmation.SlotName));
                }

                foreach (var slot in slots)
                {
                    if (dict.ContainsKey(slot.Name)) continue;
                    if (!string.IsNullOrWhiteSpace(slot.Regex))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(
                            Normalize(request.Utterance), Normalize(slot.Regex),
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

                        if (match.Success)
                        {
                            var value = match.Groups["value"].Success ? match.Groups["value"].Value : match.Value;
                            dict[slot.Name] = new SlotValue { Name = slot.Name, Value = value, Confidence = 0.7, IsResolved = false };
                        }
                    }
                }
            }
            else
            {
                foreach (var slot in anchor.Slots)
                {
                    if (dict.ContainsKey(slot.Name)) continue;
                    if (!string.IsNullOrWhiteSpace(slot.Regex))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(
                            Normalize(request.Utterance), Normalize(slot.Regex),
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

                        if (match.Success)
                        {
                            var value = match.Groups["value"].Success ? match.Groups["value"].Value : match.Value;
                            //if (value.ToLowerInvariant() == "yes" || value.ToLowerInvariant() == "no")
                            //{
                            //    dict[state.CurrentStage.]
                            //}
                            dict[slot.Name] = new SlotValue { Name = slot.Name, Value = value, Confidence = 0.7, IsResolved = false };
                        }
                    }
                }
            }
            return dict;
        }
        string Normalize(string s) =>
            s.Replace('\u2019', '\'')        // curly ’ -> '
            .Replace('\u00A0', ' ')         // NBSP -> space
            .Trim();

    }
}
