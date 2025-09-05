using System.Text.Json;

namespace MethaWebsite.Data.ResponseModel
{
    public static class SlotDefinitionLoader
    {
        public static IDictionary<string, SlotDefinition> LoadFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var slots = JsonSerializer.Deserialize<List<SlotDefinition>>(json);
            return slots.ToDictionary(s => s.Name, s => s);
        }
    }
}
