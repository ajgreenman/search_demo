namespace Search.Extensions
{
    public static class StringExtensions
    {
        public static string ToSolrReadyString(this string guid)
        {
            return guid.Replace("{", "").Replace("}", "").Replace("-", "").ToLower();
        }
    }
}