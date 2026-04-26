using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using EFCore.Migrations.AutoComments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class DependencyInjection
{
    public static TBuilder UseAutoComments<TBuilder>([NotNull] this TBuilder optionsBuilder, params string[] xmlFiles)
        where TBuilder : DbContextOptionsBuilder
    {
        return optionsBuilder.UseAutoComments(o => o.FromXmlFiles(xmlFiles));
    }

    public static TBuilder UseAutoComments<TBuilder>([NotNull] this TBuilder optionsBuilder, Action<AutoCommentOptions> configure)
        where TBuilder : DbContextOptionsBuilder
    {
        var options = new AutoCommentOptions();
        configure.Invoke(options);

        var extension = new AutoCommentsOptionsExtension(options);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}

public class AutoCommentOptions
{
    internal string[] XmlFiles { get; set; } = Array.Empty<string>();

    internal bool AutoCommentEnumDescriptions { get; set; }
}

public static class AutoCommentOptionsExtensions
{
    public static AutoCommentOptions FromXmlFiles(this AutoCommentOptions options, params string[] xmlFiles)
    {
        options.XmlFiles = GetXmlFiles(xmlFiles).ToArray();

        return options;
    }

    public static AutoCommentOptions AddEnumDescriptions(this AutoCommentOptions options)
    {
        options.AutoCommentEnumDescriptions = true;

        return options;
    }

    private static IEnumerable<string> GetXmlFiles(string[] xmlFiles)
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        foreach (var xmlFile in xmlFiles ?? Array.Empty<string>())
        {
            var found = false;
            var paths = new List<string>
            {
                xmlFile,
                Path.Combine(assemblyLocation, xmlFile)
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    yield return path;
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                var searchedPaths = string.Join(Environment.NewLine, paths.Select(p => $"- {p}"));

                throw new InvalidOperationException(
                    $"XML file not found. Searched locations:{Environment.NewLine}{searchedPaths}");
            }
        }
    }
}