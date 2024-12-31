using System;
using LXGaming.Hosting.Generators.Models;
using Microsoft.CodeAnalysis;

namespace LXGaming.Hosting.Generators.Results {

    public sealed class TransformResult : IEquatable<TransformResult> {

        public bool IsSuccess => Diagnostic == null;

        public Location Location { get; }

        public Diagnostic? Diagnostic { get; }

        public ServiceDescriptor? Descriptor { get; }

        private TransformResult(Location location, Diagnostic? diagnostic, ServiceDescriptor? descriptor) {
            Location = location;
            Diagnostic = diagnostic;
            Descriptor = descriptor;
        }

        public static TransformResult FromSuccess(Location location, ServiceDescriptor descriptor) {
            return new TransformResult(location, null, descriptor);
        }

        public static TransformResult FromError(DiagnosticDescriptor descriptor, Location location,
            params object?[]? messageArgs) {
            var diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
            return new TransformResult(location, diagnostic, null);
        }

        public bool Equals(TransformResult? other) {
            if (other is null) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Location.Equals(other.Location)
                   && Equals(Diagnostic, other.Diagnostic)
                   && Equals(Descriptor, other.Descriptor);
        }

        public override bool Equals(object? obj) {
            return ReferenceEquals(this, obj) || obj is TransformResult other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Location.GetHashCode();
                hashCode = (hashCode * 397) ^ (Diagnostic != null ? Diagnostic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Descriptor != null ? Descriptor.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TransformResult? left, TransformResult? right) {
            return Equals(left, right);
        }

        public static bool operator !=(TransformResult? left, TransformResult? right) {
            return !Equals(left, right);
        }
    }
}