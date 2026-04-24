using EFCore.Migrations.Triggers.Constants;
using EFCore.Migrations.Triggers.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.Triggers
{
    public static class TriggersExtensions
    {
        public static void AddTriggerAnnotation<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            TriggerObject trigger) where TEntity : class
        {
            entityTypeBuilder.HasAnnotation($"{SqlTriggerConstants.Trigger}_{trigger.GetHashCode()}", trigger);
        }
    }
}
