using System.Text.Json;
using BlueMeter.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replace a registered singleton service with a new instance.
    /// </summary>
    public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services, TService newInstance)
        where TService : class
    {
        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        services.AddSingleton(newInstance);
        return services;
    }

    /// <summary>
    /// Replace a registered singleton service with a new factory.
    /// </summary>
    public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Configure JSON serializer options with custom converters for the application.
    /// </summary>
    /// <remarks>
    /// The WPF version also wired a <c>ModifierKeysTypeConverter</c> via
    /// <see cref="System.ComponentModel.TypeDescriptor"/> so XAML bindings could parse
    /// modifier strings. Avalonia bindings don't need that hook, so it's gone.
    /// </remarks>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.WriteIndented = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            // Add KeyBinding JSON converter to properly handle Key.None serialization
            options.Converters.Add(new KeyBindingJsonConverter());
        });

        return services;
    }
}
