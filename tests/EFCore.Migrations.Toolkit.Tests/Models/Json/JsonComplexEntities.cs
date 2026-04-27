namespace EFCore.Migrations.Toolkit.Tests.Models.Json;

/// <summary>
/// Отчёт.
/// </summary>
public class Report
{
    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Метаданные.
    /// </summary>
    public ReportMetadata Metadata { get; set; }
}

/// <summary>
/// Метаданные отчёта.
/// </summary>
public class ReportMetadata
{
    /// <summary>
    /// Заголовок отчёта.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Автор отчёта.
    /// </summary>
    public string Author { get; set; }
}

/// <summary>
/// Билет.
/// </summary>
public class Ticket
{
    /// <summary>
    /// Идентификатор билета.
    /// </summary>
    public int Id { get; set; }

    public SeatInfo Seat { get; set; }
}

/// <summary>
/// Информация о месте.
/// </summary>
public class SeatInfo
{
    /// <summary>
    /// Номер ряда.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Номер места.
    /// </summary>
    public int Number { get; set; }
}
