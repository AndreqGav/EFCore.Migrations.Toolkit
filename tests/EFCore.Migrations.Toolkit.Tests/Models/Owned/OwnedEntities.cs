namespace EFCore.Migrations.Toolkit.Tests.Models.Owned;

/// <summary>
/// Склад.
/// </summary>
public class Warehouse
{
    /// <summary>
    /// Идентификатор склада.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название склада.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Адресс.
    /// </summary>
    public WarehouseAddress Address { get; set; }
}

/// <summary>
/// Адрес склада.
/// </summary>
public class WarehouseAddress
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
/// Отгрузка.
/// </summary>
public class Shipment
{
    /// <summary>
    /// Идентификатор отгрузки.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Трек-номер.
    /// </summary>
    public string TrackNumber { get; set; }

    /// <summary>
    /// Адресс.
    /// </summary>
    public ShipmentAddress Address { get; set; }
}

/// <summary>
/// Адрес отгрузки.
/// </summary>
public class ShipmentAddress
{
    /// <summary>
    /// Улица доставки.
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// Город доставки.
    /// </summary>
    public string City { get; set; }
}
