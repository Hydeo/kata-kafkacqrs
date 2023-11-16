using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public interface IEventStoreRepository
{
    Task SaveAsync(EventModel @event);
    Task SaveAllAsync(IEnumerable<EventModel> events);
    Task<List<EventModel>> FindByAggregateId(Guid aggregateId);
    Task<List<EventModel>> FindAllAsync();
    Task CommitTransactionalSession();
    Task RollbackTransactionalSession();
}