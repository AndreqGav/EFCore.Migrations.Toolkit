using EFCore.Migrations.AutoComments.Attributes;

namespace EFCore.Migrations.Toolkit.Sample.Models;

/// <summary>
/// Публикация в блоге.
/// </summary>
public class Post
{
    /// <summary>
    /// Идентификатор публикации.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Заголовок публикации.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Содержимое публикации.
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// Статус публикации.
    /// </summary>
    public PostStatus Status { get; set; }

    /// <summary>
    /// Идентификатор автора.
    /// </summary>
    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}

/// <summary>
/// Статус публикации.
/// </summary>
public enum PostStatus
{
    /// <summary>
    /// Черновик, не опубликован.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Опубликован и доступен читателям.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Архивирован, скрыт от читателей.
    /// </summary>
    Archived = 2,
}
