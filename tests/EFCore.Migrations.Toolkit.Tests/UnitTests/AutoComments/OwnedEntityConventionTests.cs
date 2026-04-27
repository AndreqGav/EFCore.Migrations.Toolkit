using System.Linq;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Owned;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют, что конвенция корректно обрабатывает Owned-сущности.
/// </summary>
public class OwnedEntityConventionTests
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

    private static string GetOwnedEntityComment<TOwned>(DbContext context)
    {
        var ownedType = ModelAccessor.GetModel(context)
            .GetEntityTypes()
            .First(e => e.ClrType == typeof(TOwned) && e.IsOwned());

        return ownedType.GetComment();
    }

    private static string GetOwnedPropertyComment<TOwned>(DbContext context, string propertyName)
    {
        var ownedType = ModelAccessor.GetModel(context)
            .GetEntityTypes()
            .First(e => e.ClrType == typeof(TOwned) && e.IsOwned());

        return ownedType.FindProperty(propertyName)!.GetComment();
    }

    private static string GetComment<TOwner, TOwned>(DbContext context, string navigationName, string propertyName)
    {
        var ownedType = ModelAccessor.GetModel(context)
            .GetEntityTypes()
            .Where(e => e.ClrType == typeof(TOwned) && e.IsOwned())
            .FirstOrDefault(e =>
                e.FindOwnership()?.PrincipalEntityType.ClrType == typeof(TOwner) &&
                e.FindOwnership()?.PrincipalToDependent?.Name == navigationName);

        return ownedType?.FindProperty(propertyName)?.GetComment();
    }

    [Fact]
    public void AutoComments_OwnsOne_Should_SetTableComment_OnOwner()
    {
        // Arrange
        using var context = new SimpleOwnedContext(BuildOptions<SimpleOwnedContext>());

        // Act
        var comment = GetTableComment<Warehouse>(context);

        // Assert
        Assert.Equal("Склад.", comment);
    }

    [Fact]
    public void AutoComments_OwnsOne_Should_NotSetComment_OnOwnedType()
    {
        // Arrange
        using var context = new SimpleOwnedContext(BuildOptions<SimpleOwnedContext>());

        // Act
        var comment = GetOwnedEntityComment<WarehouseAddress>(context);

        // Assert
        Assert.Null(comment);
    }

    [Fact]
    public void AutoComments_OwnsOne_Should_SetColumnComments_OnOwnedProperties()
    {
        // Arrange
        using var context = new SimpleOwnedContext(BuildOptions<SimpleOwnedContext>());

        // Act + Assert
        Assert.Equal("Улица.", GetOwnedPropertyComment<WarehouseAddress>(context, nameof(WarehouseAddress.Street)));
        Assert.Equal("Город.", GetOwnedPropertyComment<WarehouseAddress>(context, nameof(WarehouseAddress.City)));
    }

    [Fact]
    public void AutoComments_OwnsOneWithTable_Should_SetTableComment_OnOwner()
    {
        // Arrange
        using var context = new OwnedWithTableContext(BuildOptions<OwnedWithTableContext>());

        // Act
        var comment = GetTableComment<Shipment>(context);

        // Assert
        Assert.Equal("Отгрузка.", comment);
    }

    [Fact]
    public void AutoComments_OwnsOneWithTable_Should_SetTableComment_OnOwnedType()
    {
        // Arrange
        using var context = new OwnedWithTableContext(BuildOptions<OwnedWithTableContext>());

        // Act
        var comment = GetOwnedEntityComment<ShipmentAddress>(context);

        // Assert
        Assert.Equal("Адрес отгрузки.", comment);
    }

    [Fact]
    public void AutoComments_OwnedWithTable_Should_SetColumnComments_OnOwnedProperties()
    {
        // Arrange
        using var context = new OwnedWithTableContext(BuildOptions<OwnedWithTableContext>());

        // Act + Assert — SetColumnComments обходит все entity types включая owned
        Assert.Equal("Улица доставки.", GetOwnedPropertyComment<ShipmentAddress>(context, nameof(ShipmentAddress.Street)));
        Assert.Equal("Город доставки.", GetOwnedPropertyComment<ShipmentAddress>(context, nameof(ShipmentAddress.City)));
    }

    [Fact]
    public void AutoComments_OwnsMany_Should_SetTableComment_OnOwner()
    {
        // Arrange
        using var context = new OwnsManyContext(BuildOptions<OwnsManyContext>());

        // Act
        var comment = GetTableComment<ShoppingCart>(context);

        // Assert
        Assert.Equal("Корзина покупок.", comment);
    }

    [Fact]
    public void AutoComments_OwnsMany_Should_SetTableComment_OnOwnedType()
    {
        // Arrange
        using var context = new OwnsManyContext(BuildOptions<OwnsManyContext>());

        // Act
        var comment = GetOwnedEntityComment<CartItem>(context);

        // Assert
        Assert.Equal("Позиция корзины.", comment);
    }


    [Fact]
    public void AutoComments_SharedOwned_DifferentOwners_Should_NotSetEntityComment()
    {
        // Arrange
        using var context = new SharedOwnedPlainContext(BuildOptions<SharedOwnedPlainContext>());

        // Act + Assert
        Assert.Null(GetOwnedEntityComment<Customer, Address>(context, nameof(Customer.Address)));
        Assert.Null(GetOwnedEntityComment<Supplier, Address>(context, nameof(Supplier.Address)));
    }

    [Fact]
    public void AutoComments_SharedOwned_DifferentOwners_Should_SetColumnComments()
    {
        // Arrange
        using var context = new SharedOwnedPlainContext(BuildOptions<SharedOwnedPlainContext>());

        // Act + Assert
        Assert.Equal("Улица.", GetComment<Customer, Address>(context, nameof(Customer.Address), nameof(Address.Street)));
        Assert.Equal("Город.", GetComment<Customer, Address>(context, nameof(Customer.Address), nameof(Address.City)));
        Assert.Equal("Улица.", GetComment<Supplier, Address>(context, nameof(Supplier.Address), nameof(Address.Street)));
        Assert.Equal("Город.", GetComment<Supplier, Address>(context, nameof(Supplier.Address), nameof(Address.City)));
    }

    [Fact]
    public void AutoComments_TwoOwnedFields_Should_SetColumnComments()
    {
        // Arrange
        using var context = new TwoOwnedPlainContext(BuildOptions<TwoOwnedPlainContext>());

        // Act + Assert
        Assert.Equal("Улица.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.ShippingAddress), nameof(Address.Street)));
        Assert.Equal("Город.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.ShippingAddress), nameof(Address.City)));
        Assert.Equal("Улица.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress), nameof(Address.Street)));
        Assert.Equal("Город.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress), nameof(Address.City)));
    }

    [Fact]
    public void AutoComments_TwoOwnedFields_Mixed_JsonField_Should_SetNavigationComment()
    {
        // Arrange
        using var context = new TwoOwnedMixedContext(BuildOptions<TwoOwnedMixedContext>());

        // Act + Assert
        Assert.Equal("Адрес доставки.", GetOwnedEntityComment<CustomerOrder, Address>(context, nameof(CustomerOrder.ShippingAddress)));
        Assert.Null(GetOwnedEntityComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress)));
    }

    [Fact]
    public void AutoComments_TwoOwnedFields_Mixed_PlainField_Should_SetColumnComments()
    {
        // Arrange
        using var context = new TwoOwnedMixedContext(BuildOptions<TwoOwnedMixedContext>());

        // Act + Assert
        Assert.Equal("Улица.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress), nameof(Address.Street)));
        Assert.Equal("Город.", GetComment<CustomerOrder, Address>(context, nameof(CustomerOrder.BillingAddress), nameof(Address.City)));
    }

    [Fact]
    public void AutoComments_OwnedWithTable_ManualTableComment_Should_PreserveManualComment()
    {
        // Arrange
        using var context = new OwnedManualTableCommentContext(BuildOptions<OwnedManualTableCommentContext>());

        // Act
        var ownedType = ModelAccessor.GetModel(context)
            .GetEntityTypes()
            .First(e => e.ClrType == typeof(Address) && e.IsOwned());

        // Assert
        Assert.Equal("ручной комментарий", ownedType.GetComment());
    }

    [Fact]
    public void AutoComments_OwnedInline_ManualColumnComment_Should_PreserveManualComment()
    {
        // Arrange
        using var context = new OwnedManualColumnCommentContext(BuildOptions<OwnedManualColumnCommentContext>());

        // Act
        var comment = GetComment<Customer, Address>(context, nameof(Customer.Address), nameof(Address.Street));

        // Assert
        Assert.Equal("ручной комментарий на колонку", comment);
    }

    [Fact]
    public void AutoComments_OwnedInline_ManualColumnComment_Should_NotAffectOtherProperties()
    {
        // Arrange
        using var context = new OwnedManualColumnCommentContext(BuildOptions<OwnedManualColumnCommentContext>());

        // Act
        var cityComment = GetComment<Customer, Address>(context, nameof(Customer.Address), nameof(Address.City));

        // Assert
        Assert.Equal("Город.", cityComment);
    }
}

internal sealed class OwnsManyContext : DbContext
{
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public OwnsManyContext(DbContextOptions<OwnsManyContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingCart>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsMany(c => c.Items);
        });
    }
}

