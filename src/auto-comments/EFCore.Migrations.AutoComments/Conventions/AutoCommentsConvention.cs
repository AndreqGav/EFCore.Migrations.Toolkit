using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCore.Migrations.AutoComments.Extensions;
using EFCore.Migrations.AutoComments.Helpers;
using EFCore.Migrations.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Migrations.AutoComments.Conventions;

internal class AutoCommentsConvention : IModelFinalizingConvention
{
    private readonly XmlCommentsReader _xmlReader;

    private readonly bool _combineInheritanceComments;

    public AutoCommentsConvention(AutoCommentOptions options)
    {
        _xmlReader = XmlCommentsReader.Create(options.XmlFiles);
        _combineInheritanceComments = options.CombineInheritanceComments;
    }

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        SetEntityComments(modelBuilder);

        SetOwnedTypes(modelBuilder);

        SetColumnComments(modelBuilder);
    }

    private void SetEntityComments(IConventionModelBuilder modelBuilder)
    {
        var allEntityTypes = modelBuilder.Metadata.GetEntityTypes().ToList();

        var tableTypes = GetTableEntityTypes(allEntityTypes).ToList();

        foreach (var entityType in tableTypes)
        {
            if (HasConfiguredComment(entityType)) continue;

            var comment = _xmlReader.GetTypeComment(entityType.ClrType);

            SetTableComment(entityType, comment);

            MergeTphComments(entityType);

            MergeTableSplittingComments(entityType);
        }

        return;

        bool HasConfiguredComment(IConventionEntityType entityType)
        {
            return allEntityTypes
                .Where(x => HasSameTable(entityType, x))
                .Any(x => x.GetCommentConfigurationSource() is ConfigurationSource.Explicit or ConfigurationSource.DataAnnotation);
        }

        void SetTableComment(IConventionEntityType entityType, string comment)
        {
            if (comment == null) return;

            var root = entityType.Builder.Metadata.GetRootType();

            // Для TPH выставляем комментарий всей цепочке наследования.
            if (IsTph(root))
            {
                var derivedTypes = root.GetDerivedTypesInclusive().ToList();

                foreach (var derivedType in derivedTypes)
                {
                    derivedType.SetComment(comment);
                }
            }
            else
            {
                entityType.SetComment(comment);
            }
        }

        void MergeTphComments(IConventionEntityType entityType)
        {
            if (!IsTph(entityType.GetRootType())) return;

            var root = entityType.Builder.Metadata.GetRootType();

            var entityTypes = root.GetDerivedTypesInclusive().ToList();

            if (_combineInheritanceComments)
            {
                var comment = MergeComments(entityTypes.Select(e => _xmlReader.GetTypeComment(e.ClrType)));

                SetTableComment(entityType, comment);
            }
        }

        void MergeTableSplittingComments(IConventionEntityType entityType)
        {
            var entityTypes = tableTypes
                .Where(x => HasSameTable(entityType, x))
                .ToList();

            if (entityTypes.Count == 1) return;

            var comment = MergeComments(entityTypes.Select(e => _xmlReader.GetTypeComment(e.ClrType)));

            foreach (var type in entityTypes)
            {
                SetTableComment(type, comment);
            }
        }
    }

    private void SetOwnedTypes(IConventionModelBuilder modelBuilder)
    {
        var ownedEntityTypes = modelBuilder.Metadata.GetEntityTypes().Where(x => x.IsOwned()).ToList();

        foreach (var ownedEntityType in ownedEntityTypes)
        {
            if (HasConfiguredComment(ownedEntityType)) continue;

            var ownership = ownedEntityType.FindOwnership();
            var ownerType = ownership?.PrincipalEntityType;
            var navigationProperty = ownership?.PrincipalToDependent;

            if (ownerType is null || navigationProperty is null) continue;

            // Если Owned маппится на свою таблицу, то указываем ей комментарий от Owned
            if (!HasSameTable(ownedEntityType, ownerType))
            {
                var comment = _xmlReader.GetTypeComment(ownedEntityType.ClrType);

                SetOwnedComment(ownedEntityType, comment);
            }
        }

        return;

        void SetOwnedComment(IConventionEntityType entityType, string comment)
        {
            if (comment == null) return;

            entityType.SetComment(comment);
        }

        bool HasConfiguredComment(IConventionEntityType entityType)
        {
            return entityType.GetCommentConfigurationSource() is ConfigurationSource.Explicit or ConfigurationSource.DataAnnotation;
        }
    }

    private void SetColumnComments(IConventionModelBuilder modelBuilder)
    {
        var allEntityTypes = modelBuilder.Metadata.GetEntityTypes().ToList();

        foreach (var entityType in allEntityTypes)
        {
            foreach (var property in entityType.GetProperties())
            {
                HandleProperty(property);
            }
        }

        return;

        void HandleProperty(IConventionProperty property)
        {
            if (HasConfiguredComment(property)) return;

            var comment = GetColumnComment(property);

            SetColumnComment(property, comment);

            MergeTphComments(property);

            MergeTableSplittingComments(property);
        }

        bool HasConfiguredComment(IConventionProperty property)
        {
            var columnName = GetColumnName(property);
            var entityType = property.DeclaringType;

            return allEntityTypes
                .Where(x => HasSameTable(entityType, x))
                .SelectMany(GetFlattenedProperties)
                .Where(x => GetColumnName(x) == columnName)
                .Any(x => x.GetCommentConfigurationSource() is ConfigurationSource.Explicit or ConfigurationSource.DataAnnotation);
        }

        string GetColumnComment(IConventionProperty property)
        {
            var propertyComment = _xmlReader.GetPropertyComment(property.PropertyInfo?.DeclaringType, property.Name);

            return property.HasEnumDescriptionComment()
                ? GetEnumDescriptionComment(property, propertyComment)
                : propertyComment;
        }

        void SetColumnComment(IConventionProperty entityProperty, string comment)
        {
            if (comment == null) return;

            var root = GetRootEntityType(entityProperty.DeclaringType);

            // Для TPH выставляем комментарий поля всей цепочке наследования.
            if (IsTph(root))
            {
                var entityTypes = root.GetDerivedTypesInclusive().ToList();

                var columnName = GetColumnName(entityProperty);

                var properties = entityTypes.SelectMany(GetFlattenedProperties)
                    .Where(x => GetColumnName(x) == columnName)
                    .ToList();

                foreach (var property in properties)
                {
                    property.SetComment(comment);
                }
            }
            else
            {
                entityProperty.SetComment(comment);
            }
        }

        void MergeTphComments(IConventionProperty entityProperty)
        {
            if (!IsTph(GetRootEntityType(entityProperty.DeclaringType))) return;

            var root = GetRootEntityType(entityProperty.DeclaringType);

            var entityTypes = root.GetDerivedTypesInclusive().ToList();

            var columnName = GetColumnName(entityProperty);

            var properties = entityTypes.SelectMany(GetFlattenedProperties)
                .Where(x => GetColumnName(x) == columnName)
                .ToList();

            var comment = MergeComments(properties.Select(GetColumnComment));

            foreach (var property in properties)
            {
                SetColumnComment(property, comment);
            }
        }

        void MergeTableSplittingComments(IConventionProperty entityProperty)
        {
            var columnName = GetColumnName(entityProperty);
            var entityType = entityProperty.DeclaringType;

            var entityTypes = allEntityTypes
                .Where(x => HasSameTable(entityType, x))
                .ToList();

            if (entityTypes.Count == 1) return;

            var properties = entityTypes.SelectMany(GetFlattenedProperties)
                .Where(x => GetColumnName(x) == columnName)
                .ToList();

            var comment = MergeComments(properties.Select(GetColumnComment));

            foreach (var property in properties)
            {
                SetColumnComment(property, comment);
            }
        }
    }

    private string MergeComments(IEnumerable<string> comments)
    {
        var comment = string.Join("\n", comments.Where(x => !string.IsNullOrEmpty(x)).Distinct()).Trim();

        if (string.IsNullOrEmpty(comment)) return null;

        return comment;
    }

    private static IEnumerable<IConventionProperty> GetFlattenedProperties(IConventionTypeBase entityType)
    {
        if (entityType is IConventionEntityType entity)
        {
            foreach (var property in entity.GetProperties())
            {
                yield return property;
            }
        }
    }

    private static bool HasSameTable(IConventionTypeBase entityTypeA, IConventionTypeBase entityTypeB)
    {
        if (entityTypeA is IConventionEntityType entityA && entityTypeB is IConventionEntityType entityB)
        {
            return entityA.GetTableName() == entityB.GetTableName() &&
                   entityA.GetSchema() == entityB.GetSchema();
        }

        return false;
    }

    private IEnumerable<IConventionEntityType> GetTableEntityTypes(IEnumerable<IConventionEntityType> entityTypes)
    {
        foreach (var entityType in entityTypes)
        {
            // Пропускаем Owned типы (они обрабатываются отдельно)
            if (entityType.IsOwned())
            {
                continue;
            }

            // Пропускаем представления.
            if (entityType.GetViewName() != null)
            {
                continue;
            }

            // Пропускаем сущности без таблицы
            if (entityType.GetTableName() == null)
            {
                continue;
            }

            // Пропускаем дочерние классы в наследовании TPH 
            if (IsTph(entityType.GetRootType()) && entityType.BaseType != null)
            {
                continue;
            }

            yield return entityType;
        }
    }

    private string GetEnumDescriptionComment(IConventionProperty property, string baseComment)
    {
        var propType = property.PropertyInfo?.PropertyType;
        var enumType = propType != null ? Nullable.GetUnderlyingType(propType) ?? propType : null;

        if (enumType == null || !enumType.IsEnum)
            return null;

        var enumIsString = false;
#pragma warning disable EF1001
        var annotation = property.FindAnnotation(CoreAnnotationNames.ProviderClrType);
#pragma warning restore EF1001
        if (annotation?.Value as Type == typeof(string))
            enumIsString = true;

        var sb = new StringBuilder(baseComment);
        sb.Append('\n');

        foreach (var value in Enum.GetValues(enumType))
        {
            var name = value.ToString();
            var fieldComment = _xmlReader.GetEnumFieldComment(enumType, name);

            if (fieldComment == null)
                continue;

            sb.Append('\n');
            sb.Append(enumIsString ? name : ((int)value).ToString());
            sb.Append(" - ");
            sb.Append(fieldComment);
        }

        var comment = sb.ToString().Trim();

        return string.IsNullOrEmpty(comment) ? null : comment;
    }

    IConventionEntityType GetRootEntityType(IConventionTypeBase typeBase)
    {
        if (typeBase is IConventionEntityType entityType)
        {
            return entityType.GetRootType();
        }

        return null;
    }

    private static bool IsTph(IConventionEntityType root)
    {
        var derivedTypes = root.GetDerivedTypesInclusive().ToList();

        return derivedTypes.Any() && derivedTypes.All(d => d.GetTableName() == root.GetTableName());
    }
    
    private static string GetColumnName(IConventionProperty property)
    {
        var entityType = property.DeclaringEntityType;

        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        
        if (tableName is null) return null;
        
        var columnName = entityType.FindProperty(property.Name)?.GetColumnName(StoreObjectIdentifier.Table(tableName, schema));

        return columnName;
    }
}