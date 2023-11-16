using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Events;
using CQRS.Core.Producers;
using Microsoft.Extensions.Options;

namespace Post.Cmd.Infrastructure.Producers;

public class KafkaEventProducer : IEventProducer
{
    private readonly ProducerConfig _config;
    private IProducer<string, string>? _transactionalProducer;

    public KafkaEventProducer(IOptions<ProducerConfig> config)
    {
        _config = config.Value;
    }

    public async Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent
    {
        using var producer = new ProducerBuilder<string, string>(_config)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .Build();

        var eventMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonSerializer.Serialize(@event, @event.GetType())
        };

        var deliveryResult = await producer.ProduceAsync(topic, eventMessage);

        if (deliveryResult.Status != PersistenceStatus.Persisted)
        {
            throw new Exception(
                $"{@event.GetType()} message to topic '{topic}' could not be produced. Reason : {deliveryResult.Message}");
        }
    }

    public async Task ProduceAllAsync<T>(string topic, IEnumerable<T> events) where T : BaseEvent
    {
        _config.TransactionalId = "test-transaction-id";
        _transactionalProducer = new ProducerBuilder<string, string>(_config)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .Build();


        _transactionalProducer.InitTransactions(new TimeSpan(0, 0, 0, 10));
        _transactionalProducer.BeginTransaction();

        try
        {
            foreach (var @event in events)
            {
                var eventMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = JsonSerializer.Serialize(@event, @event.GetType())
                };

                var deliveryResult = await _transactionalProducer.ProduceAsync(topic, eventMessage);

                if (deliveryResult.Status != PersistenceStatus.Persisted)
                {
                    throw new Exception(
                        $"{@event.GetType()} message to topic '{topic}' could not be produced. Reason : {deliveryResult.Message}");
                }
            }
        }
        catch (Exception e)
        {
            _transactionalProducer.AbortTransaction();
            throw;
        }
    }

    public void CommitTransactionalProducer()
    {
        if (_transactionalProducer != null)
        {
            _transactionalProducer.CommitTransaction();
        }
        else
        {
            throw new Exception("Trying to commit a TransactionalProducer when non is ongoing");
        }

        _transactionalProducer = null;
    }

    public void RollbackTransactionalProducer()
    {
        _transactionalProducer?.AbortTransaction();
        _transactionalProducer = null;
    }
}