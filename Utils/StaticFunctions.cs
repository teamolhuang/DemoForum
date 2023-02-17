namespace DemoForum.Utils;

public static class StaticFunctions
{
    /// <summary>
    /// Shortens a string into a format more fit for previewing.<br/>
    /// Length 20, with suffix <u>...</u> as indicator.<br/>
    /// This is for occasions where optional parameters are not supported, <br/>
    /// for other situation, simply use ShortenToPreview() instead.
    /// </summary>
    /// <param name="s">string</param>
    /// <returns>formatted string</returns>
    public static string ShortenToPreviewDefault(this string s) => s.ShortenToPreview();

    /// <summary>
    /// Shortens a string into a format more fit for previewing.<br/>
    /// With default suffix <u>...</u> as indicator.<br/>
    /// This is for occasions where optional parameters are not supported, <br/>
    /// for other situation, simply use ShortenToPreview() instead.
    /// </summary>
    /// <param name="s">string</param>
    /// <param name="length">max length</param>
    /// <returns>formatted string</returns>
    public static string ShortenToPreviewDefault(this string s, int length) => s.ShortenToPreview(length);
    
    /// <summary>
    /// Shortens a string into a format more fit for previewing.
    /// </summary>
    /// <param name="s">string</param>
    /// <param name="length">max length</param>
    /// <param name="suffix">indicator suffix if shortened</param>
    /// <returns>formatted string</returns>
    public static string ShortenToPreview(this string s, int length = 20, string suffix = " ...") => s.Length > length ? s.Substring(0, length) + suffix : s;
}