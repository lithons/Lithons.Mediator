using Lithons.Mediator.Exceptions;

namespace Lithons.Mediator.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void HandlerNotFoundException_MessageType_MatchesProvidedType()
    {
        var ex = new HandlerNotFoundException(typeof(string));

        Assert.Equal(typeof(string), ex.MessageType);
    }

    [Fact]
    public void HandlerNotFoundException_Message_ContainsTypeName()
    {
        var ex = new HandlerNotFoundException(typeof(string));

        Assert.Contains("String", ex.Message);
    }

    [Fact]
    public void HandlerNotFoundException_WithInnerException_IsPreserved()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new HandlerNotFoundException(typeof(string), inner);

        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void HandlerNotFoundException_IsMediatorException()
    {
        var ex = new HandlerNotFoundException(typeof(string));

        Assert.IsAssignableFrom<MediatorException>(ex);
    }

    [Fact]
    public void DuplicateHandlerException_MessageType_MatchesProvidedType()
    {
        var ex = new DuplicateHandlerException(typeof(int));

        Assert.Equal(typeof(int), ex.MessageType);
    }

    [Fact]
    public void DuplicateHandlerException_Message_ContainsTypeName()
    {
        var ex = new DuplicateHandlerException(typeof(int));

        Assert.Contains("Int32", ex.Message);
    }

    [Fact]
    public void DuplicateHandlerException_IsMediatorException()
    {
        var ex = new DuplicateHandlerException(typeof(int));

        Assert.IsAssignableFrom<MediatorException>(ex);
    }
}
