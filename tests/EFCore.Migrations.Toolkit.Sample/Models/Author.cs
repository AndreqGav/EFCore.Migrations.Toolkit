namespace EFCore.Migrations.Toolkit.Sample.Models;

/// <summary>
/// Автор публикаций.
/// </summary>
public class Author
{
    /// <summary>
    /// Идентификатор автора.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя автора.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Email автора.
    /// </summary>
    public string Email { get; set; } = "";

    public ICollection<Post> Posts { get; set; } = [];
}
