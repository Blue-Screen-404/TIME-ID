using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TimeId.Operations;

#nullable disable

namespace TIME_ID_OFC.Migrations;

[DbContext(typeof(TimeIdDbContext))]
[Migration("20261117210000_AlignCompleteSqliteSchema")]
public sealed class AlignCompleteSqliteSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent so SQLite databases created by main with EnsureCreated can be adopted.
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "CompanySettings" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_CompanySettings" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Cnpj" TEXT NOT NULL,
                "Email" TEXT NOT NULL,
                "Phone" TEXT NOT NULL,
                "Address" TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS "IX_Accounts_Email" ON "Accounts" ("Email");
            CREATE INDEX IF NOT EXISTS "IX_Employees_DepartmentId" ON "Employees" ("DepartmentId");
            CREATE INDEX IF NOT EXISTS "IX_Employees_LeaderId" ON "Employees" ("LeaderId");
            CREATE INDEX IF NOT EXISTS "IX_Punches_EmployeeId_At" ON "Punches" ("EmployeeId", "At");
            CREATE INDEX IF NOT EXISTS "IX_CameraAlerts_At" ON "CameraAlerts" ("At");
            CREATE INDEX IF NOT EXISTS "IX_Vacations_EmployeeId" ON "Vacations" ("EmployeeId");
            CREATE INDEX IF NOT EXISTS "IX_Audit_At" ON "Audit" ("At");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS "IX_Accounts_Email";
            DROP INDEX IF EXISTS "IX_Employees_DepartmentId";
            DROP INDEX IF EXISTS "IX_Employees_LeaderId";
            DROP INDEX IF EXISTS "IX_Punches_EmployeeId_At";
            DROP INDEX IF EXISTS "IX_CameraAlerts_At";
            DROP INDEX IF EXISTS "IX_Vacations_EmployeeId";
            DROP INDEX IF EXISTS "IX_Audit_At";
            DROP TABLE IF EXISTS "CompanySettings";
            """);
    }
}
