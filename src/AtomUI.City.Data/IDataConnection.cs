namespace AtomUI.City.Data;

public interface IDataConnection
{
    string ConnectionId { get; }

    DataConnectionOwner Owner { get; }

    DataConnectionState State { get; }

    ValueTask StartAsync(CancellationToken cancellationToken = default);

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
