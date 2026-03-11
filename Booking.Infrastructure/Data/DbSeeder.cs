using Microsoft.EntityFrameworkCore;
using Booking.Domain.Entities.Roles;

namespace Booking.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BookingDbContext context, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        await EnsureRoleAsync(context, "Guest", "Default customer role", ct);
        await EnsureRoleAsync(context, "Host", "Property owner role", ct);
        await EnsureRoleAsync(context, "Admin", "Administrator role", ct);

        await context.SaveChangesAsync(ct);
    }

    private static async Task EnsureRoleAsync(
        BookingDbContext context,
        string name,
        string description,
        CancellationToken ct)
    {
        var exists = await context.Roles.AnyAsync(r => r.Name == name, ct);

        if (exists) return;

        context.Roles.Add(new Role
        {
            Name = name,
            Description = description
        });
    }
}