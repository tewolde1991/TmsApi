using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentsAndCertificates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Only two NEW tables — existing three are untouched
            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                                     .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(nullable: false),   // ✅ FK → Courses
                    StudentId = table.Column<int>(nullable: false),   // ✅ FK → Students
                    Score = table.Column<decimal>(nullable: false),
                    Grade = table.Column<string>(nullable: false),
                    TakenAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(                                  // ✅ Points to Courses
                        name: "FK_Assessments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(                                  // ✅ Points to Students
                        name: "FK_Assessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                                     .Annotation("Npgsql:ValueGenerationStrategy",
                                                  NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(nullable: false),   // ✅ FK → Students
                    CourseId = table.Column<int>(nullable: false),   // ✅ FK → Courses
                    IssuedAt = table.Column<DateTime>(nullable: false),
                    IssuedBy = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(                                  // ✅ Points to Students
                        name: "FK_Certificates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(                                  // ✅ Points to Courses
                        name: "FK_Certificates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        // ✅ Down() drops new tables first — dependents before parents
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Assessments");
            migrationBuilder.DropTable(name: "Certificates");
            // Courses, Students, Enrollments are untouched
        }
    }
}