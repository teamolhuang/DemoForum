using System.ComponentModel.DataAnnotations;

namespace DemoForum.Models;

public class LoginViewModel
{
    [RegularExpression("^[A-z0-9]+$", ErrorMessage = "帳號只允許輸入半形大小寫英文、半形數字！")]
    [StringLength(12, ErrorMessage = "帳號長度應為 1 ~ 12 個字！")]
    [Required(ErrorMessage = "未填入帳號！")]
    public string? Username { get; set; }
    
    [RegularExpression("^[A-z0-9_%$!#]+$", ErrorMessage = "密碼只允許輸入半形大小寫英文、半形數字，及 _ % $ ! # 符號！")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼長度應為 8 ~ 20 個字！")]
    [Required(ErrorMessage = "未填入密碼！")]
    public string? Password { get; set; }
}