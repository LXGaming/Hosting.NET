using System;
using Microsoft.Extensions.DependencyInjection;

namespace LXGaming.Hosting {

    /// <summary>
    /// Marks the class as a service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ServiceAttribute : Attribute {

        /// <summary>
        /// The lifetime of the service.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// The type of the service.
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ServiceAttribute"/> class with the specified parameters.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <param name="type">The type of the service.</param>
        public ServiceAttribute(ServiceLifetime lifetime, Type? type = null) {
            Lifetime = lifetime;
            Type = type;
        }
    }
}