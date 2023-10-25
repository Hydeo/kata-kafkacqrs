using Post.Common.Events;

namespace Post.Query.Infrastructure.Handlers;

public interface IEventHandler
{
    Task On(PostCreatedEvent @event);
    Task On(PostRemovedEvent @event);
    Task On(MessageUpdateEvent @event);
    Task On(PostLikedEvent @event);
    Task On(CommentAddedEvent @event);
    Task On(CommentUpdatedEvent @event);
    Task On(CommentRemovedEvent @event);
}