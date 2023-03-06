namespace DemoForum.Models;

public class PostPreviewViewModel
{
    public string? Id { get; set; }
    public string? AuthorName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CreatedTime { get; set; }
    public int? CommentScore { get; set; }
}