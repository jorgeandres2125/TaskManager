using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Behaviors;
using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Validators.Tasks;

namespace TaskManager.Tests.Application;

public class ValidationBehaviorTests
{
    private class SpyHandler : IRequestHandler<ChangeTaskStatusCommand, string>
    {
        public bool WasCalled { get; private set; }

        public Task<string> HandleAsync(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult("ok");
        }
    }

    [Fact]
    public async Task InvalidRequest_ThrowsWithErrorsAndSkipsHandler()
    {
        var inner = new SpyHandler();
        var behavior = new ValidationBehavior<ChangeTaskStatusCommand, string>(inner, [new ChangeTaskStatusCommandValidator()]);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => behavior.HandleAsync(new ChangeTaskStatusCommand(Guid.NewGuid(), "Archived"), CancellationToken.None));

        Assert.Contains(nameof(ChangeTaskStatusCommand.Status), exception.Errors.Keys);
        Assert.False(inner.WasCalled);
    }

    [Fact]
    public async Task ValidRequest_CallsHandler()
    {
        var inner = new SpyHandler();
        var behavior = new ValidationBehavior<ChangeTaskStatusCommand, string>(inner, [new ChangeTaskStatusCommandValidator()]);

        var result = await behavior.HandleAsync(new ChangeTaskStatusCommand(Guid.NewGuid(), "Done"), CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.True(inner.WasCalled);
    }
}
