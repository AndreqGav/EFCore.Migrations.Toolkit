using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
#if NET6_0_OR_GREATER
            return context.GetService<IDesignTimeModel>().Model;
#else
            return context.Model;
#endif
        }

        public static IRelationalModel GetRelationalModel(DbContext context) => GetModel(context).GetRelationalModel();
    }
}