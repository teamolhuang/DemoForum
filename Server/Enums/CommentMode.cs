namespace DemoForum.Enums;

public enum CommentMode
{
    Push,
    Boo,
    Natural
}

public static class CommentModeHelper
{
    public static string GetChinese(this CommentMode mode)
    {
        return mode switch
        {
            CommentMode.Push => "推",
            CommentMode.Boo => "噓",
            _ => "→"
        };
    }

    public static string GetDbEnum(this CommentMode mode)
    {
        // N for Natural, default
        return Enum.GetName(mode)?.Substring(0, 1) ?? "N";
    }

    public static CommentMode ByDbType(string dbType)
    {
        return dbType switch
        {
            "P" => CommentMode.Push,
            "B" => CommentMode.Boo,
            _ => CommentMode.Natural
        };
    }
    
    public static string GetCssSuffix(this CommentMode mode)
    {
        return Enum.GetName(mode)?.ToLower() ?? "natural";
    }

    public static int GetCommentScore(this CommentMode mode)
    {
        return mode switch
        {
            CommentMode.Push => 1,
            CommentMode.Boo => -1,
            _ => 0
        };
    }
}