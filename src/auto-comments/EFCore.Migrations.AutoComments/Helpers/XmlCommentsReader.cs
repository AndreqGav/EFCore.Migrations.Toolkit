using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace EFCore.Migrations.AutoComments.Helpers
{
    public class XmlCommentsReader
    {
        private const string NewLinePlaceholder = "\n";

        private readonly XmlDocument _xmlDocument = new XmlDocument();

        private readonly bool _autoLoadAssemblies;

        private readonly HashSet<Assembly> _loadedAssemblies = new();

        private readonly object _syncRoot = new();

        public static XmlCommentsReader Create(IReadOnlyCollection<string> xmlFiles)
        {
            return xmlFiles.Count > 0 ? new XmlCommentsReader(xmlFiles) : new XmlCommentsReader();
        }

        private XmlCommentsReader(IReadOnlyCollection<string> xmlFiles)
        {
            _autoLoadAssemblies = false;
            _xmlDocument.LoadXml("<root></root>");

            LoadXmlFiles(xmlFiles);
        }

        private XmlCommentsReader()
        {
            _autoLoadAssemblies = true;
            _xmlDocument.LoadXml("<root></root>");
        }

        public string GetTypeComment(Type type)
        {
            EnsureAssemblyLoaded(type);

            foreach (var t in TypeHelper.GetParentTypes(type))
            {
                var node = _xmlDocument.SelectSingleNode($"//member[@name='T:{GetFullName(t)}']/summary");

                if (node != null)
                    return NormalizeText(node.InnerText);
            }

            return null;
        }

        public string GetPropertyComment(Type declaringType, string propertyName)
        {
            EnsureAssemblyLoaded(declaringType);

            foreach (var t in TypeHelper.GetParentTypes(declaringType))
            {
                var node = _xmlDocument.SelectSingleNode(
                    $"//member[@name='P:{GetFullName(t)}.{propertyName}']/summary");

                if (node != null)
                    return NormalizeText(node.InnerText);
            }

            return null;
        }

        public string GetEnumFieldComment(Type enumType, string fieldName)
        {
            EnsureAssemblyLoaded(enumType);

            var node = _xmlDocument.SelectSingleNode(
                $"//member[@name='F:{GetFullName(enumType)}.{fieldName}']/summary");

            return node != null ? NormalizeText(node.InnerText) : null;
        }

        private static string GetFullName(Type type) => type?.FullName?.Replace("+", ".") ?? string.Empty;

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return string.Join(NewLinePlaceholder, text.Trim()
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim()));
        }

        private void LoadXmlFiles(IEnumerable<string> xmlFiles)
        {
            foreach (var xmlFile in xmlFiles)
            {
                var fileInfo = new FileInfo(xmlFile);

                if (fileInfo.Length == 0) continue;

                var xmlDocumentPart = new XmlDocument();
                using var commentStream = File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                xmlDocumentPart.Load(commentStream);

                if (xmlDocumentPart.DocumentElement != null)
                {
                    var importNode = _xmlDocument.ImportNode(xmlDocumentPart.DocumentElement, true);
                    _xmlDocument.DocumentElement?.AppendChild(importNode);
                }
            }
        }

        private void EnsureAssemblyLoaded(Type type)
        {
            if (!_autoLoadAssemblies || type == null) return;

            var assembly = type.Assembly;

            lock (_syncRoot)
            {
                if (_loadedAssemblies.Contains(assembly) || assembly.IsDynamic) return;

                _loadedAssemblies.Add(assembly);

                var xmlPath = GetXmlDocsPath(assembly);

                if (xmlPath != null)
                {
                    LoadXmlFiles(new[]
                    {
                        xmlPath
                    });
                }
            }
        }

        private static string GetXmlDocsPath(Assembly assembly)
        {
            var assemblyPath = assembly.Location;

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");

                if (File.Exists(xmlPath)) return xmlPath;
            }

            var baseDirectory = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                var nameSource = string.IsNullOrEmpty(assemblyPath)
                    ? assembly.ManifestModule.Name
                    : assemblyPath;

                var assemblyName = Path.GetFileNameWithoutExtension(nameSource);
                var xmlPath = Path.Combine(baseDirectory, assemblyName + ".xml");

                if (File.Exists(xmlPath)) return xmlPath;
            }

            return null;
        }
    }
}