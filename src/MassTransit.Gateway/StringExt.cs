namespace MassTransit.Gateway
{
    public static class StringExt
    {
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrWhiteSpace(value);
    }
}