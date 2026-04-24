# EFCore.Migrations.Toolkit

A collection of EF Core extension libraries for automatic database comments, custom SQL management, and trigger generation.

## Packages

### [EFCore.Migrations.AutoComments](#efcoremigrationsautocomments-1)
[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.AutoComments)](https://www.nuget.org/packages/EFCore.Migrations.AutoComments) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.AutoComments)](https://www.nuget.org/packages/EFCore.Migrations.AutoComments) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Reads XML docs and sets table/column comments in migrations.

### [EFCore.Migrations.CustomSql](#efcoremigrationscustomsql-1)
[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.CustomSql)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.CustomSql)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Tracks custom SQL in the EF model; auto-generates Up/Down migration code.

### [EFCore.Migrations.Triggers](#efcoremigrationstriggers-1)
[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers)](https://www.nuget.org/packages/EFCore.Migrations.Triggers) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers)](https://www.nuget.org/packages/EFCore.Migrations.Triggers) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Provider-agnostic trigger abstraction for EF migrations.

### [EFCore.Migrations.Triggers.PostgreSQL](#efcoremigrationstriggerspostgresql-1)
[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers.PostgreSQL)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.PostgreSQL) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers.PostgreSQL)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.PostgreSQL) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

PostgreSQL trigger provider (depends on CustomSql + Triggers).

### [EFCore.Migrations.Triggers.SqlServer](#efcoremigrationstriggerssqlserver-1)
[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers.SqlServer)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.SqlServer) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers.SqlServer)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.SqlServer) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

SQL Server trigger provider (depends on CustomSql + Triggers).

---

## EFCore.Migrations.AutoComments

[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.AutoComments)](https://www.nuget.org/packages/EFCore.Migrations.AutoComments) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.AutoComments)](https://www.nuget.org/packages/EFCore.Migrations.AutoComments) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Automatically applies database comments to tables and columns based on XML documentation. Comments flow from `<summary>` tags directly into migrations.

### Installation

```
dotnet add package EFCore.Migrations.AutoComments
```

### Prerequisites

Enable XML documentation generation in your project:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(...)
        .UseAutoComments());
```

### XML file path resolution

If no file is specified, XML docs are auto-discovered by assembly name.

```csharp
// Auto-discover (default)
options.UseAutoComments();

// Single file
options.UseAutoComments("MyApp.xml");

// Multiple files — merges docs from several assemblies
options.UseAutoComments("MyApp.xml", "SharedLibrary.xml");
```

### Example

```csharp
/// <summary>
/// Represents an animal in the shelter.
/// </summary>
public class Animal
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Animal name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of animal.
    /// </summary>
    public AnimalType Type { get; set; }
}

/// <summary>
/// Animal type.
/// </summary>
public enum AnimalType
{
    /// <summary>Dog.</summary>
    Dog,

    /// <summary>Cat.</summary>
    Cat,

    /// <summary>Fish.</summary>
    Fish
}
```

Generated migration comments:

```sql
COMMENT ON TABLE "Animals" IS 'Represents an animal in the shelter.';
COMMENT ON COLUMN "Animals"."Id" IS 'Unique identifier.';
COMMENT ON COLUMN "Animals"."Name" IS 'Animal name.';
COMMENT ON COLUMN "Animals"."Type" IS 'Type of animal.';
```

With `options.UseAutoComments(o => o.AddEnumDescriptions())`, the `Type` column comment becomes:

```
Type of animal.

0 - Dog.
1 - Cat.
2 - Fish.
```

For string-backed enums, the enum member name is used instead of the numeric value:

```
Status.

Active - Active account.
Suspended - Temporarily suspended.
Closed - Account closed.
```

Use `[AutoCommentEnumDescription]` on a property to enable enum descriptions for that property only, or `[IgnoreAutoCommentEnumDescription]` to exclude it when global mode is active.

---

## EFCore.Migrations.CustomSql

[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.CustomSql)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.CustomSql)](https://www.nuget.org/packages/EFCore.Migrations.CustomSql) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Tracks custom SQL scripts (views, functions, indexes, etc.) as part of the EF model. When you run `dotnet ef migrations add`, the library detects changes and automatically generates the correct `Up` / `Down` migration code.

### Installation

```
dotnet add package EFCore.Migrations.CustomSql
```

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(...)
        .UseCustomSql());
```

