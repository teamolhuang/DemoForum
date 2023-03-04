using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DemoForum.Repositories;

public class PostRepository : IPostRepository
{
    private readonly ForumContext _context;

    public PostRepository(ForumContext context)
    {
        _context = context;
    }

    public async Task<Post> Create(Post obj)
    {
        obj.CreatedTime = DateTime.Now;
        EntityEntry<Post> saved = await _context.AddAsync(obj);
        await _context.SaveChangesAsync();

        return saved.Entity;
    }

    public async Task<Post?> Read(int key)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.Id == key);
    }

    public async Task<Post> Update(int key, Post obj)
    {
        Post? original = await Read(key);
        if (original == null)
            throw new NullReferenceException($"Specified post id not found in DB! id: {key}");

        original.Title = obj.Title;
        original.Content = obj.Content;
        original.UpdatedTime = DateTime.Now;

        _context.Posts.Update(original);
        await _context.SaveChangesAsync();
        return original;
    }

    public async Task<Post> Delete(int key)
    {
        Post? original = await Read(key);
        if (original == null)
            throw new NullReferenceException($"Entity to be deleted already removed from DB. id: {key}");

        _context.Posts.Remove(original);
        await _context.SaveChangesAsync();
        return original;
    }

    public async Task<IEnumerable<Post>> ReadLatest(int rows)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedTime)
            .Take(rows)
            .ToListAsync();
    }
}