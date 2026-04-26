namespace EFCore.Migrations.Toolkit.Tests.Models.TableSplitting;

/// <summary>
/// Договор.
/// </summary>
public class Contract
{
    /// <summary>
    /// Идентификатор договора.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер договора.
    /// </summary>
    public string Number { get; set; }

    public ContractDetails Details { get; set; }
}

/// <summary>
/// Договор.
/// </summary>
public class ContractDetails
{
    /// <summary>
    /// Идентификатор договора.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Примечания к договору.
    /// </summary>
    public string Notes { get; set; }
}