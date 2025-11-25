using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrainBurst.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardTagsRelation1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashcardTag_Flashcards_FlashcardsFlashcardId",
                table: "FlashcardTag");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashcardTag_Tags_TagsTagId",
                table: "FlashcardTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FlashcardTag",
                table: "FlashcardTag");

            migrationBuilder.RenameTable(
                name: "FlashcardTag",
                newName: "FlashcardTags");

            migrationBuilder.RenameIndex(
                name: "IX_FlashcardTag_TagsTagId",
                table: "FlashcardTags",
                newName: "IX_FlashcardTags_TagsTagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FlashcardTags",
                table: "FlashcardTags",
                columns: new[] { "FlashcardsFlashcardId", "TagsTagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FlashcardTags_Flashcards_FlashcardsFlashcardId",
                table: "FlashcardTags",
                column: "FlashcardsFlashcardId",
                principalTable: "Flashcards",
                principalColumn: "FlashcardId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashcardTags_Tags_TagsTagId",
                table: "FlashcardTags",
                column: "TagsTagId",
                principalTable: "Tags",
                principalColumn: "TagId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashcardTags_Flashcards_FlashcardsFlashcardId",
                table: "FlashcardTags");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashcardTags_Tags_TagsTagId",
                table: "FlashcardTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FlashcardTags",
                table: "FlashcardTags");

            migrationBuilder.RenameTable(
                name: "FlashcardTags",
                newName: "FlashcardTag");

            migrationBuilder.RenameIndex(
                name: "IX_FlashcardTags_TagsTagId",
                table: "FlashcardTag",
                newName: "IX_FlashcardTag_TagsTagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FlashcardTag",
                table: "FlashcardTag",
                columns: new[] { "FlashcardsFlashcardId", "TagsTagId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FlashcardTag_Flashcards_FlashcardsFlashcardId",
                table: "FlashcardTag",
                column: "FlashcardsFlashcardId",
                principalTable: "Flashcards",
                principalColumn: "FlashcardId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashcardTag_Tags_TagsTagId",
                table: "FlashcardTag",
                column: "TagsTagId",
                principalTable: "Tags",
                principalColumn: "TagId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
