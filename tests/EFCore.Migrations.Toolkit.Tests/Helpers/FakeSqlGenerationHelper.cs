using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Migrations.Toolkit.Tests.Helpers
{
    internal sealed class FakeSqlGenerationHelper : ISqlGenerationHelper
    {
        public string StatementTerminator => ";";
    
        public string BatchTerminator => string.Empty;
    
        public string StartTransactionStatement => "BEGIN";
    
        public string CommitTransactionStatement => "COMMIT";
    
        public string SingleLineCommentToken => "--";
    
        public string DelimitIdentifier(string identifier) => $"\"{identifier}\"";
    
        public string DelimitIdentifier(string name, string schema) =>
            string.IsNullOrEmpty(schema) ? DelimitIdentifier(name) : $"\"{schema}\".\"{name}\"";
    
        public void DelimitIdentifier(StringBuilder builder, string identifier) => builder.Append(DelimitIdentifier(identifier));
    
        public void DelimitIdentifier(StringBuilder builder, string name, string schema) =>
            builder.Append(DelimitIdentifier(name, schema));
    
        public string EscapeIdentifier(string identifier) => identifier.Replace("\"", "\"\"");
    
        public void EscapeIdentifier(StringBuilder builder, string identifier) => builder.Append(EscapeIdentifier(identifier));
    
        public string EscapeLiteral(string literal) => literal.Replace("'", "''");
    
        public void EscapeLiteral(StringBuilder builder, string literal) => builder.Append(EscapeLiteral(literal));
    
        public string GenerateComment(string text) => $"-- {text}";
    
        public string GenerateParameterName(string name) => $"@{name}";
    
        public void GenerateParameterName(StringBuilder builder, string name) => builder.Append($"@{name}");
    
        public string GenerateParameterNamePlaceholder(string name) => $"@{name}";
    
        public void GenerateParameterNamePlaceholder(StringBuilder builder, string name) => builder.Append($"@{name}");
    
        public string DelimitJsonPathElement(string element) => element;
    
        public string GenerateCreateSavepointStatement(string name) => $"SAVEPOINT {name}";
    
        public string GenerateRollbackToSavepointStatement(string name) => $"ROLLBACK TO SAVEPOINT {name}";
    
        public string GenerateReleaseSavepointStatement(string name) => $"RELEASE SAVEPOINT {name}";
    }
}