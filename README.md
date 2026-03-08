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

## Concepts

| Type | Interface | Handlers | Returns |
|---|---|---|---|
| Request | `IRequest<T>` | exactly one | `T` |
| Command | `ICommand` / `ICommand<T>` | exactly one | nothing / `T` |
| Notification | `INotification` | zero or more | — |

Inject `IMediator` (or the narrower `IRequestSender`, `ICommandSender`, `INotificationSender`) to send messages.

## Registration

Scan an assembly to auto-register all handlers:

```csharp
builder.Services.AddMediator(cfg =>
{
    cfg.AddHandlersFromAssembly<Program>();
});
```

You can pass an `Assembly` directly or supply a filter:

Inject `IMediator` (or the narrower `IRequestSender`, `ICommandSender`, `INotificationSender`) wherever you need to send messages.

---

## Handler registration

### Automatic discovery (recommended)

Scan an entire assembly for handlers. Open generic type definitions and abstract classes are skipped automatically.

```csharp
cfg.AddHandlersFromAssembly(typeof(Program).Assembly);
cfg.AddHandlersFromAssembly<Program>(type => type.Namespace?.StartsWith("MyApp.Handlers") == true);
```

Or register handlers individually:

```csharp
cfg.AddRequestHandler<GetUserByIdHandler>();
cfg.AddCommandHandler<CreateUserHandler>();
cfg.AddNotificationHandler<UserCreatedEmailHandler>();
```

## Requests

Query something and get a result back. Exactly one handler must be registered.

```csharp
public record GetUserById(int Id) : IRequest<UserDto>;

public class GetUserByIdHandler : IRequestHandler<GetUserById, UserDto>
{
    public async Task<UserDto> Handle(GetUserById request, CancellationToken cancellationToken)
        => new UserDto(request.Id, "Alice");
}

var user = await mediator.SendAsync(new GetUserById(42), cancellationToken);
```

## Commands

Trigger a side effect. Commands can optionally return a result.

```csharp
// Without a result
public record DeleteUser(int Id) : ICommand;
public class DeleteUserHandler : ICommandHandler<DeleteUser> { /* ... */ }

public class DeleteUserHandler : ICommandHandler<DeleteUser>
{
    public async Task Handle(DeleteUser command, CancellationToken cancellationToken) { /* ... */ }
}

await mediator.SendAsync(new DeleteUser(42), cancellationToken);

// With a result
public record CreateUser(string Name) : ICommand<int>;

public class CreateUserHandler : ICommandHandler<CreateUser, int>
{
    public async Task<int> Handle(CreateUser command, CancellationToken cancellationToken) => newUserId;
}

int id = await mediator.SendAsync(new CreateUser("Alice"), cancellationToken);
```

### Command strategies

| Strategy | Description |
|---|---|
| `CommandStrategy.Default` | Executes inline *(default)* |
| `CommandStrategy.Background` | Queues onto `ICommandsChannel` for background processing |

```csharp
await mediator.SendAsync(new DeleteUser(42), CommandStrategy.Background, cancellationToken);

// Or configure the default
builder.Services.AddMediator(options =>
{
    options.DefaultCommandStrategy = CommandStrategy.Background;
});
```

## Notifications

Broadcast an event to zero or more independent handlers.

```csharp
public record UserCreated(int UserId) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreated>
{
    public async Task Handle(UserCreated notification, CancellationToken cancellationToken) { /* ... */ }
}

await mediator.SendAsync(new UserCreated(42), cancellationToken);
```

### Notification strategies

| Strategy | Description |
|---|---|
| `NotificationStrategy.Sequential` | Handlers run one after another *(default)* |
| `NotificationStrategy.Parallel` | Handlers run concurrently via `Task.WhenAll` |

```csharp
await mediator.SendAsync(new UserCreated(42), NotificationStrategy.Parallel, cancellationToken);

// Or configure the default
builder.Services.AddMediator(options =>
{
    options.DefaultNotificationStrategy = NotificationStrategy.Parallel;
});
```

## Middleware pipelines

Each message type has its own pipeline. Pipelines are singletons configured once at startup.

**Inline middleware:**

```csharp
var requestPipeline = app.Services.GetRequiredService<IRequestPipeline>();

requestPipeline.Setup(builder =>
{
    builder.Use(next => async ctx =>
    {
        Console.WriteLine($"→ {ctx.Request.GetType().Name}");
        await next(ctx);
        Console.WriteLine($"← {ctx.Request.GetType().Name}");
    });

    builder.UseRequestHandlers(); // must be last
});
```

**Typed middleware class:**

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

The same pattern applies to `ICommandPipeline` / `UseCommandHandlers()` and `INotificationPipeline` / `UseNotificationHandlers()`.

| Pipeline | Interface | Context type |
|---|---|---|
| `IRequestPipeline` | `IRequestMiddleware` | `RequestContext` |
| `ICommandPipeline` | `ICommandMiddleware` | `CommandContext` |
| `INotificationPipeline` | `INotificationMiddleware` | `NotificationContext` |

## Exception handling

Register exception handlers to catch unhandled exceptions from any pipeline without writing middleware. Return `true` to swallow the exception or `false` to let it propagate.

**Typed** — handles exceptions for a specific message type:

```csharp
public class GetUserByIdExceptionHandler : IExceptionHandler<GetUserById>
{
    public ValueTask<bool> Handle(Exception exception, GetUserById message, CancellationToken cancellationToken)
        => ValueTask.FromResult(true); // handled — don’t rethrow
}
```

**Global** — catch-all for all message types:

```csharp
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> Handle(Exception exception, object message, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {MessageType}", message.GetType().Name);
        return ValueTask.FromResult(false); // not handled — rethrow
    }
}
```

Register via `MediatorConfiguration`:

```csharp
builder.Services.AddMediator(cfg =>
{
    cfg.AddExceptionHandler<GlobalExceptionHandler>();
    cfg.AddExceptionHandler<GetUserById, GetUserByIdExceptionHandler>();
});
```

### Resolution order

1. **Typed** — `IExceptionHandler<TMessage>` is tried first.
2. **Global** — `IExceptionHandler` is tried if no typed handler handled it.
3. **Rethrow** — if neither returns `true`, the original exception propagates.
