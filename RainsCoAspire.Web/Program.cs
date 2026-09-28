using Microsoft.Extensions.Http.Resilience;
using RainsCoAspire.Web;
using RainsCoAspire.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

// Replace the default resilience handler: uploading several images to S3 can exceed its 10 second
// attempt timeout, and retrying a POST/PUT could create duplicate rental units or images.
// RemoveAllResilienceHandlers is marked experimental but is the supported way to override the defaults.
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient<RentalUnitApiClient>(client =>
    {
        client.BaseAddress = new("https+http://apiservice");
        // Timeouts are enforced by the resilience handler below.
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.DisableForUnsafeHttpMethods();
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
        // Must be at least twice the attempt timeout.
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(4);
    });
#pragma warning restore EXTEXP0001

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
