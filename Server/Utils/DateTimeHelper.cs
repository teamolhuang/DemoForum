namespace DemoForum.Utils;

public static class DateTimeHelper
{
    public static string ToStringForView(this DateTime dt)
    {
        return dt.ToString("yyyy/MM/dd HH:mm:ss");
    }
}