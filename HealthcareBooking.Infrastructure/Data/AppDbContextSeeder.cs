using HealthcareBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Data;

public static class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Doctors.AnyAsync())
            return;

        var doctors = new List<Doctor>
        {
            new("王大明"),
            new("李小華"),
            new("陳美玲"),
        };

        await context.Doctors.AddRangeAsync(doctors);
        await context.SaveChangesAsync();

        var patients = new List<Patient>
        {
            new() { Name = "張三" },
            new() { Name = "李四" },
            new() { Name = "王五" },
        };

        await context.Patients.AddRangeAsync(patients);
        await context.SaveChangesAsync();

        var today = DateTime.Today;
        var clinics = new List<Clinic>
        {
            new(doctors[0].Id, today.AddDays(1).AddHours(9),  20),
            new(doctors[0].Id, today.AddDays(2).AddHours(14), 15),
            new(doctors[1].Id, today.AddDays(1).AddHours(10), 10),
            new(doctors[2].Id, today.AddDays(3).AddHours(9),  25),
        };

        await context.Clinics.AddRangeAsync(clinics);
        await context.SaveChangesAsync();

        var appointments = new List<Appointment>
        {
            new() { PatientId = patients[0].Id, ClinicId = clinics[0].Id, AppointmentDate = today.AddDays(1) },
            new() { PatientId = patients[1].Id, ClinicId = clinics[0].Id, AppointmentDate = today.AddDays(1) },
            new() { PatientId = patients[2].Id, ClinicId = clinics[2].Id, AppointmentDate = today.AddDays(1) },
        };

        await context.Appointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();
    }
}
