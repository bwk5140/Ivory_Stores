using MethaWebsite.Components;
using MethaWebsite.Components.Account;
using MethaWebsite.Data;
using MethaWebsite.Data.Contexts;
using MethaWebsite.Data.ResponseModel;
using MethaWebsite.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System.Globalization;
using System.Security.Cryptography;


var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);


builder.Services.AddQuickGridEntityFrameworkAdapter();

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
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
builder.Services.AddScoped<ProductRatingService>();
builder.Services.AddScoped<SearchEngineService>();
builder.Services.AddScoped<LocalEmbeddingService>();
builder.Services.AddScoped<TemplateResponseProvider>();

//builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//    })
//    .AddIdentityCookies();

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

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44338/") });
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<FilterStateService>();
builder.Services.AddSingleton<ShoppingCartService>();
builder.Services.AddSingleton<ChatController>();
builder.Services.AddSingleton<StateChangeService>();
builder.Services.AddSingleton<CardSetupSevice>();
builder.Services.AddSingleton<ApplicationUserService>();
builder.Services.AddSingleton<EmailEncryptor>();
builder.Services.AddSingleton<ShippingCalculator>();
builder.Services.AddSingleton<MethaWebsite.Services.CheckoutService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<OTPGenerator>();
builder.Services.AddSingleton<TrackingService>();
builder.Services.AddSingleton<TrainingService>();
builder.Services.AddSingleton<MlPredictionService>();
builder.Services.AddSingleton<WorldClockService>();
builder.Services.AddSingleton<CookieReaderService>();
builder.Services.AddSingleton<SlotFillerRegistry>();
builder.Services.AddSingleton<EntityRecognizer>();
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<ConversationManager>();
builder.Services.AddSingleton<ConversationContext>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<ISlotFiller, DateTimeSlotFiller>();
builder.Services.AddSingleton<IIntentRecognizer, SimpleIntentRecognizer>();
builder.Services.AddSingleton<PersistentConversationStateStore>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
builder.Services.AddSingleton<IConversationStateStore>(provider =>
{
    var cache = provider.GetRequiredService<IMemoryCache>();
    var fallback = provider.GetRequiredService<PersistentConversationStateStore>();
    var duration = TimeSpan.FromMinutes(10); // or pull from config

    return new CachedConversationStateStore(cache, fallback, duration);
});


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IConversationIdProvider, ConversationIdProvider>();
builder.Services.AddResponseEngine(opts =>
{
    opts.GlobalMinIntentConfidence = 0.5;
    opts.LogSlotValues = false;
    opts.MissingAnchorLogLevel = LogLevel.Warning;
    opts.LowConfidenceFallbackText = "Could you clarify what you need?";
});

builder.Services.AddSingleton(provider =>
{
    Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
    return new PaymentIntentService();
});
builder.Services.AddHttpClient<SMS_Service>();
builder.Services.AddHttpClient<MpesaService>();

builder.Services.AddLocalization();
builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("StripeAPIKey");
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthentication()
   .AddCookie()
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection =
       builder.Configuration.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
       options.SignInScheme = IdentityConstants.ExternalScheme;
       options.AdditionalAuthorizationParameters.Add("prompt", "select_account");
   })
    .AddMicrosoftAccount(microsoftOptions =>
    {
        IConfigurationSection microsoftAuthNSection =
        builder.Configuration.GetSection("Authentication:Microsoft");
        microsoftOptions.ClientId = microsoftAuthNSection["ClientId"];
        microsoftOptions.ClientSecret = microsoftAuthNSection["ClientSecret"];
        microsoftOptions.CallbackPath = "/signin-oidc";
    });


var app = builder.Build();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

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
}
app.MapHub<ChatHub>("/chathub");

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
//app.MapPost("/respond", ([FromBody] RespondRequest req,
//    [FromServices] ResponseEngine engine) =>
//{
//    var res = engine.GenerateResponse(req.Action, req.Request);
//    return Results.Ok(res);
//});
app.UseAntiforgery();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.TryGetValue("device_id", out var deviceId))
    {
        deviceId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Response.Cookies.Append("device_id", deviceId, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        });
    }

    // Attach deviceId to context for downstream use
    context.Items["DeviceId"] = deviceId;

    await next();
});
var supportedCultures = new[] { "en-KE", "en-US", "en-GB", "es-US", "es-ES", "fr-FR", "fr-CA", "ar-SA", "zh-Hant", "de-DE", "ja-JP", "it-IT", "sw-KE" };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-KE"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};
app.MapPost("/api/stripe/charge-saved-card", async ([FromBody] ChargeRequest req, IConfiguration config) =>
{
    var apiKey = config["Stripe:SecretKey"];
    var client = new StripeClient(apiKey);
    var paymentIntentService = new PaymentIntentService(client);

    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(req.Amount * 100),
        Currency = "KES",
        Customer = req.CustomerId,
        PaymentMethod = req.PaymentMethodId,
        Confirm = true,
        OffSession = true,
        ReturnUrl = "https://Account/OrderComplete"
    };

    try
    {
        var paymentIntent = await paymentIntentService.CreateAsync(options);
        return Results.Ok(new
        {
            Status = paymentIntent.Status,
            Id = paymentIntent.Id,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency
        });
    }
    catch (StripeException ex)
    {
        return Results.Problem(detail: ex.Message);
    }
});


localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);


app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapRazorPages();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
