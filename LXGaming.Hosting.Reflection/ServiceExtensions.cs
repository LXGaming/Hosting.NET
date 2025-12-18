using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LXGaming.Hosting.Reflection {

    public static class ServiceExtensions {

        /// <summary>
        /// Adds all services specified in <paramref name="assembly"/> to the specified
        /// <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The value of <paramref name="services"/>.</returns>
        public static IServiceCollection AddAllServices(this IServiceCollection services, Assembly assembly) {
            return services.AddAllServices(assembly.GetTypes().OrderBy(type => type.FullName));
        }

        /// <summary>
        /// Adds all services specified in <paramref name="types"/> to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="types">The types of the services to add.</param>
        /// <returns>The value of <paramref name="services"/>.</returns>
        public static IServiceCollection AddAllServices(this IServiceCollection services, IEnumerable<Type> types) {
            foreach (var type in types) {
                if (IsValid(type) && IsService(type)) {
                    services.AddServiceInternal(type);
                }
            }

            return services;
        }

        /// <summary>
        /// Adds the service specified in <typeparamref name="TService"/> to the specified
        /// <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <returns>The value of <paramref name="services"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type of <typeparamref name="TService"/> is not a class or is abstract.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the type of <typeparamref name="TService"/> is missing a service attribute.
        /// </exception>
        public static IServiceCollection AddService<TService>(this IServiceCollection services) where TService : class {
            return services.AddService(typeof(TService));
        }

        /// <summary>
        /// Adds the service specified in <paramref name="type"/> to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="type">The type of the service to add.</param>
        /// <returns>The value of <paramref name="services"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="type"/> is not a class or is abstract.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="type"/> is missing a service attribute.
        /// </exception>
        public static IServiceCollection AddService(this IServiceCollection services, Type type) {
            if (!IsValid(type)) {
                throw new ArgumentException($"'{type.FullName}' is not valid.", nameof(type));
            }

            if (!IsService(type)) {
                throw new ArgumentException($"'{type.FullName}' is not a service.", nameof(type));
            }

            return services.AddServiceInternal(type);
        }

        private static IServiceCollection AddServiceInternal(this IServiceCollection services, Type type) {
            var serviceAttributes = GetServiceAttributes(type);
            if (serviceAttributes.Length == 0) {
                throw new ArgumentException($"'{type.FullName}' is missing a service attribute.", nameof(type));
            }

            if (serviceAttributes.Length > 1) {
                throw new ArgumentException($"'{type.FullName}' is ambiguous.", nameof(type));
            }

            var serviceAttribute = serviceAttributes.Single();
#if NET8_0_OR_GREATER
            if (serviceAttribute is KeyedServiceAttribute keyedServiceAttribute) {
                var lifetime = keyedServiceAttribute.Lifetime;
                var serviceKey = keyedServiceAttribute.Key;
                var serviceType = keyedServiceAttribute.Type;

                if (IsHostedService(type)) {
                    if (lifetime != ServiceLifetime.Singleton) {
                        throw new InvalidOperationException($"Hosted service cannot have a {lifetime} lifetime.");
                    }

                    services.AddKeyedHostedService(type, serviceKey);
                    if (serviceType != null && serviceType != typeof(IHostedService)) {
                        services.AddKeyedSingleton(serviceType, serviceKey,
                            (provider, _) => provider.GetRequiredKeyedService(type, serviceKey));
                    }

                    return services;
                }

                services.Add(new ServiceDescriptor(type, serviceKey, type, lifetime));
                if (serviceType != null) {
                    services.Add(new ServiceDescriptor(serviceType, serviceKey,
                        (provider, key) => provider.GetRequiredKeyedService(type, key), lifetime));
                }
            }
            else
#endif
            {
                var lifetime = serviceAttribute.Lifetime;
                var serviceType = serviceAttribute.Type;

                if (IsHostedService(type)) {
                    if (lifetime != ServiceLifetime.Singleton) {
                        throw new InvalidOperationException($"Hosted service cannot have a {lifetime} lifetime.");
                    }

                    services.AddHostedService(type);
                    if (serviceType != null && serviceType != typeof(IHostedService)) {
                        services.AddSingleton(serviceType, provider => provider.GetRequiredService(type));
                    }

                    return services;
                }

                services.Add(new ServiceDescriptor(type, type, lifetime));
                if (serviceType != null) {
                    services.Add(new ServiceDescriptor(serviceType, provider => provider.GetRequiredService(type),
                        lifetime));
                }
            }

            return services;
        }

        private static IServiceCollection AddHostedService(this IServiceCollection services, Type type) {
            return services
                .AddSingleton(type, type)
                .AddSingleton(typeof(IHostedService), provider => provider.GetRequiredService(type));
        }

#if NET8_0_OR_GREATER
        private static IServiceCollection AddKeyedHostedService(this IServiceCollection services, Type type,
            object? serviceKey) {
            return services
                .AddKeyedSingleton(type, serviceKey, type)
                .AddKeyedSingleton(typeof(IHostedService), serviceKey,
                    (provider, key) => provider.GetRequiredKeyedService(type, key));
        }
#endif

        private static ServiceAttribute[] GetServiceAttributes(Type type) {
            return type.GetCustomAttributes(false)
                .Where(attribute => attribute is ServiceAttribute)
                .Cast<ServiceAttribute>()
                .ToArray();
        }

        private static bool IsHostedService(Type type) {
            return typeof(IHostedService).IsAssignableFrom(type);
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        private static bool IsService(Type type) {
            if (type.IsDefined(typeof(ServiceAttribute), false)) {
                return true;
            }

#if NET8_0_OR_GREATER
            if (type.IsDefined(typeof(KeyedServiceAttribute), false)) {
                return true;
            }
#endif

            return false;
        }

        private static bool IsValid(Type type) {
            return type is { IsClass: true, IsAbstract: false };
        }
    }
}