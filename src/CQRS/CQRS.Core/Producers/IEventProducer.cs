using CQRS.Core.Events;

namespace CQRS.Core.Producers;

public interface IEventProducer
{
    Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent;
    Task ProduceAllAsync<T>(string topic, IEnumerable<T> events) where T : BaseEvent;
    void CommitTransactionalProducer();
    void RollbackTransactionalProducer();
}