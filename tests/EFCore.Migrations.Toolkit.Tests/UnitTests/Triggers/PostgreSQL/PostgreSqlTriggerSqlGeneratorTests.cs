using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Triggers.PostgreSQL;
using EFCore.Migrations.Triggers.PostgreSQL.Enums;
using EFCore.Migrations.Triggers.PostgreSQL.Models;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.Triggers.PostgreSQL;

public class PostgreSqlTriggerSqlGeneratorTests
{
    private readonly PostgreSqlTriggerSqlGenerator _generator;

    public PostgreSqlTriggerSqlGeneratorTests()
    {
        _generator = new PostgreSqlTriggerSqlGenerator(new FakeSqlGenerationHelper());
    }

    private static PostgreSqlTriggerObject MakeTrigger(
        string name = "my_trigger",
        string table = "my_table",
        string schema = null,
        TriggerOperationEnum operation = TriggerOperationEnum.Insert,
        TriggerTimeEnum time = TriggerTimeEnum.Before,
        string body = "PERFORM 1;",
        ConstraintTriggerType? constraintType = null)
        => new PostgreSqlTriggerObject
        {
            Name = name,
            Schema = schema,
            Table = table,
            Operation = operation,
            Time = time,
            Body = body,
            ConstraintType = constraintType,
        };

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnExactFullSql()
    {
        // Arrange
        var trigger = MakeTrigger(name: "my_trigger", table: "my_table", body: "PERFORM 1;", time: TriggerTimeEnum.Before,
            operation: TriggerOperationEnum.Insert);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Equal(
            "CREATE FUNCTION \"my_trigger\"() RETURNS trigger as $my_trigger$\nBEGIN\nPERFORM 1;\nRETURN NEW;\nEND;\n$my_trigger$ LANGUAGE plpgsql;\n\nCREATE TRIGGER \"my_trigger\" BEFORE INSERT\nON \"my_table\"\nFOR EACH ROW EXECUTE PROCEDURE \"my_trigger\"();",
            sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainFunctionDefinition()
    {
        // Arrange
        var trigger = MakeTrigger(name: "fn_test", body: "PERFORM 1;");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE FUNCTION \"fn_test\"() RETURNS trigger", sql);
        Assert.Contains("$fn_test$", sql);
        Assert.Contains("LANGUAGE plpgsql", sql);
        Assert.Contains("PERFORM 1;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_EndsWithForEachRowExecuteProcedure()
    {
        // Arrange
        var trigger = MakeTrigger("my_trigger");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.EndsWith("FOR EACH ROW EXECUTE PROCEDURE \"my_trigger\"();", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnNEW_ForInsertOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Insert);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("RETURN NEW;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnNEW_ForUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Update);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("RETURN NEW;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnNEW_ForInsertOrUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.InsertOrUpdate);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("RETURN NEW;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ReturnOLD_ForDeleteOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Delete);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("RETURN OLD;", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainBEFORE_ForBeforeTime()
    {
        // Arrange
        var trigger = MakeTrigger(time: TriggerTimeEnum.Before);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("BEFORE", sql);
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
    public void GenerateCreateTriggerSql_Should_ContainINSTEAD_OF_ForInsteadTime()
    {
        // Arrange
        var trigger = MakeTrigger(time: TriggerTimeEnum.Instead);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("INSTEAD OF", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainBEFORE_INSERT_ForInsertOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Insert, time: TriggerTimeEnum.Before);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("BEFORE INSERT", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainBEFORE_UPDATE_ForUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.Update, time: TriggerTimeEnum.Before);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("BEFORE UPDATE", sql);
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
    public void GenerateCreateTriggerSql_Should_ContainBEFORE_INSERT_OR_UPDATE_ForInsertOrUpdateOperation()
    {
        // Arrange
        var trigger = MakeTrigger(operation: TriggerOperationEnum.InsertOrUpdate, time: TriggerTimeEnum.Before);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("BEFORE INSERT OR UPDATE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_CreateRegularTrigger_WithoutConstraintKeywords()
    {
        // Arrange
        var trigger = MakeTrigger(time: TriggerTimeEnum.After);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE TRIGGER", sql);
        Assert.DoesNotContain("CONSTRAINT TRIGGER", sql);
        Assert.DoesNotContain("DEFERRABLE", sql);
        Assert.DoesNotContain("NOT DEFERRABLE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainCONSTRAINT_TRIGGER_And_NOT_DEFERRABLE()
    {
        // Arrange
        var trigger = MakeTrigger(constraintType: ConstraintTriggerType.NotDeferrable);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE CONSTRAINT TRIGGER", sql);
        Assert.Contains("NOT DEFERRABLE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainDEFERRABLE_INITIALLY_IMMEDIATE()
    {
        // Arrange
        var trigger = MakeTrigger(constraintType: ConstraintTriggerType.DeferrableInitiallyImmediate);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE CONSTRAINT TRIGGER", sql);
        Assert.Contains("DEFERRABLE INITIALLY IMMEDIATE", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_Should_ContainDEFERRABLE_INITIALLY_DEFERRED()
    {
        // Arrange
        var trigger = MakeTrigger(constraintType: ConstraintTriggerType.DeferrableInitiallyDeferred);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("CREATE CONSTRAINT TRIGGER", sql);
        Assert.Contains("DEFERRABLE INITIALLY DEFERRED", sql);
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
    public void GenerateDeleteTriggerSql_Should_GenerateDropFunctionCascade()
    {
        // Arrange
        var trigger = MakeTrigger(name: "my_trigger");

        // Act
        var sql = _generator.GenerateDeleteTriggerSql(trigger);

        // Assert
        Assert.Equal("DROP FUNCTION \"my_trigger\"() CASCADE;", sql);
    }

    [Fact]
    public void GenerateDeleteTriggerSql_Should_QuoteTriggerName()
    {
        // Arrange
        var trigger = MakeTrigger(name: "fn_on_insert");

        // Act
        var sql = _generator.GenerateDeleteTriggerSql(trigger);

        // Assert
        Assert.Contains("\"fn_on_insert\"", sql);
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

    [Fact]
    public void GenerateCreateTriggerSql_WithSchema_Should_ContainQualifiedTableName()
    {
        // Arrange
        var trigger = MakeTrigger(table: "orders", schema: "public");

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("ON \"public\".\"orders\"", sql);
    }

    [Fact]
    public void GenerateCreateTriggerSql_WithoutSchema_Should_ContainUnqualifiedTableName()
    {
        // Arrange
        var trigger = MakeTrigger(table: "orders", schema: null);

        // Act
        var sql = _generator.GenerateCreateTriggerSql(trigger);

        // Assert
        Assert.Contains("ON \"orders\"", sql);
        Assert.DoesNotContain("null", sql);
    }
}