namespace AtomUI.City.Lifecycle;

public static class LifecycleStages
{
    public static readonly LifecycleStage ApplicationStart = new(LifecycleStageArea.Application, "Start");
    public static readonly LifecycleStage ApplicationSuspend = new(LifecycleStageArea.Application, "Suspend");
    public static readonly LifecycleStage ApplicationResume = new(LifecycleStageArea.Application, "Resume");
    public static readonly LifecycleStage ApplicationStop = new(LifecycleStageArea.Application, "Stop");

    public static readonly LifecycleStage ModuleInitialize = new(LifecycleStageArea.Module, "Initialize");
    public static readonly LifecycleStage ModuleStart = new(LifecycleStageArea.Module, "Start");
    public static readonly LifecycleStage ModuleStop = new(LifecycleStageArea.Module, "Stop");

    public static readonly LifecycleStage PluginLoad = new(LifecycleStageArea.Plugin, "Load");
    public static readonly LifecycleStage PluginActivate = new(LifecycleStageArea.Plugin, "Activate");
    public static readonly LifecycleStage PluginDeactivate = new(LifecycleStageArea.Plugin, "Deactivate");
    public static readonly LifecycleStage PluginUnload = new(LifecycleStageArea.Plugin, "Unload");

    public static readonly LifecycleStage RouteNavigate = new(LifecycleStageArea.Route, "Navigate");
    public static readonly LifecycleStage RouteEnter = new(LifecycleStageArea.Route, "Enter");
    public static readonly LifecycleStage RouteLeave = new(LifecycleStageArea.Route, "Leave");

    public static readonly LifecycleStage ActivationActivate = new(LifecycleStageArea.Activation, "Activate");
    public static readonly LifecycleStage ActivationDeactivate = new(LifecycleStageArea.Activation, "Deactivate");

    public static readonly LifecycleStage OperationExecute = new(LifecycleStageArea.Operation, "Execute");
    public static readonly LifecycleStage OperationCancel = new(LifecycleStageArea.Operation, "Cancel");
    public static readonly LifecycleStage OperationFail = new(LifecycleStageArea.Operation, "Fail");

    public static readonly LifecycleStage ErrorHandle = new(LifecycleStageArea.Error, "Handle");

    public static IReadOnlyList<LifecycleStage> All { get; } =
    [
        ApplicationStart,
        ApplicationSuspend,
        ApplicationResume,
        ApplicationStop,
        ModuleInitialize,
        ModuleStart,
        ModuleStop,
        PluginLoad,
        PluginActivate,
        PluginDeactivate,
        PluginUnload,
        RouteNavigate,
        RouteEnter,
        RouteLeave,
        ActivationActivate,
        ActivationDeactivate,
        OperationExecute,
        OperationCancel,
        OperationFail,
        ErrorHandle,
    ];
}
