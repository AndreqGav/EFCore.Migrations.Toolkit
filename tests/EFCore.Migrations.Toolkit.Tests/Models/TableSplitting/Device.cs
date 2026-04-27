namespace EFCore.Migrations.Toolkit.Tests.Models.TableSplitting;

/// <summary>
/// Сущность для мониторинга устройства.
/// </summary>
public class DeviceMain
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Текущий статус готовности устройства.
    /// </summary>
    public string StateInfo { get; set; }

    /// <summary>
    /// Подробности.
    /// </summary>
    public DeviceDetail Detail { get; set; }
}

/// <summary>
/// Сущность для глубокой диагностики.
/// </summary>
public class DeviceDetail
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Полный лог последнего изменения состояния.
    /// </summary>
    public string StateInfo { get; set; }
}