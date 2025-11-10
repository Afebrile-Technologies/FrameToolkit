namespace FrameToolkit.Abstractions.Interfaces;

public interface IDispatcher<T, TValue> where T : class
{
    Task<Result<TValue>> Send(T message, CancellationToken cancellationToken = default);
}

public interface IDispatcher
{
    Task<Result<TResult>> Query<TQuery, TResult>(TQuery message, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;

    Task<Result<TResult>> Command<TCommand, TResult>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
    Task<Result> Command<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}