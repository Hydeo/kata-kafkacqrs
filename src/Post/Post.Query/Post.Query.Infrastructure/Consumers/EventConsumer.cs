using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;

namespace Post.Query.Infrastructure.Consumers;

public class EventConsumer :IEventConsumer
{
    private readonly ConsumerConfig _config;
    private IEventHandler _eventHandler;

    public EventConsumer(IOptions<ConsumerConfig> config,IEventHandler eventHandler)
    {
        _config = config.Value;
        _eventHandler = eventHandler;
    }

    public void Consume(string topic)
    {
        using var kafkaConsumer = new ConsumerBuilder<string, string>(_config)
            .SetKeyDeserializer(Deserializers.Utf8)
            .SetValueDeserializer(Deserializers.Utf8)
            .Build();
        
        kafkaConsumer.Subscribe(topic);

        while (true)
        {
            var kafkaConsumerResult = kafkaConsumer.Consume();
            if(kafkaConsumerResult?.Message == null) continue;

            var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } }; //Important to pass the custom converter to allow polymorphic use 
            var @event = JsonSerializer.Deserialize<BaseEvent>(kafkaConsumerResult.Message.Value, options);

            var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() }); //Reflection : This will select the "On" method with the correctly typed parameter

            if (handlerMethod is null)
            {
                throw new ArgumentNullException(nameof(handlerMethod), "Could not find matching event handler method");
            }

            handlerMethod.Invoke(_eventHandler,new object[]{@event});
            
            kafkaConsumer.Commit(kafkaConsumerResult);
        }
        
    }
}