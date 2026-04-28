using EFCore.Migrations.CustomSql.PostgreSQL.Triggers.Enums;
using EFCore.Migrations.Triggers.Models;

namespace EFCore.Migrations.CustomSql.PostgreSQL.Triggers.Models;

public record PostgreSqlTriggerObject : TriggerObject
{
    public TriggerOperationEnum Operation { get; init; }

    public TriggerTimeEnum Time { get; init; }

    public ConstraintTriggerType? ConstraintType { get; init; }
}