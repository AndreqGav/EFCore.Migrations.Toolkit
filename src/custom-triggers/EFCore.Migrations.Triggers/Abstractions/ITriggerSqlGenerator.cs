using EFCore.Migrations.Triggers.Models;

namespace EFCore.Migrations.Triggers.Abstractions
{
    public interface ITriggerSqlGenerator
    {
        string GenerateCreateTriggerSql(TriggerObject trigger);

        string GenerateDeleteTriggerSql(TriggerObject trigger);
    }
}