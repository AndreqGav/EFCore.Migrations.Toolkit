namespace EFCore.Migrations.Toolkit.Tests.Models.Owned;

/// <summary>
/// Адрес.
/// </summary>
public class Address
{
    /// <summary>
    /// Улица.
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// Город.
    /// </summary>
    public string City { get; set; }
}

/// <summary>
/// Покупатель.
/// </summary>
public class Customer
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Адрес покупателя.
    /// </summary>
    public Address Address { get; set; }
}

/// <summary>
/// Поставщик.
/// </summary>
public class Supplier
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Адрес поставщика.
    /// </summary>
    public Address Address { get; set; }
}

/// <summary>
/// Заказ.
/// </summary>
public class CustomerOrder
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Адрес доставки.
    /// </summary>
    public Address ShippingAddress { get; set; }

    /// <summary>
    /// Адрес оплаты.
    /// </summary>
    public Address BillingAddress { get; set; }
}
