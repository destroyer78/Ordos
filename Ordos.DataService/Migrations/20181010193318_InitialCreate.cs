using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordos.DataService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationValues",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceType = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    IPAddress = table.Column<string>(nullable: false),
                    Station = table.Column<string>(nullable: false),
                    Bay = table.Column<string>(nullable: false),
                    IsConnected = table.Column<bool>(nullable: false),
                    HasPing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisturbanceRecordings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    TriggerTime = table.Column<DateTime>(nullable: false),
                    TriggerLength = table.Column<double>(nullable: false),
                    TriggerChannel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisturbanceRecordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisturbanceRecordings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DRFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisturbanceRecordingId = table.Column<int>(nullable: false),
                    FileData = table.Column<byte[]>(nullable: true),
                    FileName = table.Column<string>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DRFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DRFiles_DisturbanceRecordings_DisturbanceRecordingId",
                        column: x => x.DisturbanceRecordingId,
                        principalTable: "DisturbanceRecordings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisturbanceRecordings_DeviceId",
                table: "DisturbanceRecordings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DRFiles_DisturbanceRecordingId",
                table: "DRFiles",
                column: "DisturbanceRecordingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "DRFiles");

            migrationBuilder.DropTable(
                name: "DisturbanceRecordings");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
