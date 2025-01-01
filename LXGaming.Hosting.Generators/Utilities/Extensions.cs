using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace LXGaming.Hosting.Generators.Utilities {

    public static class Extensions {

        public static void AppendSource(this StringBuilder stringBuilder, string format, params object?[] args) {
            using var stringReader = new StringReader(StringUtils.Format(format, args));
            while (stringReader.ReadLine() is { } line) {
                stringBuilder.Append(Constants.Source.Indent);
                stringBuilder.AppendLine(line);
            }
        }

        public static Location? GetLocation(this AttributeData attribute) {
            var syntaxReference = attribute.ApplicationSyntaxReference;
            return syntaxReference != null
                ? Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span)
                : null;
        }

        public static bool IsHostedService(this ISymbol symbol) {
            if (symbol is ITypeSymbol typeSymbol) {
                return typeSymbol.AllInterfaces.Any(namedTypeSymbol =>
                    string.Equals(namedTypeSymbol.ToDisplayString(), Constants.Types.HostedService));
            }

            return false;
        }

        public static bool IsServiceAttribute(this ISymbol symbol) {
            return string.Equals(symbol.ToDisplayString(), Constants.Types.ServiceAttribute);
        }

        public static bool IsKeyedServiceAttribute(this ISymbol symbol) {
            return string.Equals(symbol.ToDisplayString(), Constants.Types.KeyedServiceAttribute);
        }

        public static void ReportDiagnostic(this SourceProductionContext context, DiagnosticDescriptor descriptor,
            Location? location, params object?[]? messageArgs) {
            var diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
            context.ReportDiagnostic(diagnostic);
        }
    }
}