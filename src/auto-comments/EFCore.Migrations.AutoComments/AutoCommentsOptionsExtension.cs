using System.Collections.Generic;
using System.IO;
using System.Text;
using EFCore.Migrations.AutoComments.Conventions;
using EFCore.Migrations.Toolkit;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.AutoComments;

/// <summary>
/// Расширение, которое позволяет передать и сохранить пути до XML файлов с комментариями.
/// Также регистрирует плагин установки конвенций.
/// </summary>
internal class AutoCommentsExtension : IDbContextOptionsExtension
{
    public AutoCommentOptions Options { get; }

    public AutoCommentsExtension(AutoCommentOptions options)
    {
        Options = options;
        Info = new AutoCommentsExtensionInfo(this);
    }

    public DbContextOptionsExtensionInfo Info { get; }

    public void ApplyServices(IServiceCollection services)
    {
        new EntityFrameworkServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ConventionSetPlugin>();
    }

    public void Validate(IDbContextOptions options)
    {
        foreach (var xmlPath in Options.XmlFiles)
        {
            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException($"XML file {xmlPath} not exists", xmlPath);
            }
        }
    }
}

public class AutoCommentsExtensionInfo : DbContextOptionsExtensionInfo
{
    private string _logFragment;

    public AutoCommentsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    {
    }

    public override bool IsDatabaseProvider => false;

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is AutoCommentsExtensionInfo;

    public override int GetServiceProviderHashCode() => Extension.Options.GetHashCode();

    private new AutoCommentsExtension Extension => (AutoCommentsExtension)base.Extension;

    public override string LogFragment
    {
        get
        {
            if (_logFragment == null)
            {
                var builder = new StringBuilder();

                foreach (var xmlPath in Extension.Options.XmlFiles)
                {
                    builder.AppendJoin(' ', $"Used XML comments file: {xmlPath}");
                }

                _logFragment = builder.ToString();
            }

            return _logFragment;
        }
    }

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
        debugInfo["AutoCommentsOptionsExtension"] = "1";
    }
}