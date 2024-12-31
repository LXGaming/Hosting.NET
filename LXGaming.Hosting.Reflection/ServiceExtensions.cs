using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LXGaming.Hosting.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LXGaming.Hosting.Reflection {

    public static class ServiceExtensions {

        public static IServiceCollection AddAllServices(this IServiceCollection services, Assembly assembly) {
            return services.AddAllServices(assembly.GetTypes().OrderBy(type => type.FullName));
        }

        public static IServiceCollection AddAllServices(this IServiceCollection services, IEnumerable<Type> types) {
            foreach (var type in types) {
                if (IsValid(type) && IsService(type)) {
                    services.AddServiceInternal(type);
                }
            }

            return services;
        }

        public static IServiceCollection AddService<TService>(this IServiceCollection services) where TService : class {
            return services.AddService(typeof(TService));
        }

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
            } else {
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

        private static IServiceCollection AddKeyedHostedService(this IServiceCollection services, Type type,
            object? serviceKey) {
            return services
                .AddKeyedSingleton(type, serviceKey, type)
                .AddKeyedSingleton(typeof(IHostedService), serviceKey,
                    (provider, key) => provider.GetRequiredKeyedService(type, key));
        }

        private static ServiceAttribute[] GetServiceAttributes(Type type) {
            return type.GetCustomAttributes(false)
                .Where(attribute => attribute is ServiceAttribute)
                .Cast<ServiceAttribute>()
                .ToArray();
        }

        private static bool IsHostedService(Type type) {
            return typeof(IHostedService).IsAssignableFrom(type);
        }

        private static bool IsService(Type type) {
            return type.IsDefined(typeof(ServiceAttribute), false)
                   || type.IsDefined(typeof(KeyedServiceAttribute), false);
        }

        private static bool IsValid(Type type) {
            return type is { IsClass: true, IsAbstract: false };
        }
    }
}