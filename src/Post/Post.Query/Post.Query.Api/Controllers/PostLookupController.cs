using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostLookupController : ControllerBase
{
    private readonly ILogger<PostLookupController> _logger;
    private readonly IQueryDispatcher<PostEntity> _queryDispatcher;

    public PostLookupController(ILogger<PostLookupController> logger, IQueryDispatcher<PostEntity> queryDispatcher)
    {
        _logger = logger;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPostAsync()
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindAllPostsQuery());

            if (!posts.Any())
            {
                return NoContent();
            }

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = posts.Count.ToString()
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "An error occured while retrieving all posts";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }

    [HttpPost("{postId}")]
    public async Task<ActionResult> GetByIdAsync(Guid postId)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostByIdQuery
            {
                id = postId
            });

            if (!posts.Any())
            {
                return NoContent();
            }

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = posts.Count.ToString()
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "An error occured while retrieving a posts";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
    
    [HttpPost("byAuthor/{author}")]
    public async Task<ActionResult> GetByAuthorAsync(string author)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostByAuthorQuery()
            {
                Author = author
            });

            if (!posts.Any())
            {
                return NoContent();
            }

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = posts.Count.ToString()
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "An error occured while retrieving all posts by author";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
    
    [HttpPost("withComments")]
    public async Task<ActionResult> GetWithCommentsAsync(string author)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostWithCommentsQuery());

            if (!posts.Any())
            {
                return NoContent();
            }

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = posts.Count.ToString()
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "An error occured while retrieving all posts with comment";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
    
    [HttpPost("withComments")]
    public async Task<ActionResult> GetWithLikeAsync(int minimumLikes)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostWithLikesQuery
            {
                NumberOfOrLikes = minimumLikes
            });

            if (!posts.Any())
            {
                return NoContent();
            }

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = posts.Count.ToString()
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = $"An error occured while retrieving all posts with at minimunLikes";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}