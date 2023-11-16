using System.Data;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores;

public class EventStore : IEventStore
{
    private readonly IEventStoreRepository _eventStoreRepository;
    private readonly IEventProducer _eventProducer;

    public EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer)
    {
        _eventStoreRepository = eventStoreRepository;
        _eventProducer = eventProducer;
    }

    public async Task SaveEventAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
    {
        var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

        //Optimistic Concurrency Control Check
        if (expectedVersion != -1 && eventStream[eventStream.Count - 1].Version != expectedVersion)
        {
            throw new ConcurrencyException();
        }

        var version = expectedVersion;
        var eventModels = new List<EventModel>();
        foreach (var @event in events)
        {
            version++;
            @event.Version = version;
            eventModels.Add(new EventModel
            {
                Version = version,
                EventData = @event,
                AggregateIdentifier = aggregateId,
                EventType = @event.Type,
                TimeStamp = DateTime.UtcNow,
                AggregateType = nameof(PostAggregate)
            });
        }
        
        try
        {
            await _eventStoreRepository.SaveAllAsync(eventModels);
            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            await _eventProducer.ProduceAllAsync(topic, events);

            await _eventStoreRepository.CommitTransactionalSession();
            _eventProducer.CommitTransactionalProducer();
        }
        catch (Exception ex)
        {
            await _eventStoreRepository.RollbackTransactionalSession();
            _eventProducer.RollbackTransactionalProducer();
            throw;
        }
    }

    public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
    {
        var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

        if (eventStream is null || !eventStream.Any())
        {
            throw new AggregateNotFoundException($"Invalid {nameof(Post)} ID provided");
        }

        return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
    }

    public async Task<List<Guid>> GetAggregateIdsAsync()
    {
        var eventStream = await _eventStoreRepository.FindAllAsync();

        if (eventStream is null || !eventStream.Any())
        {
            throw new ArgumentNullException(nameof(eventStream), "Could not retrieve all events");
        }

        return eventStream.Select(x => x.AggregateIdentifier).Distinct().ToList();
    }
}