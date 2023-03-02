namespace DemoForum.Models.Entities;

public partial class User
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Id { get; set; }

    public byte[] Version { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; } = new List<Post>();
}
