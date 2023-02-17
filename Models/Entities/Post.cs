namespace DemoForum.Models.Entities;

public partial class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;
}