internal sealed class SharedOwnedPlainContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }

    public SharedOwnedPlainContext(DbContextOptions<SharedOwnedPlainContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(c => c.Address);
        });

        modelBuilder.Entity<Supplier>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(s => s.Address);
        });
    }
}

internal sealed class TwoOwnedJsonContext : DbContext
{
    public DbSet<CustomerOrder> Orders { get; set; }

    public TwoOwnedJsonContext(DbContextOptions<TwoOwnedJsonContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerOrder>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(o => o.ShippingAddress, owned => { owned.ToJson(); });
            builder.OwnsOne(o => o.BillingAddress, owned => { owned.ToJson(); });
        });
    }
}

internal sealed class TwoOwnedMixedContext : DbContext
{
    public DbSet<CustomerOrder> Orders { get; set; }

    public TwoOwnedMixedContext(DbContextOptions<TwoOwnedMixedContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerOrder>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(o => o.ShippingAddress, owned => { owned.ToJson(); });
            builder.OwnsOne(o => o.BillingAddress);
        });
    }
}

internal sealed class OwnedManualTableCommentContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public OwnedManualTableCommentContext(DbContextOptions<OwnedManualTableCommentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(c => c.Address, owned =>
            {
                owned
                    .ToTable("CustomerAddresses",
                        t => t.HasComment("ручной комментарий"));
            });
        });
    }
}

internal sealed class OwnedManualColumnCommentContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public OwnedManualColumnCommentContext(DbContextOptions<OwnedManualColumnCommentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(c => c.Address, owned => { owned.Property(a => a.Street).HasComment("ручной комментарий на колонку"); });
        });
    }
}

internal sealed class SimpleOwnedContext : DbContext
{
    public DbSet<Warehouse> Warehouses { get; set; }

    public SimpleOwnedContext(DbContextOptions<SimpleOwnedContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warehouse>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(w => w.Address);
        });
    }
}

internal sealed class OwnedWithTableContext : DbContext
{
    public DbSet<Shipment> Shipments { get; set; }

    public OwnedWithTableContext(DbContextOptions<OwnedWithTableContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.OwnsOne(s => s.Address, owned => { owned.ToTable("ShipmentAddresses"); });
        });
    }
}