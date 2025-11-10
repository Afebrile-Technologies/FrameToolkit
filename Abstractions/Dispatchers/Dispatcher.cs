namespace FrameToolkit.Abstractions.Dispatchers;

public sealed partial class Dispatcher<T, TValue>(IServiceProvider serviceProvider)
    : IDispatcher<T, TValue> where T : class
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Result<TValue>> Send(T message, CancellationToken cancellationToken = default)
    {
        var messageType = message.GetType();

        // Check if it's a query
        var queryInterface = messageType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        if (queryInterface != null)
        {
            var resultType = queryInterface.GetGenericArguments()[0];
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(messageType, resultType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException("Handler does not have a Handle method.");
            var task = (Task)method.Invoke(handler, [message, cancellationToken])!;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task) as Result<TValue> ?? Result.Failure<TValue>(Error.None);
        }

        // Check if it's a command
        var commandInterface = messageType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
        if (commandInterface != null)
        {
            var resultType = commandInterface.GetGenericArguments()[0];
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(messageType, resultType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException("Handler does not have a Handle method.");
            var task = (Task)method.Invoke(handler, [message, cancellationToken])!;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task) as Result<TValue> ?? Result.Failure<TValue>(Error.None); ;
        }

        var commandInterfaceOverload = messageType.GetInterfaces()
            .FirstOrDefault(i => i == typeof(ICommand));
        if (commandInterfaceOverload != null)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(messageType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException("Handler does not have a Handle method.");
            var task = (Task)method.Invoke(handler, [message, cancellationToken])!;
            await task.ConfigureAwait(false);
            return null ?? Result.Failure<TValue>(Error.None); ; // No result for non-generic ICommand
        }

        throw new InvalidOperationException("Message does not implement IQuery<TResult> or ICommand<TResult>.");
    }
}


public sealed partial class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public async Task<Result<TResult>> Query<TQuery, TResult>(TQuery message, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.Handle(message, cancellationToken);
    }
    public async Task<Result<TResult>> Command<TCommand, TResult>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.Handle(message, cancellationToken);
    }

    public async Task<Result> Command<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return await handler.Handle(message, cancellationToken);
    }
}