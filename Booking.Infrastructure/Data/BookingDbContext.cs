using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.Properties;
using Booking.Domain.Entities.Reviews;
using Booking.Domain.Entities.Roles;
using Booking.Domain.Entities.UserRoles;
using Booking.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;
using PropertyEntity = Booking.Domain.Entities.Properties.Property;

namespace Booking.Infrastructure.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();
    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<PropertyEntity> Properties => Set<PropertyEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<PropertyAvailability> PropertyAvailabilities => Set<PropertyAvailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OwnerProfile>(entity =>
        {
            entity.HasKey(op => op.UserId);

            entity.HasOne(op => op.User)
                  .WithOne(u => u.OwnerProfile)
                  .HasForeignKey<OwnerProfile>(op => op.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyEntity>()
            .HasOne(p => p.Address)
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PropertyAvailability>(entity =>
        {
            entity.Property(pa => pa.Price)
                  .HasPrecision(18, 2);

            entity.HasOne(pa => pa.Property)
                  .WithMany(p => p.Availabilities)
                  .HasForeignKey(pa => pa.PropertyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.PropertyId, a.Date })
                  .IsUnique();
        });

        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.HasOne<PropertyEntity>()
                  .WithMany()
                  .HasForeignKey(b => b.PropertyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(b => b.GuestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(b => b.CleaningFee)
                  .HasPrecision(18, 2);

            entity.Property(b => b.AmenitiesUpCharge)
                  .HasPrecision(18, 2);

            entity.Property(b => b.PriceForPeriod)
                  .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Review>()
            .HasOne<BookingEntity>()
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}