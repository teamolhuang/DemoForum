using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface IPostRepository : ICrudRepository<Post, int>
{ 
    Task<IEnumerable<Post>> ReadLatest(int rows);
}