namespace EFCore.Migrations.Toolkit.Tests.Models.Inheritance;

internal abstract class EntityBase
{
    public int Id { get; set; }
}

/// <summary>
/// Базовый тип платежа.
/// </summary>
internal abstract class PaymentBase : EntityBase
{
    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Оплата A.
/// </summary>
internal class PaymentA : PaymentBase
{
    /// <summary>
    /// Доп. сведения A.
    /// </summary>
    public string Extra1 { get; set; }
    
    /// <summary>
    /// Общие сведения.
    /// </summary>
    public string ExtraShared { get; set; }
}

/// <summary>
/// Оплата B.
/// </summary>
internal class PaymentB : PaymentBase
{
    /// <summary>
    /// Доп. сведения B.
    /// </summary>
    public string Extra2 { get; set; }
    
    /// <summary>
    /// Общие сведения.
    /// </summary>
    public string ExtraShared { get; set; }
}