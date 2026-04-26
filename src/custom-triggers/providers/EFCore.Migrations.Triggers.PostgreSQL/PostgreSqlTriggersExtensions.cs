using EFCore.Migrations.Triggers.PostgreSQL.Enums;
using EFCore.Migrations.Triggers.PostgreSQL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Triggers;

public static class PostgreSqlTriggersExtensions
{
    public static EntityTypeBuilder<TEntity> BeforeInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.Before, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> AfterInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body, ConstraintTriggerType? constraintType = null)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.After, body, constraintType);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> InsteadInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.Instead, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> BeforeUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.Before, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> AfterUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body, ConstraintTriggerType? constraintType = null)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.After, body, constraintType);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> InsteadUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.Instead, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> BeforeDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.Before, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> AfterDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body, ConstraintTriggerType? constraintType = null)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.After, body, constraintType);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> InsteadDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.Instead, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> BeforeInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.Before, body);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> AfterInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body, ConstraintTriggerType? constraintType = null)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.After, body, constraintType);

        return entityTypeBuilder;
    }

    public static EntityTypeBuilder<TEntity> InsteadInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, string body)
        where TEntity : class
    {
        entityTypeBuilder.AddPostgreSqlTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.Instead, body);

        return entityTypeBuilder;
    }

    private static void AddPostgreSqlTrigger<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name, TriggerOperationEnum operation, TriggerTimeEnum time, string body,
        ConstraintTriggerType? constraintType = null)
        where TEntity : class
    {
        var table = entityTypeBuilder.Metadata.GetTableName();

        var trigger = new PostgreSqlTriggerObject
        {
            Name = name,
            Table = table,
            Operation = operation,
            Time = time,
            Body = body,
            ConstraintType = constraintType,
        };

        entityTypeBuilder.AddTriggerAnnotation(trigger);
    }
}