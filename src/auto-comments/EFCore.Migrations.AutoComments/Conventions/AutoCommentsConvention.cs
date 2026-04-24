using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCore.Migrations.AutoComments.Extensions;
using EFCore.Migrations.AutoComments.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Migrations.AutoComments.Conventions
{
    internal class AutoCommentsConvention : IModelFinalizingConvention
    {
        private readonly XmlCommentsReader _xmlReader;

        public AutoCommentsConvention(IReadOnlyCollection<string> xmlFiles)
        {
            _xmlReader = XmlCommentsReader.Create(xmlFiles);
        }

        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            SetTableComments(modelBuilder);

            SetColumnComments(modelBuilder);
        }

        private void SetTableComments(IConventionModelBuilder modelBuilder)
        {
            var entityTypes = GetEntityTypes(modelBuilder);

            var entityTypesByTable = entityTypes
                .GroupBy(e => (e.GetTableName(), e.GetSchema()))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var entities in entityTypesByTable.Values)
            {
                if (entities.Count == 1)
                {
                    var entityType = entities.Single();

                    if (entityType.GetComment() != null)
                        continue;

                    var comment = _xmlReader.GetTypeComment(entityType.ClrType);
                    if (comment != null)
                        entityType.SetComment(comment);
                }
                else
                {
                    // Если хотя бы одна имеет ручной комментарий — не трогаем группу.
                    if (entities.Any(e => e.GetComment() != null))
                        continue;

                    // Уникальные XML-комментарии объединяем через \n.
                    var comments = entities
                        .Select(e => _xmlReader.GetTypeComment(e.ClrType))
                        .Where(c => c != null)
                        .Distinct()
                        .ToList();

                    if (comments.Count == 0)
                        continue;

                    var mergedComment = string.Join("\n", comments);
                    foreach (var entityType in entities)
                        entityType.SetComment(mergedComment);
                }
            }
        }

        private void SetColumnComments(IConventionModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.GetComment() != null)
                        continue;

                    var comment = _xmlReader.GetPropertyComment(property.PropertyInfo?.DeclaringType, property.Name);

                    if (comment != null)
                        property.SetComment(comment);

                    if (property.HasEnumDescriptionComment())
                        AppendEnumDescriptionComment(property);
                }
            }
        }

        private IEnumerable<IConventionEntityType> GetEntityTypes(IConventionModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                // Для owned типов (предполагается что у owned нет таблицы)
                if (entityType.IsOwned())
                {
                    continue;
                }

                // Для вьюх.
                if (entityType.GetViewName() != null)
                {
                    continue;
                }

                // Для абстрактных классов в наследовании TPC.
                if (entityType.GetTableName() == null)
                {
                    continue;
                }

                // Для наследников в TPH (они разделяют таблицу с базовым типом).
                // TPT-наследники имеют собственную таблицу — их не пропускаем.
                if (entityType.BaseType != null && entityType.BaseType.GetTableName() == entityType.GetTableName())
                {
                    continue;
                }

                yield return entityType;
            }
        }

        private void AppendEnumDescriptionComment(IConventionProperty property)
        {
            var propType = property.PropertyInfo?.PropertyType;
            var enumType = propType != null ? Nullable.GetUnderlyingType(propType) ?? propType : null;

            if (enumType == null || !enumType.IsEnum)
                return;

            var enumIsString = false;
#pragma warning disable EF1001
            var annotation = property.FindAnnotation(CoreAnnotationNames.ProviderClrType);
#pragma warning restore EF1001
            if (annotation?.Value as Type == typeof(string))
                enumIsString = true;

            var sb = new StringBuilder(property.GetComment());
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

            property.SetComment(sb.ToString());
        }
    }
}