namespace DemoForum.Utils;

public static class Consts
{
    public const int LoginExpirationMinutes = 20;
    public const string CookieKey = "demoforum-cookie";
    public const string AntiForgeryCookie = "demoforum-anti-forgery-cookie";
    public const string AntiForgeryCookieHeader = "X-CSRF-TOKEN";
}