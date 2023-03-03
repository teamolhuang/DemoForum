using System;
using System.Collections.Generic;

namespace DemoForum.Models.Entities;

public partial class Comment
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public int PostId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// P, B, N
    /// </summary>
    public string Type { get; set; } = null!;

    public virtual User Author { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
