namespace EFCore.Migrations.Toolkit.Tests.Models.TableSplitting;

/// <summary>
/// Основная информация о датчике.
/// </summary>
public class SensorMain
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Данные в формате JSON.
    /// </summary>
    public string Data { get; set; }
}

/// <summary>
/// Технические подробности датчика.
/// </summary>
public class SensorDetail
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Данные в формате JSON.
    /// </summary>
    public string Data { get; set; }
}