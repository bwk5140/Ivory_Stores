namespace MethaWebsite.Data.ResponseModel
{
    public interface ITemplateProvider
    {
        IReadOnlyList<Template> GetTemplates(string anchorId, string? locale);
    }
}
