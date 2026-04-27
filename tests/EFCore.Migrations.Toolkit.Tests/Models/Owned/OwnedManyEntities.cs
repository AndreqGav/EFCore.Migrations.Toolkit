using System.Collections.Generic;

namespace EFCore.Migrations.Toolkit.Tests.Models.Owned;

/// <summary>
/// Корзина покупок.
/// </summary>
public class ShoppingCart
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Позиции корзины.
    /// </summary>
    public ICollection<CartItem> Items { get; set; }
}

/// <summary>
/// Позиция корзины.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Название товара.
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Количество.
    /// </summary>
    public int Quantity { get; set; }
}
