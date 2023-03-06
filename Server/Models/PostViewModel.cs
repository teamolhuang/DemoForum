namespace DemoForum.Models;

public class PostViewModel
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CreatedTime { get; set; }
    public string? AuthorName { get; set; }
    public string? UpdatedTime { get; set; }
    public int? CommentScore { get; set; }

    public IEnumerable<CommentViewModel>? CommentViews { get; set; }
}