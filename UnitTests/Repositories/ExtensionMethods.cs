using System;

namespace DemoForumTests.Repositories;

public static class ExtensionMethods
{
    public static Byte[] GetNowTimestamp()
        => BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds());
}