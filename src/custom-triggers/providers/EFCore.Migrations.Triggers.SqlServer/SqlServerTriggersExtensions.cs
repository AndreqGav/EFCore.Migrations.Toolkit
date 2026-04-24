using EFCore.Migrations.Triggers.SqlServer.Enums;
using EFCore.Migrations.Triggers.SqlServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Triggers
{
    public static class SqlServerTriggersExtensions
    {
        public static EntityTypeBuilder<TEntity> AfterInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfInsert<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Insert, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Update, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.Delete, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfInsertOrUpdate<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdate, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterInsertOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrDelete, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfInsertOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrDelete, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.UpdateOrDelete, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.UpdateOrDelete, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> AfterInsertOrUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdateOrDelete, TriggerTimeEnum.After, body);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> InsteadOfInsertOrUpdateOrDelete<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, string body)
            where TEntity : class
        {
            entityTypeBuilder.AddSqlServerTrigger(name, TriggerOperationEnum.InsertOrUpdateOrDelete, TriggerTimeEnum.InsteadOf, body);

            return entityTypeBuilder;
        }

        private static void AddSqlServerTrigger<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name, TriggerOperationEnum operation, TriggerTimeEnum time, string body)
            where TEntity : class
        {
            var table = entityTypeBuilder.Metadata.GetTableName();

            var trigger = new SqlServerTriggerObject
            {
                Name = name,
                Table = table,
                Operation = operation,
                Time = time,
                Body = body,
            };

            entityTypeBuilder.AddTriggerAnnotation(trigger);

#if NET7_0_OR_GREATER
            entityTypeBuilder.Metadata.AddTrigger(name);
#endif
        }
    }
}