namespace EFCore.Migrations.Toolkit.Tests.Models
{
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
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}