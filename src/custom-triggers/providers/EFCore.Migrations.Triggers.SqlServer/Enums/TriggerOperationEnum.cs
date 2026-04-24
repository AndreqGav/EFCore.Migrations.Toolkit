namespace EFCore.Migrations.Triggers.SqlServer.Enums
{
    public enum TriggerOperationEnum
    {
        Insert = 1,

        Update = 2,

        Delete = 3,

        InsertOrUpdate = 4,

        InsertOrDelete = 5,

        UpdateOrDelete = 6,

        InsertOrUpdateOrDelete = 7,
    }
}