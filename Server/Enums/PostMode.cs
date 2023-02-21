namespace DemoForum.Enums;

public enum PostMode
{
    Edit,
    New
}

internal static class PostModeStrings
{
    internal static string GetChinese(this PostMode? mode)
    {
        return mode switch
        {
            PostMode.New => "發表",
            _ => "編輯"
        };
    }
}