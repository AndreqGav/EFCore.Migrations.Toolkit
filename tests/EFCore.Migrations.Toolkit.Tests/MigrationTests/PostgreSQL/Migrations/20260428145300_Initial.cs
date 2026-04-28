using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "BlogBaseSequence");

            migrationBuilder.CreateTable(
                name: "ArticleBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleBase", x => x.Id);
                },
                comment: "Базовый тип в наследовании TPT.");

            migrationBuilder.CreateTable(
                name: "BlogA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BlogBaseSequence\"')", comment: "Идентификатор."),
                    Name = table.Column<string>(type: "text", nullable: true, comment: "Имя А.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogA", x => x.Id);
                },
                comment: "Наследник А в TPC.");

            migrationBuilder.CreateTable(
                name: "BlogB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"BlogBaseSequence\"')", comment: "Идентификатор."),
                    Name = table.Column<string>(type: "text", nullable: true, comment: "Имя Б.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogB", x => x.Id);
                },
                comment: "Наследник Б в TPC.");

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор блога.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true, comment: "Название блога."),
                    Url = table.Column<string>(type: "text", nullable: true, comment: "URL блога.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                },
                comment: "Блог.");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор заказа.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: true, comment: "Номер заказа."),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false, comment: "Итоговая сумма заказа в рублях."),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false, comment: "Статус подтверждения заказа."),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Статус заказа.\n\n0 - Активный, ожидает выполнения.\n1 - Выполнен, доставлен покупателю.\n2 - Отменён, возврат средств."),
                    Category = table.Column<int>(type: "integer", nullable: false, comment: "Категория заказа.\n\n0 - Одежда.\n1 - Книги.\n2 - Игрушки."),
                    DeliveryMethod = table.Column<int>(type: "integer", nullable: false, comment: "Способ доставки.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                },
                comment: "Заказ покупателя.");

            migrationBuilder.CreateTable(
                name: "PostBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TextA = table.Column<string>(type: "text", nullable: true, comment: "Текст А."),
                    TextB = table.Column<string>(type: "text", nullable: true, comment: "Текст Б.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostBase", x => x.Id);
                },
                comment: "Базовый тип в наследовании TPH.");

            migrationBuilder.CreateTable(
                name: "ArticleA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор."),
                    ContentA = table.Column<string>(type: "text", nullable: true, comment: "Специфичное содержимое А.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleA_ArticleBase_Id",
                        column: x => x.Id,
                        principalTable: "ArticleBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Наследник А в TPT.");

            migrationBuilder.CreateTable(
                name: "ArticleB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор."),
                    ContentB = table.Column<string>(type: "text", nullable: true, comment: "Специфичное содержимое Б.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleB_ArticleBase_Id",
                        column: x => x.Id,
                        principalTable: "ArticleBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Наследник Б в TPT.");

            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.blog_names\nAS SELECT \"Id\", \"Name\" FROM \"Blogs\"");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION GetName(id integer)\nRETURNS text AS $$\nBEGIN\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n END;\n$$ LANGUAGE plpgsql;");

            migrationBuilder.Sql("CREATE FUNCTION before_insert_or_update_blog() RETURNS trigger as $before_insert_or_update_blog$\nBEGIN\nIF NEW.\"Url\" IS NOT NULL AND NEW.\"Url\" IS DISTINCT FROM OLD.\"Url\" THEN\n    RAISE EXCEPTION 'Нельзя менять URL';\nEND IF;\nIF NEW.\"Name\" IS NOT NULL THEN\n    UPDATE \"Blogs\" SET \"Url\" = NEW.\"Url\"\n    WHERE \"Name\" = NEW.\"Name\";\nEND IF;\nRETURN NEW;\nEND;\n$before_insert_or_update_blog$ LANGUAGE plpgsql;\n\nCREATE TRIGGER before_insert_or_update_blog BEFORE INSERT OR UPDATE\nON \"Blogs\"\nFOR EACH ROW EXECUTE PROCEDURE before_insert_or_update_blog();");

            migrationBuilder.Sql("CREATE VIEW blog_view AS SELECT * FROM \"Blogs\"");

            migrationBuilder.Sql("CREATE FUNCTION prevent_update_negative_amount() RETURNS trigger as $prevent_update_negative_amount$\nBEGIN\nIF NEW.total_amount < 0 THEN RAISE EXCEPTION 'amount negative'; END IF;\nRETURN NEW;\nEND;\n$prevent_update_negative_amount$ LANGUAGE plpgsql;\n\nCREATE TRIGGER prevent_update_negative_amount BEFORE UPDATE\nON \"Orders\"\nFOR EACH ROW EXECUTE PROCEDURE prevent_update_negative_amount();");

            migrationBuilder.Sql("CREATE FUNCTION set_order_defaults() RETURNS trigger as $set_order_defaults$\nBEGIN\nNEW.is_confirmed = false;\nRETURN NEW;\nEND;\n$set_order_defaults$ LANGUAGE plpgsql;\n\nCREATE TRIGGER set_order_defaults BEFORE INSERT\nON \"Orders\"\nFOR EACH ROW EXECUTE PROCEDURE set_order_defaults();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.blog_names");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS GetName");

            migrationBuilder.Sql("DROP FUNCTION before_insert_or_update_blog() CASCADE;");

            migrationBuilder.Sql("DROP VIEW IF EXISTS blog_view");

            migrationBuilder.Sql("DROP FUNCTION prevent_update_negative_amount() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION set_order_defaults() CASCADE;");

            migrationBuilder.DropTable(
                name: "ArticleA");

            migrationBuilder.DropTable(
                name: "ArticleB");

            migrationBuilder.DropTable(
                name: "BlogA");

            migrationBuilder.DropTable(
                name: "BlogB");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PostBase");

            migrationBuilder.DropTable(
                name: "ArticleBase");

            migrationBuilder.DropSequence(
                name: "BlogBaseSequence");
        }
    }
}
