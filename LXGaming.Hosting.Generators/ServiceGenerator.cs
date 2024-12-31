using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LXGaming.Hosting.Generators.Models;
using LXGaming.Hosting.Generators.Results;
using LXGaming.Hosting.Generators.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LXGaming.Hosting.Generators {

    [Generator]
    public class ServiceGenerator : IIncrementalGenerator {

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var serviceAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
                Constants.Types.ServiceAttribute, Predicate, Transform).Collect();

            var keyedServiceAttributes = context.SyntaxProvider.ForAttributeWithMetadataName(
                Constants.Types.KeyedServiceAttribute, Predicate, Transform).Collect();

            var transformResults = serviceAttributes.Combine(keyedServiceAttributes).Select((tuple, _) => {
                var list = new List<TransformResult>(tuple.Left.Length + tuple.Right.Length);
                list.AddRange(tuple.Left);
                list.AddRange(tuple.Right);
                return list;
            });

            context.RegisterSourceOutput(transformResults, Generate);
        }

        private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) {
            return node is ClassDeclarationSyntax;
        }

        private static TransformResult Transform(GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            var targetNodeLocation = context.TargetNode.GetLocation();

            if (context.TargetSymbol.IsAbstract) {
                return TransformResult.FromError(DiagnosticDescriptors.AbstractService, targetNodeLocation);
            }

            if (context.Attributes.Length == 0) {
                return TransformResult.FromError(DiagnosticDescriptors.MissingServiceAttribute, targetNodeLocation);
            }

            if (context.Attributes.Length > 1) {
                return TransformResult.FromError(DiagnosticDescriptors.AmbiguousService, targetNodeLocation);
            }

            var attribute = context.Attributes.Single();
            if (attribute.AttributeClass == null) {
                return TransformResult.FromError(DiagnosticDescriptors.MissingAttributeClass, targetNodeLocation);
            }

            var implementationType = context.TargetSymbol.ToDisplayString();
            var isHostedService = context.TargetSymbol.IsHostedService();
            var location = attribute.GetLocation() ?? targetNodeLocation;

            if (attribute.AttributeClass.IsServiceAttribute()) {
                if (attribute.ConstructorArguments.Length != 2) {
                    return TransformResult.FromError(DiagnosticDescriptors.InvalidServiceAttribute, location,
                        attribute.AttributeClass.Name);
                }

                var lifetimeArgument = attribute.ConstructorArguments[0];
                var serviceTypeArgument = attribute.ConstructorArguments[1];

                var lifetime = lifetimeArgument.ToCSharpString();
                var serviceType = !serviceTypeArgument.IsNull ? serviceTypeArgument.ToCSharpString() : null;

                var descriptor = new ServiceDescriptor(serviceType, null, implementationType, lifetime,
                    isHostedService);
                return TransformResult.FromSuccess(location, descriptor);
            }

            if (attribute.AttributeClass.IsKeyedServiceAttribute()) {
                if (attribute.ConstructorArguments.Length != 3) {
                    return TransformResult.FromError(DiagnosticDescriptors.InvalidServiceAttribute, location,
                        attribute.AttributeClass.Name);
                }

                var lifetimeArgument = attribute.ConstructorArguments[0];
                var serviceKeyArgument = attribute.ConstructorArguments[1];
                var serviceTypeArgument = attribute.ConstructorArguments[2];

                var lifetime = lifetimeArgument.ToCSharpString();
                var serviceKey = serviceKeyArgument.ToCSharpString();
                var serviceType = !serviceTypeArgument.IsNull ? serviceTypeArgument.ToCSharpString() : null;

                var descriptor = new ServiceDescriptor(serviceType, serviceKey, implementationType, lifetime,
                    isHostedService);
                return TransformResult.FromSuccess(location, descriptor);
            }

            return TransformResult.FromError(DiagnosticDescriptors.UnsupportedServiceAttribute, location,
                attribute.AttributeClass.Name);
        }

        private static void Generate(SourceProductionContext context, List<TransformResult> results) {
            var implementationTypes = new List<string>(results.Count);
            var stringBuilder = new StringBuilder();

            foreach (var result in results.OrderBy(result => result.Descriptor?.ImplementationType)) {
                if (!result.IsSuccess) {
                    context.ReportDiagnostic(result.Diagnostic!);
                    continue;
                }

                var location = result.Location;
                var descriptor = result.Descriptor!;

                if (implementationTypes.Contains(descriptor.ImplementationType)) {
                    context.ReportDiagnostic(DiagnosticDescriptors.AmbiguousService, location);
                    continue;
                }

                implementationTypes.Add(descriptor.ImplementationType);

                if (stringBuilder.Length != 0) {
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendSource(Constants.Source.StartRegion, descriptor.ImplementationType);

                var implementationTypeSource = $"typeof({descriptor.ImplementationType})";
                var hostedServiceTypeSource = $"typeof({Constants.Types.HostedService})";
                var lifetimeSource = descriptor.Lifetime.Substring(41);
                var lifetimeName = descriptor.Lifetime.Substring(57);

                if (descriptor.ServiceKey != null) {
                    if (descriptor.IsHostedService) {
                        if (!string.Equals(descriptor.Lifetime, Constants.Types.ServiceLifetimeSingleton)) {
                            context.ReportDiagnostic(DiagnosticDescriptors.InvalidHostedServiceLifetime, location,
                                lifetimeName);
                            continue;
                        }

                        stringBuilder.AppendSource(Constants.Source.KeyedServiceDescriptorType,
                            implementationTypeSource, descriptor.ServiceKey, implementationTypeSource, lifetimeSource);
                        stringBuilder.AppendSource(Constants.Source.KeyedServiceDescriptorFactory,
                            hostedServiceTypeSource, descriptor.ServiceKey, implementationTypeSource, lifetimeSource);
                        if (descriptor.ServiceType != null
                            && !string.Equals(descriptor.ServiceType, hostedServiceTypeSource)) {
                            stringBuilder.AppendSource(Constants.Source.KeyedServiceDescriptorFactory,
                                descriptor.ServiceType, descriptor.ServiceKey, implementationTypeSource,
                                lifetimeSource);
                        }
                    } else {
                        stringBuilder.AppendSource(Constants.Source.KeyedServiceDescriptorType,
                            implementationTypeSource, descriptor.ServiceKey, implementationTypeSource, lifetimeSource);
                        if (descriptor.ServiceType != null) {
                            stringBuilder.AppendSource(Constants.Source.KeyedServiceDescriptorFactory,
                                descriptor.ServiceType, descriptor.ServiceKey, implementationTypeSource,
                                lifetimeSource);
                        }
                    }
                } else {
                    if (descriptor.IsHostedService) {
                        if (!string.Equals(descriptor.Lifetime, Constants.Types.ServiceLifetimeSingleton)) {
                            context.ReportDiagnostic(DiagnosticDescriptors.InvalidHostedServiceLifetime, location,
                                lifetimeName);
                            continue;
                        }

                        stringBuilder.AppendSource(Constants.Source.ServiceDescriptorType, implementationTypeSource,
                            implementationTypeSource, lifetimeSource);
                        stringBuilder.AppendSource(Constants.Source.ServiceDescriptorFactory, hostedServiceTypeSource,
                            implementationTypeSource, lifetimeSource);
                        if (descriptor.ServiceType != null
                            && !string.Equals(descriptor.ServiceType, hostedServiceTypeSource)) {
                            stringBuilder.AppendSource(Constants.Source.ServiceDescriptorFactory,
                                descriptor.ServiceType, implementationTypeSource, lifetimeSource);
                        }
                    } else {
                        stringBuilder.AppendSource(Constants.Source.ServiceDescriptorType, implementationTypeSource,
                            implementationTypeSource, lifetimeSource);
                        if (descriptor.ServiceType != null) {
                            stringBuilder.AppendSource(Constants.Source.ServiceDescriptorFactory,
                                descriptor.ServiceType, implementationTypeSource, lifetimeSource);
                        }
                    }
                }

                stringBuilder.AppendSource(Constants.Source.EndRegion);
            }

            var source = StringUtils.Format(Constants.Source.ServiceExtensions, stringBuilder.ToString());
            context.AddSource("ServiceExtensions.g.cs", source);
        }
    }
}