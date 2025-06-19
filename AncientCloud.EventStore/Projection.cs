using System;

namespace AncientCloud.EventStore;

public abstract class Projection : IProjection
{
    private readonly Dictionary<Type, Func<object, CancellationToken, Task>> _handlers = new();
    public Type[] Handles => this._handlers.Keys.ToArray();

    protected void projects<TEvent>(Func<TEvent, CancellationToken, Task> action)
    {
        this._handlers.Add(typeof(TEvent), (@event, ct) =>
        {
            return action((TEvent)@event, ct);
        });
    }

    public Task Handle(object @event, CancellationToken ct)
    {
        return this._handlers[@event.GetType()](@event, ct);
    }

    public virtual void Init() { }
}