### How it works

Each custom SQL entry has:

- **Name** — unique identifier used by the migration differ to detect changes.
- **Up SQL** — SQL executed when migrating forward. Added at the **end** of the `Up` method, after schema changes.
- **Down SQL** — SQL executed when rolling back. Added at the **beginning** of the `Down` method, before schema rollback.

### Basic usage

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AnimalView>(entity =>
    {
        entity.HasNoKey();
        entity.ToView("animals_view");

        entity.AddCustomSql(
            name: "animals_view",
            upSql: "CREATE VIEW animals_view AS SELECT * FROM \"Animals\" WHERE \"AnimalType\" = 1",
            downSql: "DROP VIEW IF EXISTS animals_view");
    });
}
```

### Generated migration example

```csharp
public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // schema migrations first...
        migrationBuilder.CreateTable("Animals", ...);

        // custom SQL at the end
        migrationBuilder.Sql("CREATE VIEW animals_view AS SELECT * FROM \"Animals\" WHERE \"AnimalType\" = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // custom SQL rollback first
        migrationBuilder.Sql("DROP VIEW IF EXISTS animals_view");

        // schema rollback after
        migrationBuilder.DropTable("Animals");
    }
}
```

### Dynamic SQL with `CustomSqlGenerator`

`CustomSqlGenerator` provides helper methods to resolve actual table and column names from the EF model. When a property or entity is renamed in the model, the generated SQL stays in sync.

```csharp
public class MyCustomSqlGenerator : CustomSqlGenerator
{
    public MyCustomSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder)
        : base(dbContext, modelBuilder)
    {
    }

    public string Up()
    {
        var table = GetTableName<Animal>();
        var species = GetColumnName<Animal>(x => x.Species);
        var type = GetColumnName<Animal>(x => x.AnimalType);

        return $"CREATE VIEW animals_species_view AS SELECT {species}, {type} FROM {table}";
    }

    public string Down() => "DROP VIEW IF EXISTS animals_species_view";
}
```

Use in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var gen = new MyCustomSqlGenerator(this, modelBuilder);

    modelBuilder.AddCustomSql(
        name: "animals_species_view",
        upSql: gen.Up(),
        downSql: gen.Down());
}
```

---

## EFCore.Migrations.Triggers

