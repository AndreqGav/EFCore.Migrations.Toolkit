namespace EFCore.Migrations.Toolkit.Tests.Models;

/// <summary>
/// Блог.
/// </summary>
public class Blog
{
    /// <summary>
    /// Идентификатор блога.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название блога.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// URL блога.
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// Представление блога.
/// </summary>
public class BlogView
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// URL.
    /// </summary>
    public string Url { get; set; }
}