using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class QueryHandler :IQueryHandler
{
    private readonly IPostRepository _postRepository;

    public QueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> HandleAsync(FindAllPostsQuery query)
    {
        return await _postRepository.ListAllAsync();
    }

    public async Task<List<PostEntity>> HandleAsync(FindPostByIdQuery query)
    {
        var post= await _postRepository.GetByIdAsync(query.id);
        return new List<PostEntity> { post };
    }

    public async Task<List<PostEntity>> HandleAsync(FindPostByAuthorQuery query)
    {
        return await _postRepository.ListByAuthorAsync(query.Author);
    }

    public async Task<List<PostEntity>> HandleAsync(FindPostWithCommentsQuery query)
    {
        return await _postRepository.ListWithCommentAsync();
    }

    public async Task<List<PostEntity>> HandleAsync(FindPostWithLikesQuery query)
    {
        return await _postRepository.ListWithLikesAsync(query.NumberOfOrLikes);
    }
}