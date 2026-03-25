using HealthcareBooking.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddDatabaseServices(builder.Configuration)
    .AddApplicationServices()
    .AddCacheServices(builder.Configuration)
    .AddHangfireServices(builder.Configuration);

var app = builder.Build();
await app.SeedDatabaseAsync();
app.UseApiPipeline();
app.Run();
