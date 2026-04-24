namespace EFCore.Migrations.Toolkit.Tests.Models.Inheritance
{
    /// <summary>
    /// Абстрактный базовый тип в наследовании TPC.
    /// </summary>
    public abstract class BlogBase
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Наследник А в TPC.
    /// </summary>
    public class BlogA : BlogBase
    {
        /// <summary>
        /// Имя А.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Наследник Б в TPC.
    /// </summary>
    public class BlogB : BlogBase
    {
        /// <summary>
        /// Имя Б.
        /// </summary>
        public string Name { get; set; }
    }
}