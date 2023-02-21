using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DemoForum.Repositories;

public class PostRepository : IPostRepository
{
    private readonly ForumContext _context;

    public PostRepository(ForumContext context)
    {
        _context = context;
    }

    public Post Create(Post obj)
    {
        obj.CreatedTime = DateTime.Now;
        EntityEntry<Post> saved = _context.Add(obj);
        _context.SaveChanges();

        return saved.Entity;
    }

    public Post Read(int key)
    {
        throw new NotImplementedException();
    }

    public Post Update(int key, Post obj)
    {
        throw new NotImplementedException();
    }

    public Post Delete(int key)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Post> ReadLatest(int rows)
    {
        return _context.Posts
            .OrderByDescending(p => p.CreatedTime)
            .Take(rows)
            .AsEnumerable();
    }
}