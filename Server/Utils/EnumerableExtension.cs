namespace DemoForum.Utils;

/// <summary>
/// 針對 IENumerable 的擴充方法。
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// 驗證集合是否為 null 或為空。
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source?.Any() != true;
    }
}