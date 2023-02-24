namespace DemoForum.Enums;

public enum PostMode
{
    Edit,
    New
}

public static class PostModeStrings
{
    public static string GetChinese(this PostMode mode)
    {
        return mode switch
        {
            PostMode.Edit => "編輯",
            _ => "發表"
        };
    }
}