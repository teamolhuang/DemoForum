namespace DemoForum.Enums;

public enum CommentMode
{
    Push,
    Boo,
    Natural
}

public static class CommentModeStrings
{
    public static string GetChinese(this CommentMode mode)
    {
        return mode switch
        {
            CommentMode.Push => "推",
            CommentMode.Boo => "噓",
            _ => "箭頭"
        };
    }

    public static string GetDbEnum(this CommentMode mode)
    {
        // N for Natural, default
        return Enum.GetName(mode)?.Substring(0, 1) ?? "N";
    }
}