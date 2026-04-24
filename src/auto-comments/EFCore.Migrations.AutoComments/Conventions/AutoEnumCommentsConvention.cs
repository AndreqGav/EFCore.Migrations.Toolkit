using System;
using EFCore.Migrations.AutoComments.Attributes;
using EFCore.Migrations.AutoComments.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFCore.Migrations.AutoComments.Conventions
{
    /// <summary>
    /// Добавление аннотаций о том, что требуется дополнить комментарий к Enum с перечислением его значений.
    /// </summary>
    internal class AutoCommentEnumDescriptionConvention : IModelFinalizingConvention
    {
        private readonly bool _allEnumsHasAutoCommentDescription;

        public const string Name = "AutoCommentEnumDescription";

        public AutoCommentEnumDescriptionConvention(bool allEnumsHasAutoCommentDescription)
        {
            _allEnumsHasAutoCommentDescription = allEnumsHasAutoCommentDescription;
        }

        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    TrySetAutoCommentEnumDescriptionAnnotation(property);
                }
            }
        }

        private void TrySetAutoCommentEnumDescriptionAnnotation(IConventionProperty property)
        {
            var memberInfo = property.PropertyInfo;
            if (memberInfo == null)
            {
                return;
            }

            if (_allEnumsHasAutoCommentDescription)
            {
                var propType = property.PropertyInfo?.PropertyType;

                if (propType?.IsEnum == true)
                {
                    var ignoreAutoEnumComment = Attribute.GetCustomAttribute(memberInfo, typeof(IgnoreAutoCommentEnumDescriptionAttribute));

                    if (ignoreAutoEnumComment is null)
                    {
                        property.Builder.AddEnumDescriptionComment();
                    }
                }
            }
            else
            {
                var autoEnumComment = Attribute.GetCustomAttribute(memberInfo, typeof(AutoCommentEnumDescriptionAttribute));

                if (autoEnumComment is not null)
                {
                    property.Builder.AddEnumDescriptionComment();
                }
            }
        }
    }
}