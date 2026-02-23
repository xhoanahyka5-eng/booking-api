using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Booking.Application.Behaviors;

namespace Booking.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ======================
        // MEDIATR
        // ======================
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(ApplicationServicesRegistration).Assembly
            );
        });

        // ======================
        // FLUENT VALIDATION
        // ======================
        services.AddValidatorsFromAssembly(
            typeof(ApplicationServicesRegistration).Assembly
        );

        // ======================
        // PIPELINE BEHAVIORS
        // ======================
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );

        return services;
    }
}