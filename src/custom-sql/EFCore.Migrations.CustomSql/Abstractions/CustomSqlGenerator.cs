using System;
using System.Linq.Expressions;
using EFCore.Migrations.CustomSql.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql.Abstractions;

public abstract class CustomSqlGenerator
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private readonly ModelBuilder _modelBuilder;

    private readonly bool _isRelational;

    protected CustomSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;

        _isRelational = dbContext.Database.IsRelational();

        if (_isRelational)
        {
            _sqlGenerationHelper = dbContext.GetInfrastructure().GetRequiredService<ISqlGenerationHelper>();
        }
    }

    protected string GetTableName<TEntity>(bool includeSchema = true)
        where TEntity : class
    {
        if (!_isRelational || _sqlGenerationHelper == null) return string.Empty;

        var metadata = _modelBuilder.Entity<TEntity>().Metadata;

        var tableName = metadata.GetTableName();

        tableName = tableName is not null ? _sqlGenerationHelper.DelimitIdentifier(tableName) : null;

        if (includeSchema)
        {
            var schema = metadata.GetSchema();
            tableName = schema is not null ? $"{schema}.{tableName}" : tableName;
        }

        return tableName;
    }

    protected string GetColumnName<TEntity>(Expression<Func<TEntity, object>> propertyExpression)
        where TEntity : class
    {
        var propertyName = ExpressionHelper.GetMemberName(propertyExpression)!;

        return GetColumnName<TEntity>(propertyName);
    }

    protected string GetColumnName<TEntity>(string propertyName)
        where TEntity : class
    {
        if (!_isRelational || _sqlGenerationHelper == null) return string.Empty;

        var metadata = _modelBuilder.Entity<TEntity>().Metadata;

        var tableName = metadata.GetTableName()!;
        var schema = metadata.GetSchema()!;
        var columnName = metadata.FindProperty(propertyName)?.GetColumnName(StoreObjectIdentifier.Table(tableName, schema));

        return columnName is not null ? _sqlGenerationHelper.DelimitIdentifier(columnName) : null;
    }
}