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
}