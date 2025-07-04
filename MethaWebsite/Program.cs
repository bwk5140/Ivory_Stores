using Google.Apis.Auth.AspNetCore3;
using MethaWebsite.Components;
using MethaWebsite.Components.Account;
using MethaWebsite.Data;
using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddQuickGridEntityFrameworkAdapter();

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<RatingFilterService>();
builder.Services.AddScoped<LayoutRefreshService>();
builder.Services.AddScoped<ShippingService>();
builder.Services.AddScoped<LayoutState>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44338/") });

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection1") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<FilterStateService>();
builder.Services.AddSingleton<ApplicationUserService>();
builder.Services.AddSingleton<ShoppingCartService>();
builder.Services.AddSingleton<StateChangeService>();
builder.Services.AddSingleton<CardSetupSevice>();
builder.Services.AddSingleton<MethaWebsite.Services.CheckoutService>();
builder.Services.AddSingleton(provider =>
{
    Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
    return new PaymentIntentService();
});
builder.Services.AddHttpClient<SMS_Service>();
builder.Services.AddHttpClient<MpesaService>();

builder.Services.AddLocalization();
builder.Services.AddControllers();

StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("StripeAPIKey");

//builder.Services.AddAuthentication()
//   .AddGoogle(options =>
//   {
//       IConfigurationSection googleAuthNSection =
//       config.GetSection("Authentication:Google");
//       options.ClientId = googleAuthNSection["ClientId"];
//       options.ClientSecret = googleAuthNSection["ClientSecret"];
//   })
//   .AddMicrosoftAccount(microsoftOptions =>
//   {
//       microsoftOptions.ClientId = config["Authentication:Microsoft:ClientId"];
//       microsoftOptions.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
//   });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseForwardedHeaders();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
    app.UseHttpsRedirection();
}

app.MapPost("/api/stripe/create-setup-intent", async (
    [Microsoft.AspNetCore.Mvc.FromBody] SetupIntentRequest req,
    CardSetupSevice CardService) =>
{
    var intent = await CardService.CreateSetupIntentAsync(req.CustomerId);
    return Results.Ok(new { ClientSecret = intent.ClientSecret });
});
app.MapPost("/api/stripe/get-card-info", async (
    [Microsoft.AspNetCore.Mvc.FromBody] PaymentMethodRequest req,
    CardSetupSevice stripe) =>
{
    var cardInfo = await stripe.GetSavedCardDetailsAsync(req.PaymentMethodId);
    return Results.Ok(cardInfo);
});

app.UseAntiforgery();

var supportedCultures = new[] { "en-KE", "en-US", "en-GB", "es-US", "es-ES", "fr-FR", "fr-CA", "ar-SA", "zh-Hant", "de-DE", "ja-JP", "it-IT", "sw-KE" };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-KE"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};
app.MapPost("/api/stripe/charge-saved-card", async ([Microsoft.AspNetCore.Mvc.FromBody] ChargeRequest req) =>
{
    var paymentIntentService = new PaymentIntentService();

    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(req.Amount * 100), // Stripe works in cents
        Currency = "KES",
        Customer = req.CustomerId,
        PaymentMethod = req.PaymentMethodId,
        Confirm = true,
        OffSession = true,
        ReturnUrl = "https://Account/OrderComplete"
    };

    var paymentIntent = await paymentIntentService.CreateAsync(options);

    return Results.Ok(new
    {
        Status = paymentIntent.Status,
        Id = paymentIntent.Id,
        Amount = paymentIntent.Amount,
        Currency = paymentIntent.Currency
    });
});


localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);


app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
