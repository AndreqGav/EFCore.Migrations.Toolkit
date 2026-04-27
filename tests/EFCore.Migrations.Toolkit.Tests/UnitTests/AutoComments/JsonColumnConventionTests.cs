using System.Linq;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Json;
using EFCore.Migrations.Toolkit.Tests.Models.Owned;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют поведение конвенции автокомментариев для JSON-колонок (OwnsOne + ToJson) и комплексных типов (ComplexProperty).
/// </summary>
public class JsonColumnConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments()
            .Options;
    }

    private static string GetTableComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    private static string GetOwnedEntityComment<TOwner, TOwned>(DbContext context, string navigationName)
    {
        var ownedType = ModelAccessor.GetModel(context)
            .GetEntityTypes()
            .Where(e => e.ClrType == typeof(TOwned) && e.IsOwned())
            .FirstOrDefault(e =>
                e.FindOwnership()?.PrincipalEntityType.ClrType == typeof(TOwner) &&
                e.FindOwnership()?.PrincipalToDependent?.Name == navigationName);

        return ownedType?.GetComment();
    }

    [Fact]
    public void AutoComments_JsonOwned_JsonColumn_Should_NavigationPropertyComment()
    {
        // Arrange
        using var context = new JsonOwnedContext(BuildOptions<JsonOwnedContext>());

        // Act
        var comment = GetTableComment<ReportMetadata>(context);

        // Assert
        Assert.Equal("Метаданные.", comment);
    }

    [Fact]
    public void AutoComments_TwoOwnedFields_Json_Should_SetNavigationPropertyComments()
    {
        // Arrange
        using var context = new TwoOwnedJsonContext(BuildOptions<TwoOwnedJsonContext>());

        // Act + Assert
        Assert.Equal("Адрес доставки.", GetOwnedEntityComment<CustomerOrder, Address>(context, nameof(CustomerOrder.ShippingAddress)));
        Assert.Equal("Адрес оплаты.", GetOwnedEntityComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress)));
    }

    /// <summary>
    /// https://github.com/dotnet/efcore/issues/33375
    /// </summary>
    [Fact]
    public void AutoComments_JsonComplexType_Should_NotSetPropertiesComment()
    {
        // Arrange
        using var context = new ComplexTypeContext(BuildOptions<ComplexTypeContext>());

        // Act
        var ticketType = ModelAccessor.GetModel(context).FindEntityType(typeof(Ticket))!;
        var seatComplexProp = ticketType.FindComplexProperty(nameof(Ticket.Seat))!;
        var rowComment = seatComplexProp.ComplexType.FindProperty(nameof(SeatInfo.Row))!.GetComment();
        var numberComment = seatComplexProp.ComplexType.FindProperty(nameof(SeatInfo.Number))!.GetComment();

        // Assert
        Assert.Null(rowComment);
        Assert.Null(numberComment);
    }
}

internal sealed class JsonOwnedContext : DbContext
{
    public DbSet<Report> Reports { get; set; }

    public JsonOwnedContext(DbContextOptions<JsonOwnedContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(r => r.Metadata, owned => { owned.ToJson(); });
        });
    }
}

internal sealed class ComplexTypeContext : DbContext
{
    public DbSet<Ticket> Tickets { get; set; }

    public ComplexTypeContext(DbContextOptions<ComplexTypeContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(t => t.Seat, c => c.ToJson());
        });
    }
}

internal sealed class TwoOwnedPlainContext : DbContext
{
    public DbSet<CustomerOrder> Orders { get; set; }

    public TwoOwnedPlainContext(DbContextOptions<TwoOwnedPlainContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerOrder>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(o => o.ShippingAddress);
            builder.OwnsOne(o => o.BillingAddress);
        });
    }
}