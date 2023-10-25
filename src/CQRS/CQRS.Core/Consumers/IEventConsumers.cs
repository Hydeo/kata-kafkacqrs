namespace CQRS.Core.Consumers;

public interface IEventConsumers
{
    void Consume(string topic);
}