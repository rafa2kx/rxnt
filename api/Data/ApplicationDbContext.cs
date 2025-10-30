using Microsoft.EntityFrameworkCore;
using RXNT.API.Models;

namespace RXNT.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<BulkJobStatus> BulkJobStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.HasIndex(e => e.Email);
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialty).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.LicenseNumber);
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AppointmentTime).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.InvoiceNumber);
                
                entity.HasOne(i => i.Appointment)
                      .WithMany()
                      .HasForeignKey(i => i.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure BulkJobStatus entity
            modelBuilder.Entity<BulkJobStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.JobId).IsUnique();
                entity.Property(e => e.JobId).IsRequired().HasMaxLength(64);
                entity.Property(e => e.HangfireJobId).HasMaxLength(64);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(32);
                entity.Property(e => e.ErrorSummary).HasMaxLength(1024);
                entity.Property(e => e.SourceFilePath).HasMaxLength(512);
            });
        }
    }
}
