namespace EFCore.Migrations.CustomSql.PostgreSQL.Triggers.Enums;

public enum TriggerOperationEnum
{
    Insert = 1,

    Update = 2,

    Delete = 3,

    InsertOrUpdate = 4,
}