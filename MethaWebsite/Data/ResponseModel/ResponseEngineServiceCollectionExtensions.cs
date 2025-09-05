using Microsoft.DotNet.Scaffolding.Shared;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MethaWebsite.Data.ResponseModel
{
    public static class ResponseEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddResponseEngine(
            this IServiceCollection services,
            Action<ResponseEngineOptions>? configure = null)
        {
            var templates = JsonSerializer.Deserialize<List<Template>>(
                File.ReadAllText("Data/Config/templates.json"),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            services.AddOptions<ResponseEngineOptions>();
            if (configure is not null)
                services.Configure(configure);

            var path = Path.Combine("Data/Config", "slotDefinitions.json");
            var slotDefinitions = SlotDefinitionLoader.LoadFromJson(path);
            
            services.AddTransient<ListsHandler>();
            services.AddTransient<AccountHelpHandler>();
            services.AddTransient<PaymentsHelpHandler>();
            services.AddTransient<AddAddressHandler>();
            services.AddTransient<UpdateAddressHandler>();
            services.AddTransient<SlotActionBinder>();

            services.AddSingleton<IDictionary<string, SlotDefinition>>(slotDefinitions);
            services.AddSingleton<IDictionary<string, ISlotActionHandler>>(sp =>
            {
                return new Dictionary<string, ISlotActionHandler>
                {
                    ["listsHandler"] = sp.GetRequiredService<ListsHandler>(),
                    ["accountHelpHandler"] = sp.GetRequiredService<AccountHelpHandler>(),
                    ["paymentsHelpHandler"] = sp.GetRequiredService<PaymentsHelpHandler>(),
                    ["addAddressHandler"] = sp.GetRequiredService<AddAddressHandler>(),
                    ["updateAddressHandler"] = sp.GetRequiredService<UpdateAddressHandler>(),
                };
            });

            // Register the engine itself
            services.AddSingleton<ResponseEngine>();
            services.AddSingleton<IIntentAnchorProvider, JsonIntentAnchorProvider>();
            services.AddSingleton<ISlotExtractor, RegexSlotExtractor>();
            services.AddSingleton<ISlotResolver, SlotResolver>();
            services.AddSingleton<IResponseValidator, BasicResponseValidator>();
            services.AddSingleton<IClarificationStrategy, DefaultClarificationStrategy>();
            services.AddSingleton<ITemplateProvider>(new JsonTemplateProvider(templates));
            services.AddSingleton<ITemplateRenderer, SimpleTemplateRenderer>();

            services.AddSingleton<ICityCanonicalizer, CityCanonicalizer>(); // or Scoped, depending on usage
            services.AddSingleton<IReadOnlyDictionary<string, string>>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var dictSection = config.GetSection("IntentMappings"); // e.g. in appsettings.json
                var mappings = dictSection.GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
                return new ReadOnlyDictionary<string, string>(mappings);
            });

            services.AddSingleton<IReadOnlyDictionary<string, AnchorDefinition>>(provider =>
            {
                var env = provider.GetRequiredService<IHostEnvironment>();
                var path = Path.Combine(env.ContentRootPath, "Data/Config", "anchors.json");

                if (!File.Exists(path))
                    throw new FileNotFoundException($"Missing anchor file: {path}");

                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<Dictionary<string, AnchorDefinition>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new ReadOnlyDictionary<string, AnchorDefinition>(data ?? new());
            });


            return services;
        }
    }
}
