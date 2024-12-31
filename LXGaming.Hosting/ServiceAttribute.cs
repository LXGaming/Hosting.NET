using System;
using Microsoft.Extensions.DependencyInjection;

namespace LXGaming.Hosting {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ServiceAttribute : Attribute {

        public ServiceLifetime Lifetime { get; }

        public Type? Type { get; }

        public ServiceAttribute(ServiceLifetime lifetime, Type? type = null) {
            Lifetime = lifetime;
            Type = type;
        }
    }
}