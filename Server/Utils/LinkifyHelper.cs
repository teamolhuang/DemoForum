using System.Text.RegularExpressions;

namespace DemoForum.Utils;

public static class LinkifyHelper
{
    const string UrlPattern = "(?:https?://|www\\.)\\S+[/\\b]?";

    /// <summary>
    /// Linkify urls for views.
    /// </summary>
    /// <param name="text">Full string</param>
    /// <returns>string but with a-hrefs</returns>
    public static string Linkify(string text)
    {
        // Replace URLs with HTML hyperlink elements
        return Regex.Replace(text, UrlPattern, m =>
        {
            string url = m.Value.StartsWith("http") ? m.Value : "http://" + m.Value;
            return $"<a href='{url}' target='_blank'>{m.Value}</a>"; // target=_blank means open up a new tab 
        });
    }
}