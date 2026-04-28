using System;
using System.Diagnostics.CodeAnalysis;
using EFCore.Migrations.CustomSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class DependencyInjection
{
    public static TBuilder UseCustomSql<TBuilder>([NotNull] this TBuilder optionsBuilder)
        where TBuilder : DbContextOptionsBuilder
    {
        return optionsBuilder.UseCustomSql(_ => { });
    }

    public static TBuilder UseCustomSql<TBuilder>([NotNull] this TBuilder optionsBuilder, Action<CustomSqlOptionsBuilder> configure)
        where TBuilder : DbContextOptionsBuilder
    {
        configure.Invoke(new CustomSqlOptionsBuilder(optionsBuilder));

        var extension = optionsBuilder.Options.FindExtension<CustomSqlOptionsExtension>() ??
                        new CustomSqlOptionsExtension(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}

public interface ICustomSqlOptionsBuilder
{
    DbContextOptionsBuilder OptionsBuilder { get; }
}

public class CustomSqlOptionsBuilder : ICustomSqlOptionsBuilder
{
    public CustomSqlOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    DbContextOptionsBuilder ICustomSqlOptionsBuilder.OptionsBuilder => OptionsBuilder;
}