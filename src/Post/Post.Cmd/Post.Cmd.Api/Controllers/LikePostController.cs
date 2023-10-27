using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LikePostController : ControllerBase
{
    private readonly ILogger<LikePostController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public LikePostController(ILogger<LikePostController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> LikePostAsync(Guid id)
    {
        try
        {
            await _commandDispatcher.SendAsync(new LikePostCommand
            {
                Id = id
            });

            return StatusCode(StatusCodes.Status200OK, new BaseResponse
            {
                Message = "Message Liked"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Log(LogLevel.Warning, ex, "Client made a Bad Request");
            return BadRequest(new BaseResponse
            {
                Message = "Bad Request"
            });
        }
        catch (AggregateNotFoundException ex)
        {
            _logger.Log(LogLevel.Error, ex, "Couldn't not retrieve aggregate");
            return BadRequest(new NewPostResponse
            {
                Id = id,
                Message = "Bad Request"
            });
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Error while processing LikePost request");
            return StatusCode(StatusCodes.Status500InternalServerError, new NewPostResponse
            {
                Id = id,
                Message = "Error while processing the request"
            });
        }
    }
}