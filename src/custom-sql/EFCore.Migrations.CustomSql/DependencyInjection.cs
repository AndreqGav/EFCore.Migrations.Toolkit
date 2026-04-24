using System.Diagnostics.CodeAnalysis;
using EFCore.Migrations.CustomSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit
{
    public static class DependencyInjection
    {
        public static TBuilder UseCustomSql<TBuilder>([NotNull] this TBuilder optionsBuilder)
            where TBuilder : DbContextOptionsBuilder
        {
            var extension = optionsBuilder.Options.FindExtension<CustomSqlOptionsExtension>() ??
                            new CustomSqlOptionsExtension(optionsBuilder);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}