using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface ICommentRepository : ICrudRepository<Comment, int>
{
    
}