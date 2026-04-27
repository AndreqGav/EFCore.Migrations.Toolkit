namespace EFCore.Migrations.Toolkit.Tests.Models.Inheritance;

/// <summary>
/// Базовое уведомление.
/// </summary>
public class NotificationBase
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Уведомление, отправляемое через SMS-шлюз.
/// </summary>
public class SmsNotification : NotificationBase
{
    /// <summary>
    /// Текст сообщения для отправки.
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// Уведомление для электронной почты.
/// </summary>
public class EmailNotification : NotificationBase
{
    /// <summary>
    /// Текст сообщения для отправки.
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// Системное уведомление о работе сервиса.
/// </summary>
public class SystemNotification : NotificationBase
{
    /// <summary>
    /// Системный код события (INFO, WARN, ERROR).
    /// </summary>
    public string Content { get; set; }
}