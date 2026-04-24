using EFCore.Migrations.CustomSql.Models;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.CustomSql
{
    /// <summary>
    /// Тесты <see cref="SqlAnnotationModel"/>.
    /// </summary>
    public class SqlAnnotationModelTests
    {
        [Fact]
        public void SqlUpModel_Sql_Should_NoCrLf_WhenSqlHasCrLf()
        {
            // Arrange
            var model = new SqlUpModel("name", "line1;\r\nline2;");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Sql);
        }

        [Fact]
        public void SqlUpModel_Sql_Should_NormalizeCrLfToLf()
        {
            // Arrange
            var model = new SqlUpModel("name", "line1;\r\nline2;");

            // Act + Assert
            Assert.Contains("line1;\nline2;", model.Sql);
        }

        [Fact]
        public void SqlUpModel_Sql_Should_NoCrLf_WhenSqlHasLfOnly()
        {
            // Arrange
            var model = new SqlUpModel("name", "line1;\nline2;");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Sql);
        }

        [Fact]
        public void SqlUpModel_Sql_Should_NoCrLf_WhenSqlHasMixedLineEndings()
        {
            // Arrange
            var model = new SqlUpModel("name", "line1;\r\nline2;\nline3;\r\nline4;");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Sql);
        }

        [Fact]
        public void SqlDownModel_Sql_Should_NoCrLf_WhenSqlHasCrLf()
        {
            // Arrange
            var model = new SqlDownModel("name", "DROP VIEW IF EXISTS orders;\r\n");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Sql);
        }

        [Fact]
        public void SqlDownModel_Sql_Should_NormalizeCrLfToLf()
        {
            // Arrange
            var model = new SqlDownModel("name", "line1;\r\nline2;");

            // Act + Assert
            Assert.Contains("line1;\nline2;", model.Sql);
        }

        [Fact]
        public void SqlUpModel_Annotation_Should_NoCrLf_WhenNameHasCrLf()
        {
            // Arrange
            var model = new SqlUpModel("name\r\nwith\r\nnewlines", "SELECT 1;");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Annotation);
        }

        [Fact]
        public void SqlDownModel_Annotation_Should_NoCrLf_WhenNameHasCrLf()
        {
            // Arrange
            var model = new SqlDownModel("name\r\nwith\r\nnewlines", "SELECT 1;");

            // Act + Assert
            Assert.DoesNotContain("\r\n", model.Annotation);
        }
    }
}