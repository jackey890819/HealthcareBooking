using HealthcareBooking.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddDatabaseServices(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();
await app.SeedDatabaseAsync();
app.UseApiPipeline();
app.Run();
