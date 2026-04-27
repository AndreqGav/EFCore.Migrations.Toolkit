using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.TableSplitting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
#pragma warning disable CS0618 // Type or member is obsolete

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют, что конвенция автокомментариев корректно обрабатывает  разбиение таблицы: две сущности используют одну таблицу.
/// </summary>
public class TableSplittingConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments();

        return builder.Options;
    }

    private static string GetEntityComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    private static IProperty GetProperty<TEntity>(DbContext context, string propertyName)
        => ModelAccessor.GetModel(context)
            .FindEntityType(typeof(TEntity))!
            .FindProperty(propertyName)!;

    [Fact]
    public void AutoComments_TableSplitting_SameComment_Should_SetComment()
    {
        // Arrange
        using var context = new SameCommentSplitContext(BuildOptions<SameCommentSplitContext>());

        // Act
        var primaryComment = GetEntityComment<Contract>(context);
        var secondaryComment = GetEntityComment<ContractDetails>(context);

        // Assert
        Assert.Equal("Договор.", primaryComment);
        Assert.Equal("Договор.", secondaryComment);
    }

    [Fact]
    public void AutoComments_TableSplitting_DifferentComments_Should_SetMergedComment()
    {
        // Arrange
        using var context = new DifferentCommentSplitContext(BuildOptions<DifferentCommentSplitContext>());

        // Act
        var primaryComment = GetEntityComment<Product>(context);
        var secondaryComment = GetEntityComment<ProductDetails>(context);

        // Assert
        Assert.Equal("Продукт.\nДетали продукта.", primaryComment);
        Assert.Equal("Продукт.\nДетали продукта.", secondaryComment);
    }

    [Fact]
    public void AutoComments_TableSplitting_Should_SkipGroup_WhenAnyEntityHasManualComment()
    {
        // Arrange.
        using var context = new ManualCommentSplitContext(BuildOptions<ManualCommentSplitContext>());

        // Act
        var primaryComment = GetEntityComment<Product>(context);
        var secondaryComment = GetEntityComment<ProductDetails>(context);

        // Assert
        Assert.Equal("Ручной комментарий.", primaryComment);
        Assert.Null(secondaryComment);
    }

    [Fact]
    public void AutoComments_TableSplitting_SameColumn_SameComment_Should_SetCommentOnBothEntityProperties()
    {
        // Arrange
        using var context = new SplitSameColContext(BuildOptions<SplitSameColContext>());

        // Act
        var mainProp = GetProperty<SensorMain>(context, nameof(SensorMain.Data));
        var detailProp = GetProperty<SensorDetail>(context, nameof(SensorDetail.Data));

        // Assert
        Assert.Equal(mainProp.GetColumnName(), detailProp.GetColumnName());
        Assert.Equal("Данные в формате JSON.", mainProp.GetComment());
        Assert.Equal("Данные в формате JSON.", detailProp.GetComment());
    }

    [Fact]
    public void AutoComments_TableSplitting_SameColumn_DiffComment_Should_SetMergedComment()
    {
        // Arrange
        using var context = new SplitDiffColContext(BuildOptions<SplitDiffColContext>());

        // Act
        var mainProp = GetProperty<DeviceMain>(context, nameof(DeviceMain.StateInfo));
        var detailProp = GetProperty<DeviceDetail>(context, nameof(DeviceDetail.StateInfo));

        // Assert
        Assert.Equal(mainProp.GetColumnName(), detailProp.GetColumnName());
        Assert.Equal(mainProp.GetComment(), detailProp.GetComment());
        Assert.Contains("Текущий статус готовности устройства.", mainProp.GetComment());
        Assert.Contains("Полный лог последнего изменения состояния.", mainProp.GetComment());
    }
}

internal sealed class SameCommentSplitContext : DbContext
{
    public DbSet<Contract> Contracts { get; set; }

    public SameCommentSplitContext(DbContextOptions<SameCommentSplitContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(builder =>
        {
            builder.ToTable("Contracts");
            builder.HasKey(c => c.Id);
            builder.HasOne(c => c.Details)
                .WithOne()
                .HasForeignKey<ContractDetails>(d => d.Id);
        });

        modelBuilder.Entity<ContractDetails>(builder =>
        {
            builder.ToTable("Contracts");
            builder.HasKey(d => d.Id);
        });
    }
}

internal sealed class DifferentCommentSplitContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public DifferentCommentSplitContext(DbContextOptions<DifferentCommentSplitContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.HasOne(p => p.Details)
                .WithOne()
                .HasForeignKey<ProductDetails>(d => d.Id);
        });

        modelBuilder.Entity<ProductDetails>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(d => d.Id);
        });
    }
}

internal sealed class ManualCommentSplitContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ManualCommentSplitContext(DbContextOptions<ManualCommentSplitContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.HasOne(p => p.Details)
                .WithOne()
                .HasForeignKey<ProductDetails>(d => d.Id);
            builder.HasComment("Ручной комментарий.");
            builder.ToTable("Products");
        });

        modelBuilder.Entity<ProductDetails>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(d => d.Id);
        });
    }
}

internal sealed class SplitSameColContext : DbContext
{
    public SplitSameColContext(DbContextOptions<SplitSameColContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorMain>(builder =>
        {
            builder.ToTable("Sensors");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Data)
                .HasColumnName("PayloadJson");

            builder.HasOne<SensorDetail>()
                .WithOne()
                .HasForeignKey<SensorDetail>(d => d.Id);
        });

        modelBuilder.Entity<SensorDetail>(builder =>
        {
            builder.ToTable("Sensors");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Data)
                .HasColumnName("PayloadJson");
        });
    }
}

internal sealed class SplitDiffColContext : DbContext
{
    public SplitDiffColContext(DbContextOptions<SplitDiffColContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceMain>(builder =>
        {
            builder.ToTable("Devices");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StateInfo).HasColumnName("StatusRaw");

            builder.HasOne(e => e.Detail)
                .WithOne()
                .HasForeignKey<DeviceDetail>(d => d.Id);
        });

        modelBuilder.Entity<DeviceDetail>(builder =>
        {
            builder.ToTable("Devices");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StateInfo).HasColumnName("StatusRaw");
        });
    }
}