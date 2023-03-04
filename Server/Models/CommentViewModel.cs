using DemoForum.Enums;

namespace DemoForum.Models;

public class CommentViewModel
{
    public CommentMode CommentMode { get; set; }
    public string CommentModeChinese => CommentMode.GetChinese();
    public string CommentModeCssSuffix => CommentMode.GetCssSuffix();

    public int Floor { get; set; }
    
    public string? Username { get; set; }
    public string? Content { get; set; }
    public string? CreatedTime { get; set; }
}