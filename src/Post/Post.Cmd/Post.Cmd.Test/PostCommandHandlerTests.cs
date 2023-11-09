using System;
using System.Threading.Tasks;
using CQRS.Core.Handlers;
using Moq;
using Post.Cmd.Api.Commands;
using Post.Cmd.Domain.Aggregates;
using Xunit;

namespace Post.Cmd.Test;

public class PostCommandHandlerTests
{
    private readonly Mock<IEventSourcingHandler<PostAggregate>> _eventSourcingHandler;

    public PostCommandHandlerTests()
    {
        _eventSourcingHandler = new Mock<IEventSourcingHandler<PostAggregate>>();
    }

    [Fact]
    public async Task Handler_Should_Return_InvalidOperationException_Message_Cannot_Be_Empty()
    {
        //Arrange
        var aggregate = new PostAggregate(Guid.NewGuid(), "EditPostCommandTest Author", "EditPostCommandTest Message");
        CommandHandler commandHandler = new CommandHandler(_eventSourcingHandler.Object);
        _eventSourcingHandler
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(aggregate);
        
        //Act
        async Task Act() => await commandHandler.HandleAsync(new EditMessageCommand() { Message = "" });

        var ex = await Record.ExceptionAsync(Act);
        //Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("cannot be null or empty.",ex.Message);
    }
    
    [Fact]
    public async Task Handler_Should_Return_InvalidOperationException_Inactive_Post_Cannot_Be_Edited()
    {
        //Arrange
        var aggregate = new PostAggregate(Guid.NewGuid(), "EditPostCommandTest Author", "EditPostCommandTest Message")
            {
                Active = false
            };
        CommandHandler commandHandler = new CommandHandler(_eventSourcingHandler.Object);
        _eventSourcingHandler
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(aggregate);
        
        //Act
        async Task Act() => await commandHandler.HandleAsync(new EditMessageCommand() { Message = "" });
        var ex = await Record.ExceptionAsync(Act);
        
        //Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("A inactive post cannot be edited",ex.Message);
    }
}