using System;

namespace LXGaming.Hosting {

    /// <summary>
    /// An empty <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class EmptyServiceProvider : IServiceProvider {

        /// <summary>
        /// Singleton instance of <see cref="EmptyServiceProvider"/>.
        /// </summary>
        public static EmptyServiceProvider Instance { get; } = new EmptyServiceProvider();

        /// <inheritdoc/>
        public object? GetService(Type serviceType) {
            return null;
        }
    }
}