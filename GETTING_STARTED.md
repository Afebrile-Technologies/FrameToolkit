# Getting Started — FrameToolkit.AspNetCore

This guide shows quick examples for using the utilities and abstractions provided by this project in an ASP.NET Core Minimal API application.

## Installation

```bash
dotnet add package FrameToolkit.AspNetCore
```

## Quick Program.cs (Minimal API)

This example registers the toolkit services, endpoint definitions, and open-generic dispatcher implementations.

```csharp
using FrameToolkit.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register FrameToolkit helpers
builder.Services.AddEndpointDefinitions();      // scans and registers IEndpointDefinition implementations
builder.Services.AddDomainEventService();      // discovers and registers IDomainEventHandler<T> and IDomainEventsDispatcher
builder.Services.AddCommandQueryDefinitions(); // discovers and registers command/query handlers

var app = builder.Build();

// Map endpoints defined by IEndpointDefinition implementations
app.MapEndpointsFromDefinitions();

app.Run();
```

Notes:
- `AddEndpointDefinitions` finds and registers types implementing `IEndpointDefinition` in loaded assemblies.
- `AddDomainEventService` registers discovered `IDomainEventHandler<T>` implementations and a `DomainEventsDispatcher`.
- `AddCommandQueryDefinitions` registers implementations for the CQRS handlers and dispatchers.

## Defining endpoints with `IEndpointDefinition`

Create a class that implements `IEndpointDefinition` and register routes in `RegisterEndpoints`.

```csharp
public class PingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/ping", () => Results.Ok("pong"));

        // Example using the non-generic dispatcher to run a query
        app.MapGet("/time", async (IDispatcher dispatcher) =>
        {
            var result = await dispatcher.Query<GetTimeQuery, string>(new GetTimeQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message);
        });
    }
}

// Example query and handler
public record GetTimeQuery() : IQuery<string>;

public class GetTimeQueryHandler : IQueryHandler<GetTimeQuery, string>
{
    public Task<Result<string>> Handle(GetTimeQuery query, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(DateTime.UtcNow.ToString("O")));
}
```

Because `AddEndpointDefinitions` registers `IEndpointDefinition` implementations in DI, `MapEndpointsFromDefinitions` will resolve and call them automatically at startup.

## Domain events

Define domain events and handlers using the provided interfaces.

```csharp
// Event
public record UserCreatedEvent(Guid UserId) : IDomainEvent;

// Handler
public class SendWelcomeEmailHandler : IDomainEventHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // send email or perform side effects
        return Task.CompletedTask;
    }
}
```

Publish events by resolving `IDomainEventsDispatcher` and calling `DispatchAsync`:

```csharp
public class SomeService
{
    private readonly IDomainEventsDispatcher _dispatcher;

    public SomeService(IDomainEventsDispatcher dispatcher) => _dispatcher = dispatcher;

    public async Task CreateUserAsync(Guid id)
    {
        // ... create user
        var events = new IDomainEvent[] { new UserCreatedEvent(id) };
        await _dispatcher.DispatchAsync(events, CancellationToken.None);
    }
}
```

`AddDomainEventService` will automatically discover and register `IDomainEventHandler<T>` implementations from loaded assemblies and registers the `DomainEventsDispatcher` used above.

## Commands & Queries (CQRS)

The project provides `ICommand`, `ICommand<TResult>`, `IQuery<TResult>` and handler interfaces. After calling `AddCommandQueryDefinitions()` handlers will be registered and you can send messages via `IDispatcher`.

```csharp
// Command example
public record CreateInvoiceCommand(string CustomerId) : ICommand<Result>;

public class CreateInvoiceHandler : ICommandHandler<CreateInvoiceCommand, Result>
{
    public Task<Result> Handle(CreateInvoiceCommand message, CancellationToken cancellationToken)
    {
        // handle command
        return Task.FromResult(Result.Success());
    }
}

// Using dispatcher
var commandResult = await dispatcher.Command<CreateInvoiceCommand, Result>(new CreateInvoiceCommand("cust-1"));
```

## String helpers

Use the provided string extensions anywhere in your code. Example:

```csharp
var name = "hello".ToFirstUpperCase(); // "Hello"
var safe = html.Sanitize(); // removes disallowed tags and scripts
```

## Tips & troubleshooting

- The discovery helpers use assemblies loaded into `AppDomain.CurrentDomain`. If handlers or endpoint definitions live in assemblies that are not yet loaded at startup, ensure they are referenced or explicitly load the assembly.
- If endpoints or handlers are not being discovered, confirm types are concrete (not abstract or generic) and implement the expected interfaces.
- Use your application's DI to resolve `IDispatcher` and `IDomainEventsDispatcher` where needed.

## Support

Report issues or feature requests via the repository's GitHub Issues.
