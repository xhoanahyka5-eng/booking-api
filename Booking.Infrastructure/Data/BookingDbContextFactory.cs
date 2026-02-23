using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Booking.Infrastructure.Data;

public class BookingDbContextFactory
    : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(basePath, "../Booking.Api"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<BookingDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"));

        return new BookingDbContext(optionsBuilder.Options);
    }
}