using System;

namespace LXGaming.Hosting {

    public sealed class EmptyServiceProvider : IServiceProvider {

        public static EmptyServiceProvider Instance { get; } = new EmptyServiceProvider();

        public object? GetService(Type serviceType) {
            return null;
        }
    }
}