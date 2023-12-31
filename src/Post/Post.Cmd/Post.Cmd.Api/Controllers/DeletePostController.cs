using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DeletePostController : ControllerBase
{
    private readonly ILogger<DeletePostController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public DeletePostController(ILogger<DeletePostController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePostAsync(Guid id, DeletePostCommand command)
    {

        try
        {
            command.Id = id;
            await _commandDispatcher.SendAsync(command);

            return StatusCode(StatusCodes.Status204NoContent, new BaseResponse
            {
                Message = "Comment deleted"
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
            _logger.Log(LogLevel.Error, ex, "Error while processing DeletePost request");
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse()
            {
                Message = "Error while processing the request"
            });
        }
    }
}