var builder = DistributedApplication.CreateBuilder(args);

// Existing SQL Server database; the connection string is read from ConnectionStrings:RainsApptAndCoworking.
var rainsDb = builder.AddConnectionString("RainsApptAndCoworking");

var apiService = builder.AddProject<Projects.RainsCoAspire_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rainsDb);

builder.AddProject<Projects.RainsCoAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
