using Hangfire;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace HealthcareBooking.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.UseHangfireDashboard("/hangfire");
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await AppDbContextSeeder.SeedAsync(context);
    }
}
