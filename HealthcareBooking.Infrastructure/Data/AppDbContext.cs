using System.Numerics;
using System.Reflection.Emit;
using HealthcareBooking.Core.Entities; // 記得引入你的 Entities
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // 建構子：接收外部傳入的連線字串等設定
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet 代表資料庫裡的「資料表」
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Clinic> Clinics { get; set; }

    // 覆寫 OnModelCreating，這裡就是我們寫 Fluent API 的地方
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. 設定 Appointment 與 Patient 的一對多關聯
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)           // 一筆 Appointment 屬於一個 Patient
            .WithMany(p => p.Appointments)       // 一個 Patient 有多筆 Appointments
            .HasForeignKey(a => a.PatientId)  // 指定外來鍵是 PatientId
            .OnDelete(DeleteBehavior.Restrict);         // 【關鍵！】拒絕連動刪除

        // 2. 設定 Appointment 與 Clinic 的一對多關聯
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Clinic)           // 一筆 Appointment 屬於一個 Clinic
            .WithMany(c => c.Appointments)       // 一個 Clinic 有多筆 Appointments
            .HasForeignKey(a => a.ClinicId)  // 指定外來鍵是 ClinicId
            .OnDelete(DeleteBehavior.Restrict);        // 【關鍵！】拒絕連動刪除

        // 3. 設定 Clinic 與 Doctor 的一對多關聯
        modelBuilder.Entity<Clinic>()
            .HasOne(c => c.Doctor)           // 一筆 Clinic 屬於一個 Doctor
            .WithMany(d => d.Clinics)       // 一個 Doctor 有多筆 Clinics
            .HasForeignKey(c => c.DoctorId)  // 指定外來鍵是 DoctorId
            .OnDelete(DeleteBehavior.Restrict);        // 【關鍵！】拒絕連動刪除

        // 4. 設定 Clinic 的 RowVersion 為樂觀鎖版本欄位
        modelBuilder.Entity<Clinic>()
            .Property(c => c.RowVersion)
            .IsRowVersion(); // 設定 RowVersion 為樂觀鎖版本欄位
    }
}