using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alphabet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchedulerJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulerJobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggeredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    RetryParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJobExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerJobHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PerformedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJobHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    ScheduleType = table.Column<int>(type: "int", nullable: false),
                    ScheduleExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    RunAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JobConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetryPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPaused = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchedulerJobId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExclusionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependenciesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastExecutedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastExecutionStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJobExecutions_JobId",
                table: "SchedulerJobExecutions",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJobExecutions_StartedAt",
                table: "SchedulerJobExecutions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJobExecutions_Status",
                table: "SchedulerJobExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJobHistory_JobId",
                table: "SchedulerJobHistory",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJobHistory_PerformedAt",
                table: "SchedulerJobHistory",
                column: "PerformedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerJobExecutions");

            migrationBuilder.DropTable(
                name: "SchedulerJobHistory");

            migrationBuilder.DropTable(
                name: "SchedulerJobs");
        }
    }
}
