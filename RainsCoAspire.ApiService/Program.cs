using System.Globalization;
using Amazon.S3;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Localization;
using RainsCoAspire.ApiService.Data;
using RainsCoAspire.ApiService.Options;
using RainsCoAspire.ApiService.Repositories;
using RainsCoAspire.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddSqlServerDbContext<AppDbContext>("RainsApptAndCoworking");

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// AWS: S3 stores rental unit images; Secrets Manager holds the bucket name. No access keys live in code or
// config. Like `new AmazonS3Client()`, the clients use the SDK's default credential chain, which reads the
// [default] profile from ~/.aws/credentials (or an IAM role when hosted on AWS). AWS:Region is optional; when
// empty, the region comes from ~/.aws/config or the AWS_REGION environment variable.
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISecretsProvider, AwsSecretsManagerProvider>();
builder.Services.Configure<S3StorageOptions>(builder.Configuration.GetSection(S3StorageOptions.SectionName));

builder.Services.AddScoped<IRentalUnitRepository, RentalUnitRepository>();
builder.Services.AddScoped<IImageStorageService, S3ImageStorageService>();
builder.Services.AddScoped<IRentalUnitService, RentalUnitService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Parse form values (e.g. decimal square meters) the same way regardless of the server's OS culture.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = [CultureInfo.InvariantCulture],
    SupportedUICultures = [CultureInfo.InvariantCulture],
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
