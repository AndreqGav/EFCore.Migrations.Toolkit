using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    TextA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextB = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostBase", x => x.Id);
                });

            migrationBuilder.Sql("CREATE OR ALTER PROCEDURE [get_blog_name] @id INT AS SELECT [Name] FROM [Blogs] WHERE [Id] = @id");

            migrationBuilder.Sql("CREATE OR ALTER TRIGGER [trg_blog_log_changes]\nON [Blogs]\nAFTER INSERT, UPDATE\nAS\nBEGIN\n    SET NOCOUNT ON;\n-- log blog insert or update\nEND;");

            migrationBuilder.Sql("CREATE VIEW blog_view AS SELECT * FROM [Blogs]");

            migrationBuilder.Sql("CREATE OR ALTER TRIGGER [trg_order_prevent_negative_amount]\nON [Orders]\nAFTER UPDATE\nAS\nBEGIN\n    SET NOCOUNT ON;\nIF EXISTS (SELECT 1 FROM inserted WHERE [TotalAmount] < 0)\n    THROW 50001, 'Amount must not be negative', 1;\nEND;");

            migrationBuilder.Sql("CREATE OR ALTER TRIGGER [trg_order_set_confirmed]\nON [Orders]\nAFTER INSERT\nAS\nBEGIN\n    SET NOCOUNT ON;\nUPDATE [Orders] SET [IsConfirmed] = 0 WHERE [Id] IN (SELECT [Id] FROM inserted)\nEND;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [get_blog_name]");

            migrationBuilder.Sql("DROP TRIGGER [trg_blog_log_changes];");

            migrationBuilder.Sql("DROP VIEW IF EXISTS blog_view");

            migrationBuilder.Sql("DROP TRIGGER [trg_order_prevent_negative_amount];");

            migrationBuilder.Sql("DROP TRIGGER [trg_order_set_confirmed];");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PostBase");
        }
    }
}
