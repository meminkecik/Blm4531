using Microsoft.EntityFrameworkCore;
using Nearest.Models;
using Nearest.Models.Address;

namespace Nearest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<TowTruck> TowTrucks { get; set; }
        public DbSet<TowTruckArea> TowTruckAreas { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AbuseReport> AbuseReports { get; set; }
        
        // Address entities
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<CityDistrict> CityDistricts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Company entity configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
            });

            // Admin entity configuration
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Ticket entity configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasOne(t => t.Company)
                      .WithMany(c => c.Tickets)
                      .HasForeignKey(t => t.CompanyId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TowTruck entity configuration
            modelBuilder.Entity<TowTruck>(entity =>
            {
                entity.HasOne(t => t.Company)
                      .WithMany()
                      .HasForeignKey(t => t.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Plaka sistem genelinde benzersiz
                entity.HasIndex(t => t.LicensePlate).IsUnique();
            });

            modelBuilder.Entity<TowTruckArea>(entity =>
            {
                entity.HasOne(a => a.TowTruck)
                      .WithMany(t => t.OperatingAreas)
                      .HasForeignKey(a => a.TowTruckId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserLocation entity configuration
            modelBuilder.Entity<UserLocation>(entity =>
            {
                entity.HasIndex(e => e.SessionId);
            });

            // Address entities configuration
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasIndex(e => e.ProvinceId).IsUnique();
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.HasIndex(e => e.DistrictId).IsUnique();
            });

            modelBuilder.Entity<CityDistrict>(entity =>
            {
                entity.HasOne(cd => cd.City)
                      .WithMany(c => c.Districts)
                      .HasForeignKey(cd => cd.CityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cd => cd.District)
                      .WithMany(d => d.Cities)
                      .HasForeignKey(cd => cd.DistrictId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Review entity configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.TowTruck)
                      .WithMany()
                      .HasForeignKey(r => r.TowTruckId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => r.TowTruckId);
                entity.HasIndex(r => r.ReviewerPhone);
            });

            // AbuseReport entity configuration
            modelBuilder.Entity<AbuseReport>(entity =>
            {
                entity.HasOne(a => a.TowTruck)
                      .WithMany()
                      .HasForeignKey(a => a.TowTruckId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Company)
                      .WithMany()
                      .HasForeignKey(a => a.CompanyId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.ReviewedByAdmin)
                      .WithMany()
                      .HasForeignKey(a => a.ReviewedByAdminId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => a.CreatedAt);
            });
        }
    }
}
