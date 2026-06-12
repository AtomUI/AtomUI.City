using AtomUI.City.Diagnostics;
using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class ApplicationStateTests
{
    [Fact]
    public void RegistryReadsRegisteredApplicationStateByKey()
    {
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry();

        registry.Add(StateDefinition.Create(key, "light"));

        IApplicationState state = registry;
        var theme = state.Get(key);

        Assert.Equal("light", theme.Value);
        Assert.Equal(0, theme.Version);
    }

    [Fact]
    public void WriterUpdatesRegisteredApplicationState()
    {
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry();
        registry.Add(StateDefinition.Create(key, "light"));

        IApplicationStateWriter writer = registry;

        var changed = writer.Set(key, "dark");

        Assert.True(changed);
        Assert.Equal("dark", registry.Get(key).Value);
    }

    [Fact]
    public void ReadOnlyStateDefinitionRejectsApplicationWriter()
    {
        var key = new StateKey<string>("AtomUI.City.Tests.ReadOnly");
        var registry = new ApplicationStateRegistry();
        registry.Add(StateDefinition.Create(key, "fixed", access: StateAccessPolicy.ReadOnly));

        var exception = Assert.Throws<StateAccessDeniedException>(
            () => registry.Set(key, "changed"));

        Assert.Equal(key.Name, exception.StateName);
        Assert.Equal("fixed", registry.Get(key).Value);
    }

    [Fact]
    public void ReadOnlyStateDefinitionRecordsWriteDeniedDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.ReadOnly");
        var registry = new ApplicationStateRegistry(diagnostics);
        registry.Add(StateDefinition.Create(key, "fixed", access: StateAccessPolicy.ReadOnly));

        var exception = Assert.Throws<StateAccessDeniedException>(
            () => registry.Set(key, "changed"));

        Assert.Equal(key.Name, exception.StateName);
        Assert.Equal("fixed", registry.Get(key).Value);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.ApplicationStateWriteDenied, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingApplicationStateDoesNotCreateImplicitState()
    {
        var registry = new ApplicationStateRegistry();
        var key = new StateKey<int>("AtomUI.City.Tests.Missing");

        var exception = Assert.Throws<StateNotRegisteredException>(
            () => registry.Get(key));

        Assert.Equal(key.Name, exception.StateName);
    }

    [Fact]
    public void MissingApplicationStateRecordsDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ApplicationStateRegistry(diagnostics);
        var key = new StateKey<int>("AtomUI.City.Tests.Missing");

        var exception = Assert.Throws<StateNotRegisteredException>(
            () => registry.Get(key));

        Assert.Equal(key.Name, exception.StateName);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.ApplicationStateNotRegistered, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateApplicationStateRegistrationRecordsDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ApplicationStateRegistry(diagnostics);
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        registry.Add(StateDefinition.Create(key, "light"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => registry.Add(StateDefinition.Create(key, "dark")));

        Assert.Contains(key.Name, exception.Message, StringComparison.Ordinal);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal("AUCSTA008", record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
    }
}
