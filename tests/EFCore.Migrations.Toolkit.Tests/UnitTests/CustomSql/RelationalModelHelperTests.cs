using System.Linq;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.CustomSql.Helpers;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.CustomSql
{
    /// <summary>
    /// Тесты для <see cref="RelationalModelHelper"/>.
    /// </summary>
    public class RelationalModelHelperTests
    {
        // SQL для тестового представления сводки заказов
        internal const string SqlName = "orders_summary";

        internal const string SqlUp =
            "CREATE VIEW orders_summary AS SELECT id, number, total_amount FROM \"Orders\";";

        internal const string SqlDown = "DROP VIEW IF EXISTS orders_summary;";

        private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite("Data Source=unit_tests.db").UseCustomSql();

            return builder.Options;
        }

        [Fact]
        public void GetCustomAnnotations_Should_ReturnSingleRegisteredAnnotation()
        {
            // Arrange
            using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

            // Act
            var annotations = RelationalModelHelper
                .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context));

            // Assert
            Assert.Single(annotations);
            Assert.Contains(annotations, a => a.Name == SqlName);
        }

        [Fact]
        public void GetCustomAnnotations_Should_ReturnAnnotation_WithCorrectSql()
        {
            // Arrange
            using var context = new CustomSqlContext(BuildOptions<CustomSqlContext>());

            // Act
            var annotation = RelationalModelHelper
                .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context))
                .Single(a => a.Name == SqlName);

            // Assert
            Assert.Equal(SqlUp, annotation.SqlUp);
            Assert.Equal(SqlDown, annotation.SqlDown);
        }

        [Fact]
        public void GetCustomAnnotations_Should_ReturnEmpty_WhenModelIsNull()
        {
            // Arrange & Act
            var result = RelationalModelHelper.GetCustomSqlAnnotations(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCustomAnnotations_Should_ReturnEmpty_WhenNoAnnotationsRegistered()
        {
            // Arrange
            using var context = new EmptyCustomSqlContext(BuildOptions<EmptyCustomSqlContext>());

            // Act
            var annotations = RelationalModelHelper
                .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context));

            // Assert
            Assert.Empty(annotations);
        }

        internal sealed class EmptyCustomSqlContext : DbContext
        {
            public DbSet<Order> Orders { get; set; }

            public EmptyCustomSqlContext(DbContextOptions<EmptyCustomSqlContext> options) : base(options)
            {
            }
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
                    CustomSqlAnnotationTests.SqlName,
                    CustomSqlAnnotationTests.SqlUp,
                    CustomSqlAnnotationTests.SqlDown);
            }
        }
    }
}