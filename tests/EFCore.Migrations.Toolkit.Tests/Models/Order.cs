using EFCore.Migrations.AutoComments.Attributes;

namespace EFCore.Migrations.Toolkit.Tests.Models
{
    /// <summary>
    /// Заказ покупателя.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Идентификатор заказа.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер заказа.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        ///     Итоговая сумма заказа в рублях.     
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Статус подтверждения заказа.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Статус заказа.
        /// </summary>
        [AutoCommentEnumDescription]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Категория заказа.
        /// </summary>
        public OrderCategory Category { get; set; }

        /// <summary>
        /// Способ доставки.
        /// </summary>
        [IgnoreAutoCommentEnumDescription]
        public DeliveryMethod DeliveryMethod { get; set; }
    }

    /// <summary>
    /// Представление каталога заказов.
    /// </summary>
    public class OrderCatalogView
    {
        /// <summary>
        /// Номер заказа.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Итоговая сумма заказа в рублях.
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Запрос каталога заказов.
    /// </summary>
    public class OrderCatalogSqlQuery
    {
        /// <summary>
        /// Номер заказа.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Итоговая сумма заказа в рублях.
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Статус заказа.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Активный, ожидает выполнения.
        /// </summary>
        Active = 0,

        /// <summary>
        /// Выполнен, доставлен покупателю.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Отменён, возврат средств.
        /// </summary>
        Cancelled = 2,
    }

    /// <summary>
    /// Категория заказа.
    /// </summary>
    public enum OrderCategory
    {
        /// <summary>
        /// Одежда.
        /// </summary>
        Clothing,

        /// <summary>
        /// Книги.
        /// </summary>
        Books,

        /// <summary>
        /// Игрушки.
        /// </summary>
        Toys,
    }

    /// <summary>
    /// Способ доставки заказа.
    /// </summary>
    public enum DeliveryMethod
    {
        /// <summary>
        /// Самовывоз из пункта выдачи.
        /// </summary>
        Pickup,

        /// <summary>
        /// Курьерская доставка до двери.
        /// </summary>
        Courier,

        /// <summary>
        /// Почтовая служба.
        /// </summary>
        Post,
    }
}