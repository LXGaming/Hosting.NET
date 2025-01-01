<div align="center">
<picture>
  <img src="icon.png" alt="Hosting.NET Icon" height="128">
</picture>

[![License](https://img.shields.io/github/license/LXGaming/Hosting.NET?label=License&cacheSeconds=86400)](https://github.com/LXGaming/Hosting.NET/blob/main/LICENSE)
[![NuGet](https://img.shields.io/nuget/vpre/LXGaming.Hosting?label=NuGet)](https://www.nuget.org/packages/LXGaming.Hosting)
</div>

**Hosting.NET** is an open source [.NET](https://dotnet.microsoft.com/) library which automates the registration of services for dependency injection through attributes.

## Features
- Keyed services support.
- `IHostedService` support.
- Reflection and Source Generator implementations.

## Usage
### Service Attribute
```csharp
// Transient Service
[Service(ServiceLifetime.Transient)]
public class MyTransientService {
}

// Scoped Service with a Service Type
[Service(ServiceLifetime.Scoped, typeof(MyServiceType))]
public class MyScopedService : MyServiceType {
}

// Hosted Service
[Service(ServiceLifetime.Singleton)]
public class MyHostedService : IHostedService {
    // implementation omitted for brevity
}
```

### Keyed Service Attribute
```csharp
// Transient Service
[KeyedService(ServiceLifetime.Transient, "MyTransientServiceKey")]
public class MyTransientService {
}

// Scoped Service with a Service Type
[KeyedService(ServiceLifetime.Scoped, "MyScopedServiceKey", typeof(MyServiceType))]
public class MyScopedService : MyServiceType {
}

// Hosted Service
[KeyedService(ServiceLifetime.Singleton, "MyHostedServiceKey")]
public class MyHostedService : IHostedService {
    // implementation omitted for brevity
}
```

### Registration
```csharp
var services = new ServiceCollection();

// Reflection
services.AddAllServices(Assembly.GetExecutingAssembly());

// Source Generator
services.AddAllServices();
```

## License
Hosting.NET is licensed under the [Apache 2.0](https://github.com/LXGaming/Hosting.NET/blob/main/LICENSE) license.