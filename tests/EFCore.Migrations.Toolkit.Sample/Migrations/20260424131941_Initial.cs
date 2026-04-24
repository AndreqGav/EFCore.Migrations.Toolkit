using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EFCore.Migrations.Toolkit.Sample.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор автора.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Имя автора."),
                    Email = table.Column<string>(type: "text", nullable: false, comment: "Email автора.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                },
                comment: "Автор публикаций.");

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор публикации.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false, comment: "Заголовок публикации."),
                    Content = table.Column<string>(type: "text", nullable: false, comment: "Содержимое публикации."),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Статус публикации.\n\n0 - Черновик, не опубликован.\n1 - Опубликован и доступен читателям.\n2 - Архивирован, скрыт от читателей."),
                    AuthorId = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор автора.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Публикация в блоге.");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");

            migrationBuilder.Sql("CREATE OR REPLACE FUNCTION get_author_name(author_id integer)\nRETURNS text\nLANGUAGE sql STABLE AS $$\n    SELECT \"Name\" FROM \"Authors\" WHERE \"Id\" = author_id;\n$$");

            migrationBuilder.Sql("CREATE OR REPLACE VIEW published_posts_view AS\nSELECT p.\"Id\", p.\"Title\", p.\"AuthorId\"\nFROM \"Posts\" p\nWHERE p.\"Status\" = 1");

            migrationBuilder.Sql("CREATE FUNCTION audit_post_insert() RETURNS trigger as $audit_post_insert$\r\nBEGIN\r\nRAISE NOTICE 'Post inserted: %', NEW.\"Id\";\r\nRETURN NEW;\r\nEND;\r\n$audit_post_insert$ LANGUAGE plpgsql;\r\n\r\nCREATE CONSTRAINT TRIGGER audit_post_insert AFTER INSERT\r\nON \"Posts\"\r\nDEFERRABLE INITIALLY DEFERRED\r\nFOR EACH ROW EXECUTE PROCEDURE audit_post_insert();\r\n");

            migrationBuilder.Sql("CREATE FUNCTION prevent_direct_archive() RETURNS trigger as $prevent_direct_archive$\r\nBEGIN\r\nIF NEW.\"Status\" = 2 AND OLD.\"Status\" = 1 THEN\n    RAISE EXCEPTION 'Use dedicated archive procedure instead of direct UPDATE';\nEND IF;\r\nRETURN NEW;\r\nEND;\r\n$prevent_direct_archive$ LANGUAGE plpgsql;\r\n\r\nCREATE TRIGGER prevent_direct_archive BEFORE UPDATE\r\nON \"Posts\"\r\nFOR EACH ROW EXECUTE PROCEDURE prevent_direct_archive();\r\n");

            migrationBuilder.Sql("CREATE FUNCTION set_post_status_on_insert() RETURNS trigger as $set_post_status_on_insert$\r\nBEGIN\r\nNEW.\"Status\" = 0;\r\nRETURN NEW;\r\nEND;\r\n$set_post_status_on_insert$ LANGUAGE plpgsql;\r\n\r\nCREATE TRIGGER set_post_status_on_insert BEFORE INSERT\r\nON \"Posts\"\r\nFOR EACH ROW EXECUTE PROCEDURE set_post_status_on_insert();\r\n");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_author_name(integer)");

            migrationBuilder.Sql("DROP VIEW IF EXISTS published_posts_view");

            migrationBuilder.Sql("DROP FUNCTION audit_post_insert() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION prevent_direct_archive() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION set_post_status_on_insert() CASCADE;");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
