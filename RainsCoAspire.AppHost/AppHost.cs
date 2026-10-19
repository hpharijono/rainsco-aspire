var builder = DistributedApplication.CreateBuilder(args);

// Existing SQL Server database; the connection string is read from ConnectionStrings:RainsApptAndCoworking.
var rainsDb = builder.AddConnectionString("RainsApptAndCoworking");

// Key the API signs its JWTs with. Generated on the first run and saved to this project's user secrets
// (Parameters:jwt-signing-key), so tokens stay valid across restarts.
var jwtSigningKey = builder.AddParameter(
    "jwt-signing-key",
    new GenerateParameterDefault { MinLength = 64, Special = false },
    secret: true,
    persist: true);

var apiService = builder.AddProject<Projects.RainsCoAspire_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rainsDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey);

builder.AddProject<Projects.RainsCoAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
