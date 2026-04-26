using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Triggers.SqlServer;
using EFCore.Migrations.Triggers.SqlServer.Enums;
using EFCore.Migrations.Triggers.SqlServer.Models;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.Triggers.SqlServer;

public class SqlServerTriggerSqlGeneratorTests
{
    private readonly SqlServerTriggerSqlGenerator _generator;

    public SqlServerTriggerSqlGeneratorTests()
    {
        _generator = new SqlServerTriggerSqlGenerator(new FakeSqlGenerationHelper());
    }

    private static SqlServerTriggerObject MakeTrigger(
        string name = "my_trigger",
        string table = "my_table",
        TriggerOperationEnum operation = TriggerOperationEnum.Insert,
        TriggerTimeEnum time = TriggerTimeEnum.After,
        string body = "PERFORM 1;")
        => new SqlServerTriggerObject
        {
            Name = name,
            Table = table,
            Operation = operation,
            Time = time,
            Body = body,
        };

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnExactFullSql()
    {
        // Arrange
        var trigger = MakeTrigger(name: "my_trigger", table: "my_table", body: "PERFORM 1;",
            time: TriggerTimeEnum.After, operation: TriggerOperationEnum.Insert);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Equal(
            "CREATE OR ALTER TRIGGER \"my_trigger\"\nON \"my_table\"\nAFTER INSERT\nAS\nBEGIN\n    SET NOCOUNT ON;\nPERFORM 1;\nEND;",
            sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainCreateOrAlterTrigger()
    {
        // Arrange
        var trigger = MakeTrigger();

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE OR ALTER TRIGGER", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainBeginEnd()
    {
        // Arrange
        var trigger = MakeTrigger();

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("BEGIN", sql);
        Assert.Contains("END;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainSetNoCountOn()
    {
        // Arrange
        var trigger = MakeTrigger();

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("SET NOCOUNT ON;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainBody()
    {
        // Arrange
        var trigger = MakeTrigger(body: "UPDATE [Orders] SET [IsConfirmed] = 0;");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("UPDATE [Orders] SET [IsConfirmed] = 0;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_ForAfterTime()
    {
        // Arrange
        var trigger = MakeTrigger(time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainINSTEAD_OF_ForInsteadOfTime()
    {
        // Arrange
        var trigger = MakeTrigger(time: TriggerTimeEnum.InsteadOf);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("INSTEAD OF", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_INSERT_ForInsertOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Insert, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER INSERT", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_UPDATE_ForUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Update, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER UPDATE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_DELETE_ForDeleteOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Delete, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER DELETE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_INSERT_UPDATE_ForInsertOrUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.InsertOrUpdate, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER INSERT, UPDATE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_INSERT_DELETE_ForInsertOrDeleteOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.InsertOrDelete, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER INSERT, DELETE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_UPDATE_DELETE_ForUpdateOrDeleteOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.UpdateOrDelete, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER UPDATE, DELETE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainAFTER_INSERT_UPDATE_DELETE_ForInsertOrUpdateOrDeleteOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.InsertOrUpdateOrDelete, time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("AFTER INSERT, UPDATE, DELETE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainINSTEAD_OF_INSERT_ForInsteadOfInsert()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Insert, time: TriggerTimeEnum.InsteadOf);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("INSTEAD OF INSERT", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainINSTEAD_OF_UPDATE_DELETE_ForInsteadOfUpdateOrDelete()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.UpdateOrDelete, time: TriggerTimeEnum.InsteadOf);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("INSTEAD OF UPDATE, DELETE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_QuoteTriggerName()
    {
        // Arrange
        var trigger = MakeTrigger(name: "trg_on_insert");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE OR ALTER TRIGGER \"trg_on_insert\"", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_QuoteTableName()
    {
        // Arrange
        var trigger = MakeTrigger(table: "Orders");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("ON \"Orders\"", sql);
    }

    [Fact]
    public void GenerateDeleteTriggerSql_Should_GenerateDropTrigger()
    {
        // Arrange
        var trigger = MakeTrigger(name: "my_trigger");

        // Act
        var sql = _generator.GenerateDeleteTriggerSql(trigger);

        // Assert
        Assert.Equal("DROP TRIGGER \"my_trigger\";", sql);
    }

    [Fact]
    public void SqlUpModel_Sql_Should_NormalizeCrLfToLf()
    {
        // Arrange
        var trigger = MakeTrigger(name: "fn_on_insert", body: "line1;\r\nline2;");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.DoesNotContain("\r", sql);
        Assert.DoesNotContain("\r\n", sql);
        Assert.Contains("line1;\nline2;", sql);
    }
}