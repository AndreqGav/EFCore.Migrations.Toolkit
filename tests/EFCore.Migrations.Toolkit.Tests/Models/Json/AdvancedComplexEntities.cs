namespace EFCore.Migrations.Toolkit.Tests.Models.Json;

/// <summary>
/// Контактная информация.
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Телефон.
    /// </summary>
    public required string Phone { get; set; }

    /// <summary>
    /// Email.
    /// </summary>
    public string Email { get; set; }
}

/// <summary>
/// Почтовый адрес.
/// </summary>
public class PostalAddress
{
    /// <summary>
    /// Улица.
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// Контакт.
    /// </summary>
    public ContactInfo Contact { get; set; }
}

/// <summary>
/// Сотрудник.
/// </summary>
public class Employee
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Контакты.
    /// </summary>
    public ContactInfo Contact { get; set; }
}

/// <summary>
/// Подрядчик.
/// </summary>
public class Contractor
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Контакты.
    /// </summary>
    public ContactInfo Contact { get; set; }
}

/// <summary>
/// Персонал.
/// </summary>
public class Staff
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Домашние контакты.
    /// </summary>
    public ContactInfo HomeContact { get; set; }

    /// <summary>
    /// Рабочие контакты.
    /// </summary>
    public ContactInfo WorkContact { get; set; }
}

/// <summary>
/// Человек.
/// </summary>
public class Person
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Домашний адрес.
    /// </summary>
    public PostalAddress HomeAddress { get; set; }
}

/// <summary>
/// Продукт.
/// </summary>
public class Product
{
    /// <summary>
    /// Идентификатор продукта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название продукта.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Характеристика.
    /// </summary>
    public ProductSpec Spec { get; set; }
}

/// <summary>
/// Характеристика продукта.
/// </summary>
public class ProductSpec
{
    /// <summary>
    /// Название характеристики.
    /// </summary>
    public string Name { get; set; }
}