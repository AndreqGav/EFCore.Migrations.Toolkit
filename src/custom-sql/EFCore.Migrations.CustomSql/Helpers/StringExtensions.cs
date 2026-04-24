namespace EFCore.Migrations.CustomSql.Helpers
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input.Replace("\r\n", "\n");
        }
    }
}