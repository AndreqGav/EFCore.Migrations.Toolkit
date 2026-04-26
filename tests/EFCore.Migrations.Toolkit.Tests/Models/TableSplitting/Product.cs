namespace EFCore.Migrations.Toolkit.Tests.Models.TableSplitting;

/// <summary>
/// Продукт.
/// </summary>
public class Product
{
    /// <summary>
    /// Идентификатор продукта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название продукта.
    /// </summary>
    public string Name { get; set; }

    public ProductDetails Details { get; set; }
}

/// <summary>
/// Детали продукта.
/// </summary>
public class ProductDetails
{
    /// <summary>
    /// Идентификатор продукта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Описание продукта.
    /// </summary>
    public string Description { get; set; }
}