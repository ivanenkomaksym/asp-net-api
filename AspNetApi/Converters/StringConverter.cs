namespace AspNetApi.Converters
{
    public static class StringConverter
    {
        public static string ToCamelCaseFromPascal(string str)
        {
            return char.ToLowerInvariant(str[0]) + str[1..];
        }
    }
}
