namespace DemoForum.Models;

public class CommentInputViewModel
{
    public string? PushChinese { get; set; }
    public string? BooChinese { get; set; }
    public string? NaturalChinese { get; set; }
    public bool? IsUserAuthor { get; set; }
    public int? PostId { get; set; }
    
    public string? CommentContent { get; set; }
}