namespace AtomUI.City.Lifecycle;

public sealed class LifecyclePipelineBuilder
{
    private readonly List<LifecycleMiddleware> _middleware = [];

    public LifecyclePipelineBuilder Use(LifecycleMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        _middleware.Add(middleware);

        return this;
    }

    public LifecyclePipelineBuilder Use(LifecycleStage stage, LifecycleMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return Use((context, next) =>
        {
            return context.Stage == stage
                ? middleware(context, next)
                : next();
        });
    }

    public LifecyclePipeline Build(Func<LifecycleContext, ValueTask> terminalHandler)
    {
        ArgumentNullException.ThrowIfNull(terminalHandler);

        return new LifecyclePipeline(_middleware.ToArray(), terminalHandler);
    }
}
