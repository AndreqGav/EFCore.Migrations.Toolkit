namespace EFCore.Migrations.Toolkit.Tests.Models.Inheritance;

/// <summary>
/// Базовый тип в наследовании TPT.
/// </summary>
public class ArticleBase
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Наследник А в TPT.
/// </summary>
public class ArticleA : ArticleBase
{
    /// <summary>
    /// Специфичное содержимое А.
    /// </summary>
    public string ContentA { get; set; }
}

/// <summary>
/// Наследник Б в TPT.
/// </summary>
public class ArticleB : ArticleBase
{
    /// <summary>
    /// Специфичное содержимое Б.
    /// </summary>
    public string ContentB { get; set; }
}