using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;

namespace Post.Cmd.Infrastructure.Repositories;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IMongoCollection<EventModel> _eventStoreCollection;
    private readonly MongoClient _mongoClient;
    private IClientSessionHandle? _transactionalSession;

    public EventStoreRepository(IOptions<MongoDbConfig> config) //IOption allows us to dep inject the config
    {
        _mongoClient = new MongoClient(config.Value.ConnectionString);
        var mongoDatabase = _mongoClient.GetDatabase(config.Value.Database);
        _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Value.Collection);
    }

    public async Task SaveAsync(EventModel @event)
    {
        await _eventStoreCollection.InsertOneAsync(@event);
    }

    public async Task SaveAllAsync(IEnumerable<EventModel> events)
    {
        _transactionalSession = await _mongoClient.StartSessionAsync();
        _transactionalSession.StartTransaction();
        try
        {
            foreach (var eventModel in events)
            {
                await _eventStoreCollection.InsertOneAsync(_transactionalSession,eventModel);
            }
        }
        catch (Exception ex)
        {
            await _transactionalSession.AbortTransactionAsync();
            throw;
        }
    }

    public async Task CommitTransactionalSession()
    {
        if (_transactionalSession != null)
        {
            await _transactionalSession.CommitTransactionAsync();
        }
        else
        {
            throw new Exception("Trying to commit a TransactionalSession when non is ongoing");
        }

        _transactionalSession = null;
    }

    public async Task RollbackTransactionalSession()
    {
        await _transactionalSession?.AbortTransactionAsync();
        _transactionalSession = null;
    }

    public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
    {
        return await _eventStoreCollection.Find(x => x.AggregateIdentifier == aggregateId).ToListAsync();
    }

    public async Task<List<EventModel>> FindAllAsync()
    {
        return await _eventStoreCollection.Find(_ => true).ToListAsync();
    }
}