using System;

namespace AncientCloud.EventStore;

public interface IProjection
{
    void Init();
    Type[] Handles { get; }
    Task Handle(object @event, CancellationToken ct);
}
