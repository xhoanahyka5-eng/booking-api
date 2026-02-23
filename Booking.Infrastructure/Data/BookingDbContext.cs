using Microsoft.EntityFrameworkCore;

using Booking.Domain.Entities.Users;
using Booking.Domain.Entities.Roles;
using Booking.Domain.Entities.UserRoles;
using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.Reviews;

using PropertyEntity = Booking.Domain.Entities.Properties.Property;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----------------------------
        // USER: Id GUID gjenerohet kur krijohet (opsionale por e mire)
        // ----------------------------
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // ----------------------------
        // USER EMAIL UNIQUE
        // ----------------------------
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ----------------------------
        // USER <-> ROLE (many-to-many me tabelë lidhëse UserRoles)
        // PK = (UserId, RoleId)
        // ----------------------------
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

        // ----------------------------
        // USER <-> OWNERPROFILE (one-to-one, PK=FK)
        // ----------------------------
        modelBuilder.Entity<OwnerProfile>(entity =>
        {
            entity.HasKey(op => op.UserId);

            entity.HasOne(op => op.User)
                  .WithOne(u => u.OwnerProfile)
                  .HasForeignKey<OwnerProfile>(op => op.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ----------------------------
        // PROPERTY -> OWNER (User)  (1 User : shume Properties)
        // ----------------------------
        modelBuilder.Entity<PropertyEntity>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // PROPERTY -> ADDRESS (shume Properties : 1 Address)
        // ✅ Kjo është më korrekt se 1-1
        // ----------------------------
        modelBuilder.Entity<PropertyEntity>()
            .HasOne<Address>()
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // BOOKING -> PROPERTY (1 Property : shume Bookings)
        // ----------------------------
        modelBuilder.Entity<BookingEntity>()
            .HasOne<PropertyEntity>()
            .WithMany()
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // BOOKING -> GUEST (User) (1 User : shume Bookings)
        // ----------------------------
        modelBuilder.Entity<BookingEntity>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // REVIEW -> BOOKING (1 Booking : shume Reviews)
        // ----------------------------
        modelBuilder.Entity<Review>()
            .HasOne<BookingEntity>()
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // ----------------------------
        // REVIEW -> GUEST (User) (1 User : shume Reviews)
        // ----------------------------
        modelBuilder.Entity<Review>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // BOOKING – DECIMAL PRECISION (heq warnings)
        // ----------------------------
        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.Property(b => b.CleaningFee).HasPrecision(18, 2);
            entity.Property(b => b.AmenitiesUpCharge).HasPrecision(18, 2);
            entity.Property(b => b.PriceForPeriod).HasPrecision(18, 2);
            entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
        });
    }
}
