using EFCore.Migrations.Triggers.Models;
using EFCore.Migrations.Triggers.PostgreSQL.Enums;

namespace EFCore.Migrations.Triggers.PostgreSQL.Models
{
    public record PostgreSqlTriggerObject : TriggerObject
    {
        public TriggerOperationEnum Operation { get; init; }

        public TriggerTimeEnum Time { get; init; }

        public ConstraintTriggerType? ConstraintType { get; init; }
    }

}