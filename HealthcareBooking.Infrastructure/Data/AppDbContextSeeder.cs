using HealthcareBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Data;

public static class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedDoctorsAsync(context);
        await SeedPatientsAsync(context);
        await SeedClinicsAsync(context);
        await SeedAppointmentsAsync(context);
    }

    public static async Task ResetAndSeedAsync(AppDbContext context)
    {
        // 依照外鍵相依的反向順序清空，避免 FK 衝突
        await context.Appointments.ExecuteDeleteAsync();
        await context.Clinics.ExecuteDeleteAsync();
        await context.Patients.ExecuteDeleteAsync();
        await context.Doctors.ExecuteDeleteAsync();

        await SeedDoctorsAsync(context);
        await SeedPatientsAsync(context);
        await SeedClinicsAsync(context);
        await SeedAppointmentsAsync(context);
    }

    private static async Task SeedDoctorsAsync(AppDbContext context)
    {
        if (await context.Doctors.AnyAsync())
            return;

        await context.Doctors.AddRangeAsync(
            new("王大明"),
            new("李小華"),
            new("陳美玲")
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPatientsAsync(AppDbContext context)
    {
        if (await context.Patients.AnyAsync())
            return;

        await context.Patients.AddRangeAsync(
            new() { Name = "張三" },
            new() { Name = "李四" },
            new() { Name = "王五" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedClinicsAsync(AppDbContext context)
    {
        if (await context.Clinics.AnyAsync())
            return;

        var doctors = await context.Doctors.ToListAsync();
        var today = DateTime.Today;

        await context.Clinics.AddRangeAsync(
            new(doctors[0].Id, today.AddDays(1).AddHours(9),  20),
            new(doctors[0].Id, today.AddDays(2).AddHours(14), 15),
            new(doctors[1].Id, today.AddDays(1).AddHours(10), 10),
            new(doctors[2].Id, today.AddDays(3).AddHours(9),  25)
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedAppointmentsAsync(AppDbContext context)
    {
        if (await context.Appointments.AnyAsync())
            return;

        var patients = await context.Patients.ToListAsync();
        var clinics  = await context.Clinics.ToListAsync();
        var today    = DateTime.Today;

        await context.Appointments.AddRangeAsync(
            new() { PatientId = patients[0].Id, ClinicId = clinics[0].Id, AppointmentDate = today.AddDays(1) },
            new() { PatientId = patients[1].Id, ClinicId = clinics[0].Id, AppointmentDate = today.AddDays(1) },
            new() { PatientId = patients[2].Id, ClinicId = clinics[2].Id, AppointmentDate = today.AddDays(1) }
        );
        clinics[0].AddBooking();
        clinics[0].AddBooking();
        clinics[2].AddBooking();
        await context.SaveChangesAsync();
    }
}
