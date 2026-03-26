using HealthcareBooking.API.Middlewares;
using HealthcareBooking.Core.Interfaces;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Core.Services;
using HealthcareBooking.Infrastructure.Data;
using HealthcareBooking.Infrastructure.Repositories;
using HealthcareBooking.Infrastructure.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace HealthcareBooking.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddOpenApi(options => options.AddHealthcareBookingMetadata());

        return services;
    }

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<DoctorService>();
        services.AddScoped<PatientService>();
        services.AddScoped<ClinicService>();
        services.AddScoped<AppointmentService>();
        services.AddScoped<BookService>();

        return services;
    }

    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = configuration["Redis:InstanceName"];
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    // 註冊 Hangfire 服務
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddHangfireServer();
        return services;
    }

    // 註冊假的通知服務實作
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, FakeEmailService>();
        return services;
    }
}
