using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ADMS.API.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatterActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatterActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatterDocumentActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatterDocumentActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevisionActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCheckedOut = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Matters_MatterId",
                        column: x => x.MatterId,
                        principalTable: "Matters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatterActivityUsers",
                columns: table => new
                {
                    MatterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatterActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatterActivityUsers", x => new { x.MatterId, x.MatterActivityId, x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_MatterActivityUsers_MatterActivities_MatterActivityId",
                        column: x => x.MatterActivityId,
                        principalTable: "MatterActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatterActivityUsers_Matters_MatterId",
                        column: x => x.MatterId,
                        principalTable: "Matters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatterActivityUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentActivityUsers",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentActivityUsers", x => new { x.DocumentId, x.DocumentActivityId, x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_DocumentActivityUsers_DocumentActivities_DocumentActivityId",
                        column: x => x.DocumentActivityId,
                        principalTable: "DocumentActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentActivityUsers_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentActivityUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatterDocumentActivityUsersFrom",
                columns: table => new
                {
                    MatterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatterDocumentActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatterDocumentActivityUsersFrom", x => new { x.MatterId, x.DocumentId, x.MatterDocumentActivityId, x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersFrom_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersFrom_MatterDocumentActivities_MatterDocumentActivityId",
                        column: x => x.MatterDocumentActivityId,
                        principalTable: "MatterDocumentActivities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersFrom_Matters_MatterId",
                        column: x => x.MatterId,
                        principalTable: "Matters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersFrom_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MatterDocumentActivityUsersTo",
                columns: table => new
                {
                    MatterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatterDocumentActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatterDocumentActivityUsersTo", x => new { x.MatterId, x.DocumentId, x.MatterDocumentActivityId, x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersTo_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersTo_MatterDocumentActivities_MatterDocumentActivityId",
                        column: x => x.MatterDocumentActivityId,
                        principalTable: "MatterDocumentActivities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersTo_Matters_MatterId",
                        column: x => x.MatterId,
                        principalTable: "Matters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatterDocumentActivityUsersTo_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revisions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionActivityUsers",
                columns: table => new
                {
                    RevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevisionActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionActivityUsers", x => new { x.RevisionId, x.RevisionActivityId, x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_RevisionActivityUsers_RevisionActivities_RevisionActivityId",
                        column: x => x.RevisionActivityId,
                        principalTable: "RevisionActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionActivityUsers_Revisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionActivityUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DocumentActivities",
                columns: new[] { "Id", "Activity" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "CHECKED IN" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "CHECKED OUT" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "CREATED" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "DELETED" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "RESTORED" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "SAVED" }
                });

            migrationBuilder.InsertData(
                table: "MatterActivities",
                columns: new[] { "Id", "Activity" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "ARCHIVED" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "CREATED" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "DELETED" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "RESTORED" },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "UNARCHIVED" },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "VIEWED" }
                });

            migrationBuilder.InsertData(
                table: "MatterDocumentActivities",
                columns: new[] { "Id", "Activity" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "COPIED" },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "MOVED" }
                });

            migrationBuilder.InsertData(
                table: "Matters",
                columns: new[] { "Id", "CreationDate", "Description", "IsArchived", "IsDeleted" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #1", false, false },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #2", false, false },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #3", true, false },
                    { new Guid("60000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #4", false, true },
                    { new Guid("60000000-0000-0000-0000-000000000005"), new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #5", true, true },
                    { new Guid("60000000-0000-0000-0000-000000000006"), new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Test Matter #6", true, true }
                });

            migrationBuilder.InsertData(
                table: "RevisionActivities",
                columns: new[] { "Id", "Activity" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "CREATED" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "DELETED" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "RESTORED" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "SAVED" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("50000000-0000-0000-0000-000000000001"), "rbrown" });

            migrationBuilder.InsertData(
                table: "MatterActivityUsers",
                columns: new[] { "CreatedAt", "MatterActivityId", "MatterId", "UserId" },
                values: new object[,]
                {
                    { new DateTime(2024, 1, 10, 0, 1, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000001"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 2, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000002"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 7, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000001"), new Guid("60000000-0000-0000-0000-000000000003"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 3, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000003"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 4, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000004"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 10, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000003"), new Guid("60000000-0000-0000-0000-000000000004"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 8, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000001"), new Guid("60000000-0000-0000-0000-000000000005"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 5, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000005"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 11, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000003"), new Guid("60000000-0000-0000-0000-000000000005"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 9, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000001"), new Guid("60000000-0000-0000-0000-000000000006"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 6, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), new Guid("60000000-0000-0000-0000-000000000006"), new Guid("50000000-0000-0000-0000-000000000001") },
                    { new DateTime(2024, 1, 10, 0, 12, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000003"), new Guid("60000000-0000-0000-0000-000000000006"), new Guid("50000000-0000-0000-0000-000000000001") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentActivityUsers_DocumentActivityId",
                table: "DocumentActivityUsers",
                column: "DocumentActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentActivityUsers_UserId",
                table: "DocumentActivityUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_MatterId",
                table: "Documents",
                column: "MatterId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterActivityUsers_MatterActivityId",
                table: "MatterActivityUsers",
                column: "MatterActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterActivityUsers_UserId",
                table: "MatterActivityUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersFrom_DocumentId",
                table: "MatterDocumentActivityUsersFrom",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersFrom_MatterDocumentActivityId",
                table: "MatterDocumentActivityUsersFrom",
                column: "MatterDocumentActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersFrom_UserId",
                table: "MatterDocumentActivityUsersFrom",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersTo_DocumentId",
                table: "MatterDocumentActivityUsersTo",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersTo_MatterDocumentActivityId",
                table: "MatterDocumentActivityUsersTo",
                column: "MatterDocumentActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_MatterDocumentActivityUsersTo_UserId",
                table: "MatterDocumentActivityUsersTo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivityUsers_RevisionActivityId",
                table: "RevisionActivityUsers",
                column: "RevisionActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivityUsers_UserId",
                table: "RevisionActivityUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Revisions_DocumentId",
                table: "Revisions",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentActivityUsers");

            migrationBuilder.DropTable(
                name: "MatterActivityUsers");

            migrationBuilder.DropTable(
                name: "MatterDocumentActivityUsersFrom");

            migrationBuilder.DropTable(
                name: "MatterDocumentActivityUsersTo");

            migrationBuilder.DropTable(
                name: "RevisionActivityUsers");

            migrationBuilder.DropTable(
                name: "DocumentActivities");

            migrationBuilder.DropTable(
                name: "MatterActivities");

            migrationBuilder.DropTable(
                name: "MatterDocumentActivities");

            migrationBuilder.DropTable(
                name: "RevisionActivities");

            migrationBuilder.DropTable(
                name: "Revisions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Matters");
        }
    }
}
