using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DemoForum.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ForumContext _forumContext;

    public CommentRepository(ForumContext forumContext)
    {
        _forumContext = forumContext;
    }
    
    public async Task<Comment> Create(Comment obj)
    {
        EntityEntry<Comment> entity = await _forumContext.Comments.AddAsync(obj);
        await _forumContext.SaveChangesAsync();
        return entity.Entity;
    }

    public Task<Comment?> Read(int key)
    {
        throw new NotImplementedException();
    }

    public Task<Comment> Update(int key, Comment obj)
    {
        throw new NotImplementedException();
    }

    public Task<Comment> Delete(int key)
    {
        throw new NotImplementedException();
    }
}