namespace EFCore.Migrations.Toolkit.Tests.Models.Inheritance
{
    /// <summary>
    /// Базовый тип в наследовании TPH.
    /// </summary>
    public class PostBase
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Наследник А.
    /// </summary>
    public class PostA : PostBase
    {
        /// <summary>
        /// Текст А.
        /// </summary>
        public string TextA { get; set; }
    }

    /// <summary>
    /// Наследник Б.
    /// </summary>
    public class PostB : PostBase
    {
        /// <summary>
        /// Текст Б.
        /// </summary>
        public string TextB { get; set; }
    }
}