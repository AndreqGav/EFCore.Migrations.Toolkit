using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Inheritance;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments
{
    /// <summary>
    /// Тесты проверяют, что конвенция автокомментариев корректно обрабатывает стратегии наследования TPH, TPC, TPT
    /// </summary>
    public class InheritanceStrategyConventionTests
    {
        private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>()
                .UseSqlite("Data Source=unit_tests.db")
                .UseAutoComments();

            return builder.Options;
        }

        private static string GetTableComment<TEntity>(DbContext context)
            => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

        [Fact]
        public void AutoComments_Tph_Should_SetComment_OnBaseType()
        {
            // Arrange
            using var context = new TphAutoCommentsContext(BuildOptions<TphAutoCommentsContext>());

            // Act
            var comment = GetTableComment<PostBase>(context);

            // Assert
            Assert.Equal("Базовый тип в наследовании TPH.", comment);
        }

        [Fact]
        public void AutoComments_Tph_ShouldNot_SetComment_OnDerivedTypes()
        {
            // Arrange
            using var context = new TphAutoCommentsContext(BuildOptions<TphAutoCommentsContext>());

            // Act + Assert
            Assert.Null(GetTableComment<PostA>(context));
            Assert.Null(GetTableComment<PostB>(context));
        }

    }

    internal sealed class TphAutoCommentsContext : DbContext
    {
        public DbSet<PostBase> Posts { get; set; }

        public DbSet<PostA> PostAs { get; set; }

        public DbSet<PostB> PostBs { get; set; }

        public TphAutoCommentsContext(DbContextOptions<TphAutoCommentsContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostBase>(builder =>
            {
                builder.HasKey(e => e.Id);
            });

            modelBuilder.Entity<PostA>(b => b.HasBaseType<PostBase>());
            modelBuilder.Entity<PostB>(b => b.HasBaseType<PostBase>());
        }
    }
}