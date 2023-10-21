using CQRS.Core.Commands;
using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public abstract class AggregateRoot
{
    protected Guid _id;

    private readonly List<BaseEvent> _changes = new();

    public Guid Id => _id;

    public int Version { get; set; } = -1;

    public IEnumerable<BaseEvent> GetUncommittedChanges()
    {
        return _changes;
    }

    public void MarkChangesAsCommitted()
    {
        _changes.Clear();
    }

    // @ allows us to use a reserved word
    private void ApplyChanges(BaseEvent @event, bool isNew)
    {
        //Give reflection enough info to match with the overloaded Apply method of the concrete aggregate
        var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });

        if (method is null)
        {
            throw new ArgumentNullException(nameof(method), $"{@event.GetType().Name} does not have an Apply method");
        }

        method.Invoke(this, new object[] { @event });

        if (isNew)
        {
            _changes.Add(@event);
        }
    }

    protected void RaiseEvent(BaseEvent @event)
    {
        ApplyChanges(@event, true);
    }

    public void ReplayEvents(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyChanges(@event, false);
        }
    }
}