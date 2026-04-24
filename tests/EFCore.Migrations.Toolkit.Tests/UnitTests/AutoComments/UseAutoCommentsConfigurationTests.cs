using System;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments
{
    /// <summary>
    /// Тесты проверяют конфигурацию UseAutoComments.
    /// </summary>
    public class UseAutoCommentsConfigurationTests
    {
        private static string GetTableComment<TEntity>(DbContext context)
            => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

        [Fact]
        public void UseAutoComments_Should_SetComments_When_NoXmlFiles()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ConfigurationTestContext>()
                .UseSqlite("Data Source=unit_tests.db")
                .UseAutoComments()
                .Options;

            // Act
            using var context = new ConfigurationTestContext(options);
            var comment = GetTableComment<Order>(context);

            // Assert
            Assert.Equal("Заказ покупателя.", comment);
        }

        [Fact]
        public void UseAutoComments_Should_SetComments_When_ValidXmlFile()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ConfigurationTestContext>()
                .UseSqlite("Data Source=unit_tests.db")
                .UseAutoComments("EFCore.Migrations.Toolkit.Tests.xml")
                .Options;

            // Act
            using var context = new ConfigurationTestContext(options);
            var comment = GetTableComment<Order>(context);

            // Assert
            Assert.Equal("Заказ покупателя.", comment);
        }

        [Fact]
        public void UseAutoComments_Should_Throw_When_MissingXmlFile()
        {
            // Arrange
            Action action = () =>
            {
                new DbContextOptionsBuilder<ConfigurationTestContext>()
                    .UseSqlite("Data Source=unit_tests.db")
                    .UseAutoComments("DoesNotExist.xml");
            };

            // Act + Assert
            Assert.Throws<InvalidOperationException>(action);
        }
    }

    internal sealed class ConfigurationTestContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public ConfigurationTestContext(DbContextOptions options) : base(options)
        {
        }
    }
}