using EFCore.Migrations.Triggers.Models;
using EFCore.Migrations.Triggers.SqlServer.Enums;

namespace EFCore.Migrations.Triggers.SqlServer.Models;

public record SqlServerTriggerObject : TriggerObject
{
    public TriggerOperationEnum Operation { get; init; }

    public TriggerTimeEnum Time { get; init; }
}