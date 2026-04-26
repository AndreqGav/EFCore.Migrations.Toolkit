using System.Collections.Generic;
using System.Linq;
using EFCore.Migrations.Abstractions;
using EFCore.Migrations.CustomSql.Helpers;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Migrations.CustomSql;

public class CustomSqlMigrationOperationModifier : IMigrationOperationModifier
{
    public IReadOnlyList<MigrationOperation> ModifyOperations(IReadOnlyList<MigrationOperation> operations,
        IRelationalModel source, IRelationalModel target)
    {
        var (create, delete) = CreateCustomSqlOperations(source, target);

        return delete.Concat(operations).Concat(create).ToList();
    }

    private (List<SqlOperation> create, List<SqlOperation> delete) CreateCustomSqlOperations(
        IRelationalModel source,
        IRelationalModel target)
    {
        var createOperations = new List<SqlOperation>();
        var deleteOperations = new List<SqlOperation>();

        var sourceAnnotations = RelationalModelHelper.GetCustomSqlAnnotations(source);
        var targetAnnotations = RelationalModelHelper.GetCustomSqlAnnotations(target);

        var deletedAnnotations = sourceAnnotations.Where(sa =>
            !targetAnnotations.Select(ta => ta.Name).Contains(sa.Name)
        );

        foreach (var annotation in deletedAnnotations)
        {
            AddToDelete(annotation.SqlDown);
        }

        foreach (var targetAnnotation in targetAnnotations)
        {
            var sourceAnnotation = sourceAnnotations.SingleOrDefault(sa => sa.Name == targetAnnotation.Name);
            var targetSql = targetAnnotation.SqlUp;
            var sourceSql = sourceAnnotation?.SqlUp;

            if (sourceAnnotation is not null)
            {
                if (targetSql != sourceSql)
                {
                    AddToDelete(sourceAnnotation.SqlDown);
                    AddToCreate(targetSql);
                }
            }
            else
            {
                AddToCreate(targetSql);
            }
        }

        return (createOperations, deleteOperations);

        void AddToCreate(string sql) => AddOperation(sql, createOperations);

        void AddToDelete(string sql) => AddOperation(sql, deleteOperations);

        void AddOperation(string sql, List<SqlOperation> operations)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                operations.Add(new SqlOperation
                {
                    Sql = sql,
                });
            }
        }
    }
}