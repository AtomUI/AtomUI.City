using AtomUI.City.Lifecycle;

namespace AtomUI.City.Core.Tests;

public sealed class LifecycleStageModelTests
{
    [Fact]
    public void BuiltInStagesExposeStableKeysAndDeterministicOrder()
    {
        var stages = LifecycleStages.All;

        Assert.Equal(
            [
                "Application.Start",
                "Application.Suspend",
                "Application.Resume",
                "Application.Stop",
                "Module.Initialize",
                "Module.Start",
                "Module.Stop",
                "Plugin.Load",
                "Plugin.Activate",
                "Plugin.Deactivate",
                "Plugin.Unload",
                "Route.Navigate",
                "Route.Enter",
                "Route.Leave",
                "Activation.Activate",
                "Activation.Deactivate",
                "Operation.Execute",
                "Operation.Cancel",
                "Operation.Fail",
                "Error.Handle",
            ],
            stages.Select(stage => stage.Key));
    }

    [Fact]
    public void LifecycleStageValueUsesAreaAndNameForIdentity()
    {
        var stage = new LifecycleStage(LifecycleStageArea.Application, "Start");

        Assert.Equal("Application", stage.AreaName);
        Assert.Equal("Start", stage.Name);
        Assert.Equal("Application.Start", stage.Key);
        Assert.Equal(stage, LifecycleStages.ApplicationStart);
    }

    [Fact]
    public void ScopeKindsAndStatesExposeCoreLifecycleVocabulary()
    {
        Assert.Contains(LifecycleScopeKind.Host, Enum.GetValues<LifecycleScopeKind>());
        Assert.Contains(LifecycleScopeKind.Navigation, Enum.GetValues<LifecycleScopeKind>());
        Assert.Contains(LifecycleScopeKind.Operation, Enum.GetValues<LifecycleScopeKind>());
        Assert.True(LifecycleScopeState.Created < LifecycleScopeState.Running);
        Assert.True(LifecycleScopeState.Disposing < LifecycleScopeState.Disposed);
    }
}
