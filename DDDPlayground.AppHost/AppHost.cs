var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database for DDD Playground
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var database = postgres.AddDatabase("dddplaygrounddb");

// Add Redis cache
var cache = builder.AddRedis("cache");

// Add API service with database and cache dependencies
var apiService = builder.AddProject<Projects.DDDPlayground_ApiService>("apiservice")
    .WithReference(database)
    .WithReference(cache)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
