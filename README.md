# Lithons.Mediator

[![NuGet Version](https://img.shields.io/nuget/v/Lithons.Mediator)](https://www.nuget.org/packages/Lithons.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Lithons.Mediator)](https://www.nuget.org/packages/Lithons.Mediator)
[![Build](https://github.com/lithons/Lithons.Mediator/actions/workflows/build.yml/badge.svg)](https://github.com/lithons/Lithons.Mediator/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/license-MIT-green)](https://github.com/lithons/Lithons.Mediator/blob/main/LICENSE)

A lightweight mediator implementation for .NET with first-class support for **requests**, **commands**, and **notifications** — each with its own configurable middleware pipeline.

## Installation

```
dotnet add package Lithons.Mediator
```

> Requires .NET 10 or later.

## Registration

```csharp
builder.Services.AddMediator();
```

Register your handlers individually:

```csharp
builder.Services
    .AddRequestHandler<GetUserByIdHandler>()
    .AddCommandHandler<CreateUserHandler>()
    .AddNotificationHandler<UserCreatedEmailHandler>()
    .AddNotificationHandler<UserCreatedAuditHandler>();
```

---

## Concepts

Lithons.Mediator distinguishes between three message types:

| Type | Interface | Handlers | Returns |
|---|---|---|---|
| Request | `IRequest<T>` | exactly one | `T` |
| Command | `ICommand` / `ICommand<T>` | exactly one | nothing / `T` |
| Notification | `INotification` | zero or more | — |

Inject `IMediator` (or the narrower `IRequestSender`, `ICommandSender`, `INotificationSender`) wherever you need to send messages.

---

## Requests

Use a request when you need to **query** something and get a result back. Exactly one handler must be registered.

```csharp
// Define
public record GetUserById(int Id) : IRequest<UserDto>;

// Handle
public class GetUserByIdHandler : IRequestHandler<GetUserById, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserById request, CancellationToken cancellationToken)
    {
        // ...
        return new UserDto(request.Id, "Alice");
    }
}

// Send
var user = await mediator.SendAsync(new GetUserById(42), cancellationToken);
```

---

## Commands

Use a command when you want to **trigger a side effect**. Commands can optionally return a result.

**Without a result:**

```csharp
// Define
public record DeleteUser(int Id) : ICommand;

// Handle
public class DeleteUserHandler : ICommandHandler<DeleteUser>
{
    public async Task HandleAsync(DeleteUser command, CancellationToken cancellationToken)
    {
        // ...
    }
}

// Send
await mediator.SendAsync(new DeleteUser(42), cancellationToken);
```

**With a result:**

```csharp
// Define
public record CreateUser(string Name) : ICommand<int>;

// Handle
public class CreateUserHandler : ICommandHandler<CreateUser, int>
{
    public async Task<int> HandleAsync(CreateUser command, CancellationToken cancellationToken)
    {
        // ...
        return newUserId;
    }
}

// Send
int id = await mediator.SendAsync(new CreateUser("Alice"), cancellationToken);
```

### Command strategies

Commands support an optional execution strategy. Pass one inline or configure the default in `MediatorOptions`.

```csharp
// Run in the background (fire-and-forget via ICommandsChannel)
await mediator.SendAsync(new DeleteUser(42), CommandStrategy.Background, cancellationToken);
```

| Strategy | Description |
|---|---|
| `CommandStrategy.Default` | Executes inline, same as no strategy |
| `CommandStrategy.Background` | Queues onto `ICommandsChannel` for background processing |

Configure the default:

```csharp
builder.Services.AddMediator(options =>
{
    options.DefaultCommandStrategy = CommandStrategy.Background;
});
```

---

## Notifications

Use a notification when you want to **broadcast an event** to multiple independent handlers.

```csharp
// Define
public record UserCreated(int UserId) : INotification;

// Handle (register as many handlers as you need)
public class SendWelcomeEmailHandler : INotificationHandler<UserCreated>
{
    public async Task HandleAsync(UserCreated notification, CancellationToken cancellationToken)
    {
        // send email...
    }
}

public class AuditLogHandler : INotificationHandler<UserCreated>
{
    public async Task HandleAsync(UserCreated notification, CancellationToken cancellationToken)
    {
        // write audit log...
    }
}

// Publish
await mediator.SendAsync(new UserCreated(42), cancellationToken);
```

### Notification strategies

| Strategy | Description |
|---|---|
| `NotificationStrategy.Sequential` | Handlers run one after another *(default)* |
| `NotificationStrategy.Parallel` | Handlers run concurrently via `Task.WhenAll` |

```csharp
await mediator.SendAsync(new UserCreated(42), NotificationStrategy.Parallel, cancellationToken);
```

Configure the default:

```csharp
builder.Services.AddMediator(options =>
{
    options.DefaultNotificationStrategy = NotificationStrategy.Parallel;
});
```

---

## Middleware pipelines

Each message type has its own pipeline that you can customise with middleware. Pipelines are singletons and should be configured once at startup.

```csharp
// Resolve the pipeline and call Setup before the app starts
var requestPipeline = app.Services.GetRequiredService<IRequestPipeline>();

requestPipeline.Setup(builder =>
{
    builder.Use(next => async ctx =>
    {
        // runs before every request handler
        Console.WriteLine($"Handling {ctx.Request.GetType().Name}");
        await next(ctx);
        Console.WriteLine($"Handled {ctx.Request.GetType().Name}");
    });

    builder.UseRequestHandlers(); // must be last
});
```

The same pattern applies to `ICommandPipeline` / `UseCommandHandlers()` and `INotificationPipeline` / `UseNotificationHandlers()`.

### Typed middleware classes

For reusable middleware, implement the corresponding interface and register it with `UseMiddleware<T>()`:

```csharp
public class LoggingRequestMiddleware(RequestMiddlewareDelegate next) : IRequestMiddleware
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
    builder.UseMiddleware<LoggingRequestMiddleware>();
    builder.UseRequestHandlers();
});
```

| Pipeline | Interface | Context type |
|---|---|---|
| `IRequestPipeline` | `IRequestMiddleware` | `RequestContext` |
| `ICommandPipeline` | `ICommandMiddleware` | `CommandContext` |
| `INotificationPipeline` | `INotificationMiddleware` | `NotificationContext` |