using Microsoft.EntityFrameworkCore;
using RentCarApi.models;

namespace RentCarApi.data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<FavouriteCar> FavoriteCars { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.PhoneNumber);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();

                entity.HasMany(e => e.OwnedCars)
                    .WithOne(c => c.Owner)
                    .HasForeignKey(c => c.OwnerPhoneNumber)
                    .OnDelete(DeleteBehavior.SetNull); // Owner deleted, car remains unowned

                entity.HasMany(e => e.FavoriteCars)
                    .WithOne(fc => fc.User)
                    .HasForeignKey(fc => fc.UserPhoneNumber)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Rentals)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserPhoneNumber)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Messages)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserPhoneNumber)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Car configuration
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ImageUrls).HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                ); // Store List<string> as comma-separated string
            });

            // Rental configuration
            modelBuilder.Entity<Rental>(entity =>
            {
                entity.HasOne(r => r.Car)
                    .WithMany(c => c.Rentals)
                    .HasForeignKey(r => r.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FavoriteCar configuration
            modelBuilder.Entity<FavouriteCar>(entity =>
            {
                entity.HasOne(fc => fc.Car)
                    .WithMany(c => c.FavoritedByUsers)
                    .HasForeignKey(fc => fc.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Message configuration (no extra config needed)
            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);
        }
    }
}