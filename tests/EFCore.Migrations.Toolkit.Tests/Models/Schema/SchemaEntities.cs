namespace EFCore.Migrations.Toolkit.Tests.Models.Schema;

/// <summary>
/// Счёт в домене.
/// </summary>
public class DomainInvoice
{
    /// <summary>
    /// Идентификатор счёта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер счёта.
    /// </summary>
    public string Number { get; set; }
}

/// <summary>
/// Счёт в биллинге.
/// </summary>
public class BillingInvoice
{
    /// <summary>
    /// Идентификатор счёта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Сумма счёта.
    /// </summary>
    public decimal Amount { get; set; }
}
