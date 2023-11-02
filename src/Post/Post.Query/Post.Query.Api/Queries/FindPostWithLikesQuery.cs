using CQRS.Core.Queries;

namespace Post.Query.Api.Queries;

public class FindPostWithLikesQuery : BaseQuery
{
    public int NumberOfOrLikes { get; set; }
}