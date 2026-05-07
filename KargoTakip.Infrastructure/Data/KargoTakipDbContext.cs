using KargoTakip.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Data
{
    public class KargoTakipDbContext:DbContext
    {
        public KargoTakipDbContext(DbContextOptions<KargoTakipDbContext> options)
           : base(options) { }

        public DbSet<City> Cities { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<TransferRequestItem> TransferRequestItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TransferRequest — Branch ilişkisi iki FK olduğu için elle tanımlanmalı
            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.RequesterBranch)
                .WithMany()
                .HasForeignKey(t => t.RequesterBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.TargetBranch)
                .WithMany()
                .HasForeignKey(t => t.TargetBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // TransferRequest — User ilişkisi iki FK olduğu için elle tanımlanmalı
            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.RequestedByUser)
                .WithMany()
                .HasForeignKey(t => t.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.RespondedByUser)
                .WithMany()
                .HasForeignKey(t => t.RespondedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shipment — birden fazla FK olduğu için cascade sorun çıkarır
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.ReceiverCity)
                .WithMany(c => c.Shipments)
                .HasForeignKey(s => s.ReceiverCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.Weight)
                .HasPrecision(18, 2);
            // Vehicles ilişkileri
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Branch)
                .WithMany(b => b.Vehicles)
                .HasForeignKey(v => v.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.City)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shipment ilişkileri
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Branch)
                .WithMany(b => b.Shipments)
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.AssignedVehicle)
                .WithMany(v => v.Shipments)
                .HasForeignKey(s => s.AssignedVehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShipmentStatusHistory ilişkileri
            modelBuilder.Entity<ShipmentStatusHistory>()
                .HasOne(s => s.Shipment)
                .WithMany(s => s.StatusHistories)
                .HasForeignKey(s => s.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShipmentStatusHistory>()
                .HasOne(s => s.ChangedByUser)
                .WithMany()
                .HasForeignKey(s => s.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification ilişkileri
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Branch)
                .WithMany()
                .HasForeignKey(n => n.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Shipment)
                .WithMany(s => s.Notifications)
                .HasForeignKey(n => n.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TransferRequest)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TransferRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // TransferRequestItem ilişkileri
            modelBuilder.Entity<TransferRequestItem>()
                .HasOne(t => t.Shipment)
                .WithMany(s => s.TransferRequestItems)
                .HasForeignKey(t => t.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferRequestItem>()
                .HasOne(t => t.TransferRequest)
                .WithMany(t => t.Items)
                .HasForeignKey(t => t.TransferRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // User — Branch ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
