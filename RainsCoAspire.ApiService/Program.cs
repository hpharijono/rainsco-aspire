using System.Globalization;
using Amazon.Runtime;
using Amazon.S3;
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

// AWS S3 for rental unit images. Access keys are read from appsettings for now; when they're absent the
// SDK falls back to its default credential chain (environment variables, AWS profile, IAM role).
var awsOptions = builder.Configuration.GetAWSOptions();
var awsAccessKeyId = builder.Configuration["AWS:AccessKeyId"];
var awsSecretAccessKey = builder.Configuration["AWS:SecretAccessKey"];
if (!string.IsNullOrWhiteSpace(awsAccessKeyId) && !string.IsNullOrWhiteSpace(awsSecretAccessKey))
{
    awsOptions.Credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
}
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();
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
