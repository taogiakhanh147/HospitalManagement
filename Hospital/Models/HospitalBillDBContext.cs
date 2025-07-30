using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Hospital.Models
{
    public partial class HospitalBillDBContext : DbContext
    {
        public HospitalBillDBContext()
        {
        }

        public HospitalBillDBContext(DbContextOptions<HospitalBillDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Diagnosis> Diagnoses { get; set; } = null!;
        public virtual DbSet<Invoice> Invoices { get; set; } = null!;
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; } = null!;
        public virtual DbSet<InvoiceStatus> InvoiceStatuses { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<Patient> Patients { get; set; } = null!;
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<staff> staff { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DESKTOP-1ROJLG2\\BAITAP;Database=HospitalBillDB;User Id=sa;Password=123;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Diagnosis>(entity =>
            {
                entity.ToTable("Diagnosis");
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoice");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.RestoredAt).HasColumnType("datetime");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Cashier)
                    .WithMany(p => p.InvoiceCashiers)
                    .HasForeignKey(d => d.CashierId)
                    .HasConstraintName("FK__Invoice__Cashier__34C8D9D1");

                entity.HasOne(d => d.Diagnosis)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.DiagnosisId)
                    .HasConstraintName("FK__Invoice__Diagnos__38996AB5");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.InvoiceDoctors)
                    .HasForeignKey(d => d.DoctorId)
                    .HasConstraintName("FK__Invoice__DoctorI__33D4B598");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK__Invoice__Active__32E0915F");

                entity.HasOne(d => d.PaymentMethod)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.PaymentMethodId)
                    .HasConstraintName("FK__Invoice__Payment__37A5467C");

                entity.HasOne(d => d.Pharmacist)
                    .WithMany(p => p.InvoicePharmacists)
                    .HasForeignKey(d => d.PharmacistId)
                    .HasConstraintName("FK__Invoice__Pharmac__35BCFE0A");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Invoice__StatusI__36B12243");
            });

            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.ToTable("InvoiceDetail");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("FK__InvoiceDe__Invoi__3D5E1FD2");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK__InvoiceDe__ItemI__3E52440B");
            });

            modelBuilder.Entity<InvoiceStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId)
                    .HasName("PK__InvoiceS__C8EE2063B08FC8CC");

                entity.ToTable("InvoiceStatus");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Item");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patient");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethod");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
            });

            modelBuilder.Entity<staff>(entity =>
            {
                entity.ToTable("Staff");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.staff)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Staff__RoleId__286302EC");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
