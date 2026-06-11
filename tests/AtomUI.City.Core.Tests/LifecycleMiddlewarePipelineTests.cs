using AtomUI.City.Lifecycle;

namespace AtomUI.City.Core.Tests;

public sealed class LifecycleMiddlewarePipelineTests
{
    [Fact]
    public async Task PipelineExecutesMiddlewareInRegistrationOrderAroundTerminalHandler()
    {
        var trace = new List<string>();
        var pipeline = new LifecyclePipelineBuilder()
            .Use(async (context, next) =>
            {
                trace.Add("one.before");
                await next().ConfigureAwait(false);
                trace.Add("one.after");
            })
            .Use(async (context, next) =>
            {
                trace.Add("two.before");
                await next().ConfigureAwait(false);
                trace.Add("two.after");
            })
            .Build(context =>
            {
                trace.Add("terminal");

                return ValueTask.CompletedTask;
            });

        await pipeline.ExecuteAsync(new LifecycleContext(LifecycleStages.ApplicationStart));

        Assert.Equal(
            ["one.before", "two.before", "terminal", "two.after", "one.after"],
            trace);
    }

    [Fact]
    public async Task PipelineRunsStageSpecificMiddlewareOnlyForMatchingStage()
    {
        var trace = new List<string>();
        var pipeline = new LifecyclePipelineBuilder()
            .Use(LifecycleStages.ApplicationStart, async (context, next) =>
            {
                trace.Add("start");
                await next().ConfigureAwait(false);
            })
            .Use(LifecycleStages.ApplicationStop, async (context, next) =>
            {
                trace.Add("stop");
                await next().ConfigureAwait(false);
            })
            .Build(context =>
            {
                trace.Add("terminal");

                return ValueTask.CompletedTask;
            });

        await pipeline.ExecuteAsync(new LifecycleContext(LifecycleStages.ApplicationStart));

        Assert.Equal(["start", "terminal"], trace);
    }

    [Fact]
    public async Task MiddlewareCanShortCircuitTerminalHandler()
    {
        var trace = new List<string>();
        var pipeline = new LifecyclePipelineBuilder()
            .Use((context, next) =>
            {
                trace.Add("short");
                context.ShortCircuit();

                return ValueTask.CompletedTask;
            })
            .Build(context =>
            {
                trace.Add("terminal");

                return ValueTask.CompletedTask;
            });

        var context = new LifecycleContext(LifecycleStages.ApplicationStart);

        await pipeline.ExecuteAsync(context);

        Assert.True(context.IsShortCircuited);
        Assert.Equal(["short"], trace);
    }
}
