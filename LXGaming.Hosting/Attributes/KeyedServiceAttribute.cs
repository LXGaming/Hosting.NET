using System;
using Microsoft.Extensions.DependencyInjection;

namespace LXGaming.Hosting.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class KeyedServiceAttribute : ServiceAttribute {

        public object? Key { get; }

        public KeyedServiceAttribute(ServiceLifetime lifetime, object? key, Type? type = null) : base(lifetime, type) {
            Key = key;
        }
    }
}