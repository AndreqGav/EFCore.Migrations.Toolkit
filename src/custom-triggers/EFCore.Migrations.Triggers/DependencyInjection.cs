using System.Diagnostics.CodeAnalysis;
using EFCore.Migrations.Triggers.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class TriggersDependencyInjection
{
    public static TBuilder UseTriggers<TBuilder, TExtension>([NotNull] this TBuilder optionsBuilder)
        where TExtension : TriggerSqlExtension, new()
        where TBuilder : DbContextOptionsBuilder
    {
        optionsBuilder.UseCustomSql();

        var extension = new TExtension();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}