namespace DemoForum.Models;

public class PostViewModel
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CreatedTime { get; set; }
    public string? AuthorName { get; set; }
}