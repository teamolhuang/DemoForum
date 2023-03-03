using System.ComponentModel.DataAnnotations;
using DemoForum.Enums;

namespace DemoForum.Models;

public class EditorViewModel
{
    public int EntityId { get; set; }

    [StringLength(20, ErrorMessage = "標題長度應為 1 ~ 20 個字！")]
    [Required(ErrorMessage = "未填入標題！")]
    public string? PostTitle { get; set; }

    [StringLength(1000, MinimumLength = 10, ErrorMessage = "文章內容應為 10 ~ 1000 個字！")]
    [Required(ErrorMessage = "未填入文章內容！")]
    public string? PostContent { get; set; }

    public PostMode PostMode { get; set; }

    public string PostModeString => PostMode.GetChinese();
}