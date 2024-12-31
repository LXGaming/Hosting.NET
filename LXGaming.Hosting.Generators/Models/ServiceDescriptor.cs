using System;

namespace LXGaming.Hosting.Generators.Models {

    public sealed class ServiceDescriptor : IEquatable<ServiceDescriptor> {

        public string? ServiceType { get; }

        public string? ServiceKey { get; }

        public string ImplementationType { get; }

        public string Lifetime { get; }

        public bool IsHostedService { get; }

        public ServiceDescriptor(string? serviceType, string? serviceKey, string implementationType, string lifetime,
            bool isHostedService) {
            ServiceType = serviceType;
            ServiceKey = serviceKey;
            ImplementationType = implementationType;
            Lifetime = lifetime;
            IsHostedService = isHostedService;
        }

        public bool Equals(ServiceDescriptor? other) {
            if (other is null) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ServiceType == other.ServiceType
                   && ServiceKey == other.ServiceKey
                   && ImplementationType == other.ImplementationType
                   && Lifetime == other.Lifetime
                   && IsHostedService == other.IsHostedService;
        }

        public override bool Equals(object? obj) {
            return ReferenceEquals(this, obj) || obj is ServiceDescriptor other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (ServiceType != null ? ServiceType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServiceKey != null ? ServiceKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ImplementationType.GetHashCode();
                hashCode = (hashCode * 397) ^ Lifetime.GetHashCode();
                hashCode = (hashCode * 397) ^ IsHostedService.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ServiceDescriptor? left, ServiceDescriptor? right) {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceDescriptor? left, ServiceDescriptor? right) {
            return !Equals(left, right);
        }
    }
}