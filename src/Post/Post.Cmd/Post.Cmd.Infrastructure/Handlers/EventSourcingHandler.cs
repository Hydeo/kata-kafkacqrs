using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers;

public class EventSourcingHandler: IEventSourcingHandler<PostAggregate>
{
    private readonly IEventStore _eventStore;

    public EventSourcingHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task SaveAsync(AggregateRoot aggregate)
    {
        await _eventStore.SaveEventAsync(aggregate.Id,aggregate.GetUncommittedChanges(),aggregate.Version);
        aggregate.MarkChangesAsCommitted();
    }

    public async Task<PostAggregate> GetByIdAsync(Guid id)
    {
        var aggregate = new PostAggregate();
        var events = await _eventStore.GetEventsAsync(id);
        
        if (events is null || !events.Any()) return aggregate;
        
        aggregate.ReplayEvents(events);
        var latestVersion = events.Select(x => x.Version).Max();
        aggregate.Version = latestVersion;

        return aggregate;
    }
}