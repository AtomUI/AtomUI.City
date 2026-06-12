using AtomUI.City.EventBus;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventContractRegistryTests
{
    [Fact]
    public void SharedContractDescriptorRequiresSharedAssemblyMatch()
    {
        var contractId = new EventContractId("atomui.city.tests.shared.v1");

        var exception = Assert.Throws<InvalidOperationException>(
            () => EventContractDescriptor.Shared<TestEvent>(contractId, typeof(string).Assembly));

        Assert.Contains(typeof(TestEvent).FullName!, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ContractRegistryRejectsDuplicateContractId()
    {
        var contractId = new EventContractId("atomui.city.tests.duplicate.v1");
        var registry = new InMemoryEventContractRegistry();

        registry.Register(EventContractDescriptor.Shared<TestEvent>(contractId, typeof(TestEvent).Assembly));

        var exception = Assert.Throws<InvalidOperationException>(
            () => registry.Register(EventContractDescriptor.Shared<OtherEvent>(contractId, typeof(OtherEvent).Assembly)));

        Assert.Contains(contractId.Value, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ContractRegistryReturnsDefaultDescriptorForUnregisteredInternalEvent()
    {
        var registry = new InMemoryEventContractRegistry();

        var descriptor = registry.GetOrCreate<TestEvent>();

        Assert.Equal(new EventContractId(typeof(TestEvent).FullName!), descriptor.ContractId);
        Assert.Equal(EventContractPlane.Shared, descriptor.Plane);
        Assert.Equal(typeof(TestEvent), descriptor.EventType);
    }

    [Fact]
    public void ContractRegistryKeepsDefaultDescriptorMappingStable()
    {
        var registry = new InMemoryEventContractRegistry();
        var descriptor = registry.GetOrCreate<TestEvent>();
        var changedContractId = new EventContractId("atomui.city.tests.changed.v1");

        var exception = Assert.Throws<InvalidOperationException>(
            () => registry.Register(EventContractDescriptor.Shared<TestEvent>(changedContractId, typeof(TestEvent).Assembly)));

        Assert.Contains(typeof(TestEvent).FullName!, exception.Message, StringComparison.Ordinal);
        Assert.Equal(descriptor.ContractId, registry.GetOrCreate<TestEvent>().ContractId);
    }

    private sealed record TestEvent(string Value);

    private sealed record OtherEvent(string Value);
}
