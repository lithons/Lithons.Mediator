namespace Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

public interface IRequestMiddleware
{
    ValueTask InvokeAsync(RequestContext context);
}
