using System.Linq;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.CustomSql.Constants;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.CustomSql
{
    /// <summary>
    /// Тесты проверяют хранение произвольного SQL в аннотациях модели
    /// </summary>
    public class CustomSqlAnnotationTests
    {
        // SQL для тестового представления сводки заказов
        internal const string SqlName = "orders_summary";

        internal const string SqlUp =
            "CREATE VIEW orders_summary AS SELECT id, number, total_amount FROM \"Orders\";";

        internal const string SqlDown = "DROP VIEW IF EXISTS orders_summary;";

        // Изменённая версия того же SQL для проверки сценариев смены скрипта
        internal const string ChangedSqlUp =
            "CREATE VIEW orders_summary AS SELECT id, number, total_amount, 'v2' AS version FROM \"Orders\";";

        private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite("Data Source=unit_tests.db").UseCustomSql();

            return builder.Options;
        }

        [Fact]
        public void AddCustomSql_Should_StoreSqlUpAnnotation_WithCorrectScript()
        {
            // Arrange
            using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

            // Act
            var annotation = ModelAccessor.GetModel(context).GetAnnotations()
                .SingleOrDefault(a => a.Name == $"{CustomSqlConstants.SqlUp}{SqlName}");

            // Assert
            Assert.NotNull(annotation);
            Assert.Equal(SqlUp, annotation.Value?.ToString());
        }

        [Fact]
        public void AddCustomSql_Should_StoreSqlDownAnnotation_WithCorrectScript()
        {
            // Arrange
            using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

            // Act
            var annotation = ModelAccessor.GetModel(context).GetAnnotations()
                .SingleOrDefault(a => a.Name == $"{CustomSqlConstants.SqlDown}{SqlName}");

            // Assert
            Assert.NotNull(annotation);
            Assert.Equal(SqlDown, annotation.Value?.ToString());
        }

        internal sealed class CustomSqlContext : DbContext
        {
            public DbSet<Order> Orders { get; set; }

            public CustomSqlContext(DbContextOptions<CustomSqlContext> options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.AddCustomSql(
                    SqlName,
                    SqlUp,
                    SqlDown);
            }
        }
    }
}