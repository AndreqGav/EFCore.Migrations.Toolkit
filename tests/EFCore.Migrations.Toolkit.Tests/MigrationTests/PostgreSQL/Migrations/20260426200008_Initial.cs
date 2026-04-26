using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    TextA = table.Column<string>(type: "text", nullable: true, comment: "Текст А."),
                    TextB = table.Column<string>(type: "text", nullable: true, comment: "Текст Б.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostBase", x => x.Id);
                },
                comment: "Базовый тип в наследовании TPH.");

            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.blog_names\nAS SELECT \"Id\", \"Name\" FROM \"Blogs\"");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION GetName(id integer)\nRETURNS text AS $$\nBEGIN\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n END;\n$$ LANGUAGE plpgsql;");

            migrationBuilder.Sql("CREATE FUNCTION before_insert_or_update_blog() RETURNS trigger as $before_insert_or_update_blog$\nBEGIN\nIF NEW.\"Url\" IS NOT NULL AND NEW.\"Url\" IS DISTINCT FROM OLD.\"Url\" THEN\n    RAISE EXCEPTION 'Нельзя менять URL';\nEND IF;\nIF NEW.\"Name\" IS NOT NULL THEN\n    UPDATE \"Blogs\" SET \"Url\" = NEW.\"Url\"\n    WHERE \"Name\" = NEW.\"Name\";\nEND IF;\nRETURN NEW;\nEND;\n$before_insert_or_update_blog$ LANGUAGE plpgsql;\n\nCREATE TRIGGER before_insert_or_update_blog BEFORE INSERT OR UPDATE\nON \"Blogs\"\nFOR EACH ROW EXECUTE PROCEDURE before_insert_or_update_blog();");

            migrationBuilder.Sql("CREATE VIEW blog_view AS SELECT * FROM \"Blogs\"");

            migrationBuilder.Sql("CREATE FUNCTION prevent_update_negative_amount() RETURNS trigger as $prevent_update_negative_amount$\nBEGIN\nIF NEW.total_amount < 0 THEN RAISE EXCEPTION 'amount negative'; END IF;\nRETURN NEW;\nEND;\n$prevent_update_negative_amount$ LANGUAGE plpgsql;\n\nCREATE TRIGGER prevent_update_negative_amount BEFORE UPDATE\nON \"Orders\"\nFOR EACH ROW EXECUTE PROCEDURE prevent_update_negative_amount();");

            migrationBuilder.Sql("CREATE FUNCTION set_order_defaults() RETURNS trigger as $set_order_defaults$\nBEGIN\nNEW.is_confirmed = false;\nRETURN NEW;\nEND;\n$set_order_defaults$ LANGUAGE plpgsql;\n\nCREATE TRIGGER set_order_defaults BEFORE INSERT\nON \"Orders\"\nFOR EACH ROW EXECUTE PROCEDURE set_order_defaults();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.blog_names");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS GetName");

            migrationBuilder.Sql("DROP FUNCTION before_insert_or_update_blog() CASCADE;");

            migrationBuilder.Sql("DROP VIEW IF EXISTS blog_view");

            migrationBuilder.Sql("DROP FUNCTION prevent_update_negative_amount() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION set_order_defaults() CASCADE;");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PostBase");
        }
    }
}
