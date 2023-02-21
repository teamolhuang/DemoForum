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
            PostMode.New => "發表",
            _ => "編輯"
        };
    }
}