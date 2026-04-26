namespace EFCore.Migrations.CustomSql.Constants;

public static class CustomSqlConstants
{
    public const string Sql = "Sql";

    public static readonly string SqlUp = $"{Sql}Up:";

    public static readonly string SqlDown = $"{Sql}Down:";
}