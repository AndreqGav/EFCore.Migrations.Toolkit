using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Migrations.Toolkit.Tests.Helpers
{
    /// <summary>
    /// Унифицированный доступ к модели EF Core для тестов, с учётом отличий между версиями фреймворка.
    /// </summary>
    internal static class ModelAccessor
    {
        public static IModel GetModel(DbContext context)
        {
            return context.Model;
        }

        public static IRelationalModel GetRelationalModel(DbContext context) => GetModel(context).GetRelationalModel();
    }
}