[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers)](https://www.nuget.org/packages/EFCore.Migrations.Triggers) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers)](https://www.nuget.org/packages/EFCore.Migrations.Triggers) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

Provider-agnostic trigger abstraction built on top of `EFCore.Migrations.CustomSql`. Define triggers in `OnModelCreating` using typed extension methods — the concrete provider (PostgreSQL, SQL Server) generates the final SQL.

### Installation

Install the provider package — it pulls in this package automatically:
- [EFCore.Migrations.Triggers.PostgreSQL](#efcoremigrationstriggerspostgresql-1)
- [EFCore.Migrations.Triggers.SqlServer](#efcoremigrationstriggerssqlserver-1)

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(..., o => o.UseTriggers()));
```

### How it works

Call extension methods on the entity builder inside `OnModelCreating`. Each method accepts a unique trigger name and the trigger body.

### Usage

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Figure>(entity =>
    {
        entity.BeforeInsert(
            name: "set_square",
            body: "new.square = 0;");

        entity.BeforeUpdate(
            name: "prevent_negative_square",
            body: "IF new.square < 0 THEN RAISE EXCEPTION 'square must be non-negative'; END IF;");
    });
}
```

### Generated migration example

```csharp
public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Figures", ...);

        migrationBuilder.Sql("""
            CREATE FUNCTION set_square() RETURNS trigger AS $set_square$
            BEGIN
                new.square = 0;
                RETURN NEW;
            END;
            $set_square$ LANGUAGE plpgsql;

            CREATE TRIGGER set_square BEFORE INSERT
            ON "Figures"
            FOR EACH ROW EXECUTE PROCEDURE set_square();
            """);

        migrationBuilder.Sql("""
            CREATE FUNCTION prevent_negative_square() RETURNS trigger AS $prevent_negative_square$
            BEGIN
                IF new.square < 0 THEN RAISE EXCEPTION 'square must be non-negative'; END IF;
                RETURN NEW;
            END;
            $prevent_negative_square$ LANGUAGE plpgsql;

            CREATE TRIGGER prevent_negative_square BEFORE UPDATE
            ON "Figures"
            FOR EACH ROW EXECUTE PROCEDURE prevent_negative_square();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION prevent_negative_square() CASCADE;");
        migrationBuilder.Sql("DROP FUNCTION set_square() CASCADE;");

        migrationBuilder.DropTable("Figures");
    }
}
```

### Dynamic trigger bodies with `CustomSqlGenerator`

Extend `CustomSqlGenerator` to resolve table and column names from the EF model. If a property or entity is renamed, `dotnet ef migrations add` picks up the change and regenerates the trigger SQL automatically.

```csharp
public class TriggersGenerator : CustomSqlGenerator
{
    public TriggersGenerator(DbContext dbContext, ModelBuilder modelBuilder)
        : base(dbContext, modelBuilder)
    {
    }

    public string GenerateSyncScript()
    {
        var table = GetTableName<Animal>();
        var species = GetColumnName<Animal>(x => x.Species);
        var animalType = GetColumnName<Animal>(x => x.AnimalType);

        return $"""
            IF NEW.{species} IS NOT NULL AND NEW.{species} IS DISTINCT FROM OLD.{species} THEN
                RAISE EXCEPTION 'Species cannot be changed';
            END IF;
            IF NEW.{species} IS NOT NULL THEN
                UPDATE {table} SET {animalType} = NEW.{animalType} WHERE {species} = NEW.{species};
            END IF;
            """;
    }
}
```

```csharp
entity.BeforeInsertOrUpdate(
    name: "sync_animal_type",
    body: gen.GenerateSyncScript());
```

---

## EFCore.Migrations.Triggers.PostgreSQL

[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers.PostgreSQL)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.PostgreSQL) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers.PostgreSQL)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.PostgreSQL) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

PostgreSQL provider for `EFCore.Migrations.Triggers`.

### Installation

```
dotnet add package EFCore.Migrations.Triggers.PostgreSQL
```

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(..., o => o.UseTriggers()));
```

### Generated SQL

```sql
CREATE FUNCTION trigger_name() RETURNS trigger AS $trigger_name$
BEGIN
    -- your body here
    RETURN NEW;  -- or RETURN OLD; for DELETE triggers
END;
$trigger_name$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_name {BEFORE|AFTER|INSTEAD OF} {INSERT|UPDATE|DELETE|INSERT OR UPDATE}
ON "TableName"
FOR EACH ROW EXECUTE PROCEDURE trigger_name();
```

Rollback:

```sql
DROP FUNCTION trigger_name() CASCADE;
```

---

## EFCore.Migrations.Triggers.SqlServer

[![NuGet](https://img.shields.io/nuget/v/EFCore.Migrations.Triggers.SqlServer)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.SqlServer) [![Downloads](https://img.shields.io/nuget/dt/EFCore.Migrations.Triggers.SqlServer)](https://www.nuget.org/packages/EFCore.Migrations.Triggers.SqlServer) [![License](https://img.shields.io/github/license/AndreqGav/EFCore.Migrations.Toolkit)](LICENSE)

SQL Server provider for `EFCore.Migrations.Triggers`.

### Installation

```
dotnet add package EFCore.Migrations.Triggers.SqlServer
```

### Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseSqlServer(..., o => o.UseTriggers()));
```

### Generated SQL

```sql
CREATE OR ALTER TRIGGER [trigger_name]
ON [TableName]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- your body here
END;
```

Rollback:

```sql
DROP TRIGGER [trigger_name];
```

---

## License

MIT © Андрей Гаврилов 2026

---

> Migrated from [AndreqGav/EF.Toolkits](https://github.com/AndreqGav/EF.Toolkits).
