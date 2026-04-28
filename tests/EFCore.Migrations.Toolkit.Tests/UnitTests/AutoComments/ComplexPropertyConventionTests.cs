using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты для ComplexProperty.
/// </summary>
public class ComplexPropertyConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments()
            .Options;
    }

    private static IComplexProperty GetComplexProperty<TEntity>(DbContext context, string complexPropertyName)
        => ModelAccessor.GetModel(context)
            .FindEntityType(typeof(TEntity))!
            .FindComplexProperty(complexPropertyName)!;

    private static IProperty GetProperty(IComplexProperty complexProperty, string propertyName)
        => complexProperty.ComplexType.FindProperty(propertyName);

    private static string GetEntityPropertyComment<TEntity>(DbContext context, string propertyName)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!.GetComment();

    [Fact]
    public void AutoComments_OwnerAndComplex_SamePropertyName_Should_SetDistinctComments()
    {
        // Arrange
        using var context = new OwnerAndComplexSharedPropertyNameContext(BuildOptions<OwnerAndComplexSharedPropertyNameContext>());

        // Act
        var ownerNameComment = GetEntityPropertyComment<Product>(context, nameof(Product.Name));
        var specProp = GetComplexProperty<Product>(context, nameof(Product.Spec));
        var complexNameComment = GetProperty(specProp, nameof(ProductSpec.Name)).GetComment();

        // Assert
        Assert.Equal("Название продукта.", ownerNameComment);
        Assert.Equal("Название характеристики.", complexNameComment);
    }

    [Fact]
    public void AutoComments_ComplexType_ManualPropertyComment_Should_PreserveManualComment()
    {
        // Arrange
        using var context = new ComplexTypeManualCommentContext(BuildOptions<ComplexTypeManualCommentContext>());

        // Act
        var seatProp = GetComplexProperty<Ticket>(context, nameof(Ticket.Seat))!;
        var rowProp = GetProperty(seatProp, nameof(SeatInfo.Row))!;

        // Assert
        Assert.Equal("ручной комментарий", rowProp.GetComment());
    }

    [Fact]
    public void AutoComments_ComplexType_ManualPropertyComment_Should_NotAffectOtherProperties()
    {
        // Arrange
        using var context = new ComplexTypeManualCommentContext(BuildOptions<ComplexTypeManualCommentContext>());

        // Act
        var seatProp = GetComplexProperty<Ticket>(context, nameof(Ticket.Seat))!;
        var numberProp = GetProperty(seatProp, nameof(SeatInfo.Number))!;

        // Assert
        Assert.Equal("Номер места.", numberProp.GetComment());
    }

    [Fact]
    public void AutoComments_ComplexType_SameTypeInDifferentEntities_Should_SetColumnComments()
    {
        // Arrange
        using var context = new MultiEntityComplexTypeContext(BuildOptions<MultiEntityComplexTypeContext>());

        // Act
        var employeeContact = GetComplexProperty<Employee>(context, nameof(Employee.Contact))!;
        var contractorContact = GetComplexProperty<Contractor>(context, nameof(Contractor.Contact))!;

        // Assert
        Assert.Equal("Телефон.", GetProperty(employeeContact, nameof(ContactInfo.Phone)).GetComment());
        Assert.Equal("Email.", GetProperty(employeeContact, nameof(ContactInfo.Email)).GetComment());
        Assert.Equal("Телефон.", GetProperty(contractorContact, nameof(ContactInfo.Phone)).GetComment());
        Assert.Equal("Email.", GetProperty(contractorContact, nameof(ContactInfo.Email)).GetComment());
    }

    [Fact]
    public void AutoComments_ComplexType_MultipleOnSameEntity_Should_SetColumnComments()
    {
        // Arrange
        using var context = new MultipleComplexPropsContext(BuildOptions<MultipleComplexPropsContext>());

        // Act
        var homeContact = GetComplexProperty<Staff>(context, nameof(Staff.HomeContact))!;
        var workContact = GetComplexProperty<Staff>(context, nameof(Staff.WorkContact))!;

        // Assert — each complex property independently gets XML comments from ContactInfo
        Assert.Equal("Телефон.", GetProperty(homeContact, nameof(ContactInfo.Phone)).GetComment());
        Assert.Equal("Email.", GetProperty(homeContact, nameof(ContactInfo.Email)).GetComment());
        Assert.Equal("Телефон.", GetProperty(workContact, nameof(ContactInfo.Phone)).GetComment());
        Assert.Equal("Email.", GetProperty(workContact, nameof(ContactInfo.Email)).GetComment());
    }

    [Fact]
    public void AutoComments_ComplexType_Nested_Should_SetColumnComments_OnNestedProperties()
    {
        // Arrange
        using var context = new NestedComplexTypeContext(BuildOptions<NestedComplexTypeContext>());

        // Act
        var homeAddress = GetComplexProperty<Person>(context, nameof(Person.HomeAddress))!;
        var contactProp = homeAddress.ComplexType.FindComplexProperty(nameof(PostalAddress.Contact))!;

        // Assert
        Assert.Equal("Телефон.", GetProperty(contactProp, nameof(ContactInfo.Phone))!.GetComment());
        Assert.Equal("Email.", GetProperty(contactProp, nameof(ContactInfo.Email))!.GetComment());
    }
}

internal sealed class OwnerAndComplexSharedPropertyNameContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public OwnerAndComplexSharedPropertyNameContext(DbContextOptions<OwnerAndComplexSharedPropertyNameContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(p => p.Spec, c => c.HasDiscriminator());
        });
    }
}

internal sealed class ComplexTypeManualCommentContext : DbContext
{
    public ComplexTypeManualCommentContext(DbContextOptions<ComplexTypeManualCommentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(t => t.Seat, seat =>
            {
                seat
                    .Property(s => s.Row)
                    .HasComment("ручной комментарий");
            });
        });
    }
}

internal sealed class MultiEntityComplexTypeContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    public DbSet<Contractor> Contractors { get; set; }

    public MultiEntityComplexTypeContext(DbContextOptions<MultiEntityComplexTypeContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(e => e.Contact, c => c.HasDiscriminator());
        });

        modelBuilder.Entity<Contractor>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(e => e.Contact, c => c.HasDiscriminator());
        });
    }
}

internal sealed class MultipleComplexPropsContext : DbContext
{
    public DbSet<Staff> Staff { get; set; }

    public MultipleComplexPropsContext(DbContextOptions<MultipleComplexPropsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Staff>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(s => s.HomeContact, c => c.HasDiscriminator());
            builder.ComplexProperty(s => s.WorkContact, c => c.HasDiscriminator());
        });
    }
}

internal sealed class NestedComplexTypeContext : DbContext
{
    public DbSet<Person> Persons { get; set; }

    public NestedComplexTypeContext(DbContextOptions<NestedComplexTypeContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ComplexProperty(p => p.HomeAddress, addr =>
            {
                addr.ComplexProperty(a => a.Contact, c => c.HasDiscriminator());

                addr.HasDiscriminator();
            });
        });
    }
}