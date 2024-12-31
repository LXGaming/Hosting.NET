using Microsoft.CodeAnalysis;

namespace LXGaming.Hosting.Generators.Utilities {

    public static class DiagnosticDescriptors {

        public static readonly DiagnosticDescriptor MissingServiceAttribute = new DiagnosticDescriptor(
            "HOSTING0001",
            "Missing service attribute",
            "Service is missing a service attribute",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );

        public static readonly DiagnosticDescriptor InvalidServiceAttribute = new DiagnosticDescriptor(
            "HOSTING0002",
            "Invalid service attribute",
            "{0} is not a valid service attribute",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );

        public static readonly DiagnosticDescriptor UnsupportedServiceAttribute = new DiagnosticDescriptor(
            "HOSTING0003",
            "Unsupported service attribute",
            "{0} is not a supported service attribute",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );

        public static readonly DiagnosticDescriptor MissingAttributeClass = new DiagnosticDescriptor(
            "HOSTING0004",
            "Missing attribute class",
            "Attribute is missing an attribute class",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );

        public static readonly DiagnosticDescriptor AbstractService = new DiagnosticDescriptor(
            "HOSTING0005",
            "Abstract service",
            "Service cannot be abstract",
            "Usage",
            DiagnosticSeverity.Error,
            true
        );

        public static readonly DiagnosticDescriptor AmbiguousService = new DiagnosticDescriptor(
            "HOSTING0006",
            "Ambiguous service",
            "Service is ambiguous",
            "Usage",
            DiagnosticSeverity.Error,
            true
        );

        public static readonly DiagnosticDescriptor InvalidHostedServiceLifetime = new DiagnosticDescriptor(
            "HOSTING0007",
            "Invalid IHostedService lifetime",
            "Hosted service cannot have a {0} lifetime",
            "Usage",
            DiagnosticSeverity.Error,
            true
        );
    }
}