using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EFCore.Migrations.AutoComments.Conventions;
using EFCore.Migrations.Toolkit;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.AutoComments
{
    /// <summary>
    /// Расширение, которое позволяет передать и сохранить пути до XML файлов с комментариями.
    /// Также регистрирует плагин установки конвенций.
    /// </summary>
    internal class AutoCommentsOptionsExtension : IDbContextOptionsExtension
    {
        public string[] XmlFiles { get; }

        public bool AutoCommentEnumDescriptions { get; set; }

        public AutoCommentsOptionsExtension(AutoCommentOptions autoCommentOptions)
        {
            XmlFiles = autoCommentOptions.XmlFiles;
            AutoCommentEnumDescriptions = autoCommentOptions.AutoCommentEnumDescriptions;

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
            foreach (var xmlPath in XmlFiles)
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

        public override long GetServiceProviderHashCode() => CalculateHashCode();

        private int CalculateHashCode()
        {
            var hash = new HashCode();
            foreach (var item in Extension.XmlFiles)
            {
                hash.Add(item);
            }

            hash.Add(Extension.AutoCommentEnumDescriptions);

            return hash.ToHashCode();
        }

        private new AutoCommentsOptionsExtension Extension => (AutoCommentsOptionsExtension)base.Extension;

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    foreach (var xmlPath in Extension.XmlFiles)
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
}