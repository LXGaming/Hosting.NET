using System;
using Microsoft.Extensions.DependencyInjection;

#if NET8_0_OR_GREATER
namespace LXGaming.Hosting {

    /// <summary>
    /// Marks the class as a keyed service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class KeyedServiceAttribute : ServiceAttribute {

        /// <summary>
        /// The key of the service.
        /// </summary>
        public object? Key { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="KeyedServiceAttribute"/> class with the specified parameters.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <param name="key">The key of the service.</param>
        /// <param name="type">The type of the service.</param>
        public KeyedServiceAttribute(ServiceLifetime lifetime, object? key, Type? type = null) : base(lifetime, type) {
            Key = key;
        }
    }
}
#endif