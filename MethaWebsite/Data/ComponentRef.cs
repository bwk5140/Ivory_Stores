using Microsoft.AspNetCore.Components;

namespace MethaWebsite.Data
{
    public class ComponentRef
    {
        public string? Id { get; set; }
        public IComponent? Instance { get; set; }
    }
}
