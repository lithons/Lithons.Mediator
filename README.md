# Lithons.Mediator

[![NuGet Version](https://img.shields.io/nuget/v/Lithons.Mediator)](https://www.nuget.org/packages/Lithons.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Lithons.Mediator)](https://www.nuget.org/packages/Lithons.Mediator)
[![Build](https://github.com/lithons/Lithons.Mediator/actions/workflows/build.yml/badge.svg)](https://github.com/lithons/Lithons.Mediator/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/license-MIT-green)](https://github.com/lithons/Lithons.Mediator/blob/main/LICENSE)

A lightweight mediator for .NET with first-class support for **requests**, **commands**, and **notifications** — each with its own configurable middleware pipeline.

## Installation

```
dotnet add package Lithons.Mediator
```

> Requires .NET 10 or later.

## Quick start

```csharp
// Program.cs
builder.Services.AddMediator(cfg =>
    cfg.AddHandlersFromAssembly<Program>());
```

That single call registers the mediator and **automatically discovers** every handler in the assembly containing `Program`. No manual registration needed.

Inject `IMediator` (or the narrower `IRequestSender`, `ICommandSender`, `INotificationSender`) wherever you need to send messages.

---

## Handler registration

### Automatic discovery (recommended)

Scan an entire assembly for handlers. Open generic type definitions and abstract classes are skipped automatically.

```csharp
// Inside the AddMediator callback — fluent API
builder.Services.AddMediator(cfg =>
    cfg.AddHandlersFromAssembly<Program>());

// Or as a standalone extension method
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);
```

You can pass an optional filter to control which types are registered:

```csharp
builder.Services.AddMediator(cfg =>
    cfg.AddHandlersFromAssembly<Program>(type => type.Namespace!.StartsWith("MyApp.Handlers")));
```

### Manual registration

Register individual handlers when you want explicit control:

```csharp
builder.Services
    .AddRequestHandler<GetUserByIdHandler>()
    .AddCommandHandler<CreateUserHandler>()
    .AddNotificationHandler<UserCreatedEmailHandler>();
```

---

## Message types

| Type | Interface | Handlers | Returns |
|---|---|---|---|
| Request | `IRequest<TResponse>` | exactly one | `TResponse` |
| Command | `ICommand` / `ICommand<TResult>` | exactly one | nothing / `TResult` |
| Notification | `INotification` | zero or more | — |

All handler interfaces are generic, so the mediator resolves the correct handler at runtime based on the message type arguments.

---

## Requests

A request **queries** something and returns a result. Exactly one handler must be registered for each `IRequest<T>`.

```csharp
public record GetUserById(int Id) : IRequest<UserDto>;

public class GetUserByIdHandler : IRequestHandler<GetUserById, UserDto>
{
    public Task<UserDto> Handle(GetUserById request, CancellationToken ct)
        => Task.FromResult(new UserDto(request.Id, "Alice"));
}

// Usage
var user = await mediator.SendAsync(new GetUserById(42));
```

---

## Commands

A command **triggers a side effect**. It can optionally return a result.

```csharp
// Without a result
public record DeleteUser(int Id) : ICommand;
public class DeleteUserHandler : ICommandHandler<DeleteUser> { /* ... */ }

// With a result
public record CreateUser(string Name) : ICommand<int>;
public class CreateUserHandler : ICommandHandler<CreateUser, int> { /* ... */ }

await mediator.SendAsync(new DeleteUser(42));
int id = await mediator.SendAsync(new CreateUser("Alice"));
```

### Command strategies

| Strategy | Description |
|---|---|
| `CommandStrategy.Default` | Executes inline *(default)* |
| `CommandStrategy.Background` | Queues onto `ICommandsChannel` for background processing |

```csharp
await mediator.SendAsync(new DeleteUser(42), CommandStrategy.Background);

// Or set a default
builder.Services.AddMediator(cfg =>
    cfg.DefaultCommandStrategy = CommandStrategy.Background);
```

---

## Notifications

A notification **broadcasts an event** to zero or more handlers.

```csharp
public record UserCreated(int UserId) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreated> { /* ... */ }
public class AuditLogHandler : INotificationHandler<UserCreated> { /* ... */ }

await mediator.SendAsync(new UserCreated(42));
```

### Notification strategies

| Strategy | Description |
|---|---|
| `NotificationStrategy.Sequential` | Handlers run one after another *(default)* |
| `NotificationStrategy.Parallel` | Handlers run concurrently via `Task.WhenAll` |

```csharp
await mediator.SendAsync(new UserCreated(42), NotificationStrategy.Parallel);

// Or set a default
builder.Services.AddMediator(cfg =>
    cfg.DefaultNotificationStrategy = NotificationStrategy.Parallel);
```

---

## Middleware pipelines

Each message type has its own pipeline. Pipelines are singletons configured once at startup.

```csharp
var requestPipeline = app.Services.GetRequiredService<IRequestPipeline>();

requestPipeline.Setup(builder =>
{
    builder.Use(next => async ctx =>
    {
        Console.WriteLine($"Before {ctx.Request.GetType().Name}");
        await next(ctx);
        Console.WriteLine($"After {ctx.Request.GetType().Name}");
    });

    builder.UseRequestHandlers(); // must be last
});
```

The same pattern applies to `ICommandPipeline` / `UseCommandHandlers()` and `INotificationPipeline` / `UseNotificationHandlers()`.

### Typed middleware classes

For reusable middleware, implement the corresponding interface and register with `UseMiddleware<T>()`:

```csharp
public class LoggingMiddleware(RequestMiddlewareDelegate next) : IRequestMiddleware
{
    public async ValueTask InvokeAsync(RequestContext context)
    {
        Console.WriteLine($"→ {context.Request.GetType().Name}");
        await next(context);
        Console.WriteLine($"← {context.Request.GetType().Name}");
    }
}

requestPipeline.Setup(builder =>
{
    builder.UseMiddleware<LoggingMiddleware>();
    builder.UseRequestHandlers();
});
```

| Pipeline | Interface | Context type |
|---|---|---|
| `IRequestPipeline` | `IRequestMiddleware` | `RequestContext` |
| `ICommandPipeline` | `ICommandMiddleware` | `CommandContext` |
| `INotificationPipeline` | `INotificationMiddleware` | `NotificationContext` |