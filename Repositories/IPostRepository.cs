using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface IPostRepository : ICrudRepository<Post, int>
{
    
}