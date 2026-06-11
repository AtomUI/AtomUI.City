namespace AtomUI.City.Lifecycle;

public sealed class LifecyclePipeline
{
    private readonly IReadOnlyList<LifecycleMiddleware> _middleware;
    private readonly Func<LifecycleContext, ValueTask> _terminalHandler;

    internal LifecyclePipeline(
        IReadOnlyList<LifecycleMiddleware> middleware,
        Func<LifecycleContext, ValueTask> terminalHandler)
    {
        _middleware = middleware;
        _terminalHandler = terminalHandler;
    }

    public ValueTask ExecuteAsync(LifecycleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var index = -1;

        ValueTask Next()
        {
            if (context.IsShortCircuited)
            {
                return ValueTask.CompletedTask;
            }

            index++;

            return index == _middleware.Count
                ? _terminalHandler(context)
                : _middleware[index](context, Next);
        }

        return Next();
    }
}
