using System.Reflection;
using AutoMapper;
using Booking.Application.Behaviors;
using Booking.Application.Features.Bookings.CreateBooking;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(ApplicationServicesRegistration).Assembly;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient<IRequestHandler<CreateBookingCommand, int>, CreateBookingCommandHandler>();

        return services;
    }
}