using System;

namespace AncientCloud.EventStore;

public interface IEventStore
{
    void Init();

    void RegisterProjection(IProjection projection);

    Task AppendEvents<TStream>(
        Guid streamId, IEnumerable<object> @events, long expectedVersion, CancellationToken ct = default
    ) where TStream : notnull;

    Task<IReadOnlyCollection<object>> GetEventsAsync(
        Guid streamId, long? atVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default
    );
}
