using CQRS.Core.Domain;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregates;

public class PostAggregate : AggregateRoot
{
    private bool _active;
    private string _author;
    private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();

    public bool Active
    {
        get => _active;
        set => _active = value;
    }
    
    public PostAggregate(){}

    public PostAggregate(Guid id, string author, string message)
    {
        RaiseEvent(new PostCreatedEvent
        {
            Id = id,
            Author = author,
            Message = message,
            DatePosted = DateTime.UtcNow
        });
    }

    public void Apply(PostCreatedEvent @event)
    {
        _id = @event.Id;
        _active = true;
        _author = @event.Author;
    }

    public void EditMessage(string message)
    {
        if (!_active)
        {
            throw new InvalidOperationException("A inactive post cannot be edited");
        }

        if (string.IsNullOrWhiteSpace((message)))
        {
            throw new InvalidOperationException($"{nameof(message)} cannot be null or empty.");
        }
        
        RaiseEvent(new MessageUpdateEvent
        {
            Id = _id,
            Message = message
        });
    }

    public void Apply(MessageUpdateEvent @event)
    {
        _id = @event.Id;
    }

    public void LikePost()
    {
        if (!_active)
        {
            throw new InvalidOperationException("A inactive post cannot be liked");
        }
        RaiseEvent(new PostLikedEvent
        {
            Id = _id
        });
    }

    public void Apply(PostLikedEvent @event)
    {
        _id = @event.Id;
    }

    public void AddComment(string comment, string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("A inactive post cannot be commented");
        }
        if (string.IsNullOrWhiteSpace((comment)))
        {
            throw new InvalidOperationException($"{nameof(comment)} cannot be null or empty.");
        }
        RaiseEvent(new CommentAddedEvent
        {
            Id = _id,
            CommentId = Guid.NewGuid(),
            Comment = comment,
            Username = username,
            CommentDate = DateTime.UtcNow
        });
    }

    public void Apply(CommentAddedEvent @event)
    {
        _id = @event.Id;
        _comments.Add(@event.CommentId,new Tuple<string, string>(@event.Comment,@event.Username));
    }

    public void EditComment(Guid commentId, string comment, string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("A inactive post comment cannot be edited");
        }
        if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException($"A comment can only be edited by the user who created it");
        }
        if (string.IsNullOrWhiteSpace((comment)))
        {
            throw new InvalidOperationException($"{nameof(comment)} cannot be null or empty.");
        }
        
        RaiseEvent(new CommentUpdatedEvent
        {
            Id = _id,
            CommentId = commentId,
            Comment = comment,
            Username = username,
            EditDate = DateTime.UtcNow
        });
        
    }

    public void Apply(CommentUpdatedEvent @event)
    {
        _id = @event.Id;
        _comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.Username);
    }

    public void RemoveComment(Guid commentId, string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("A inactive post comment cannot be deleted");
        }
        if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException($"A comment can only be removed by the user who created it");
        }
        RaiseEvent(new CommentRemovedEvent
        {
            Id = _id,
            CommentId = commentId
        });
    }

    public void Apply(CommentRemovedEvent @event)
    {
        _id = @event.Id;
        _comments.Remove(@event.CommentId);
    }

    public void DeletePost(string username)
    {
        if (!_active)
        {
            throw new InvalidOperationException("Post already deleted");
        }
        if (!_author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException($"A post can only be deleted by the user who created it");
        }
        RaiseEvent(new PostRemovedEvent
        {
            Id = _id
        });
    }

    public void Apply(PostRemovedEvent @event)
    {
        _id = @event.Id;
        _active = false;
    }
}