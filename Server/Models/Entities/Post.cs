namespace DemoForum.Models.Entities;

public partial class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public int AuthorId { get; set; }

    public virtual User Author { get; set; } = null!;
}
