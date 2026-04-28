using EFCore.Migrations.CustomSql.SqlServer.Enums;
using EFCore.Migrations.Triggers.Models;

namespace EFCore.Migrations.CustomSql.SqlServer.Models;

public record SqlServerTriggerObject : TriggerObject
{
    public TriggerOperationEnum Operation { get; init; }

    public TriggerTimeEnum Time { get; init; }
}