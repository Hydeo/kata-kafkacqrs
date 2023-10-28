using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;


namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EditPostController : ControllerBase
{
    private readonly ILogger<EditPostController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public EditPostController(ILogger<EditPostController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    
    [HttpPut("{id}")]
    public async Task<ActionResult> EditMessageAsync(Guid id, EditMessageCommand command)
    {
        try
        {
            command.Id = id;
            await _commandDispatcher.SendAsync(command);

            return StatusCode(StatusCodes.Status200OK, new BaseResponse
            {
                Message = "Message Edited"
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
            return BadRequest(new BaseResponse
            {
                Message = "Bad Request"
            });
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Error while processing EditPost request");
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = "Error while processing the request"
            });
        }
    }
}