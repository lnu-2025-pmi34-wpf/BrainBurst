using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrainBurst.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardTagsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashcardTag",
                columns: table => new
                {
                    FlashcardsFlashcardId = table.Column<int>(type: "integer", nullable: false),
                    TagsTagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardTag", x => new { x.FlashcardsFlashcardId, x.TagsTagId });
                    table.ForeignKey(
                        name: "FK_FlashcardTag_Flashcards_FlashcardsFlashcardId",
                        column: x => x.FlashcardsFlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "FlashcardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardTag_Tags_TagsTagId",
                        column: x => x.TagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardTag_TagsTagId",
                table: "FlashcardTag",
                column: "TagsTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardTag");
        }
    }
}
