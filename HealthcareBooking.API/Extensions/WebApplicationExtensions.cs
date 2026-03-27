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
        app.UseStaticFiles(); // 啟用 wwwroot 靜態檔案支援
        app.MapControllers();
        app.MapSignalRHubs();

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

    // SignalR 的 Hub 需要在這裡註冊，才能在 Controller 中注入 IHubContext<ClinicHub> 使用
    public static WebApplication MapSignalRHubs(this WebApplication app)
    {
        app.MapHub<Hubs.ClinicHub>("/clinicHub");
        return app;
    }

}
