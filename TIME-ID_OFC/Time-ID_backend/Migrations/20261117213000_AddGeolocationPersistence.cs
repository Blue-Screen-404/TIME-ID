using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TimeId.Operations;

#nullable disable

namespace TIME_ID_OFC.Migrations;

[DbContext(typeof(TimeIdDbContext))]
[Migration("20261117213000_AddGeolocationPersistence")]
public sealed class AddGeolocationPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Geolocation",
            table: "Punches",
            type: "TEXT",
            nullable: true);

        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "GeolocationSettings" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_GeolocationSettings" PRIMARY KEY AUTOINCREMENT,
                "MaxCaptureAgeSeconds" INTEGER NOT NULL,
                "FutureToleranceSeconds" INTEGER NOT NULL,
                "RequireJustification" INTEGER NOT NULL,
                "ManagersCanManageLocations" INTEGER NOT NULL,
                "ManagersCanReview" INTEGER NOT NULL,
                "RetentionDays" INTEGER NULL,
                "ReverseGeocodingEnabled" INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS "WorkLocations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkLocations" PRIMARY KEY,
                "Name" TEXT NOT NULL,
                "Latitude" REAL NOT NULL,
                "Longitude" REAL NOT NULL,
                "RadiusMeters" REAL NOT NULL,
                "CompanyWide" INTEGER NOT NULL,
                "DepartmentIds" TEXT NOT NULL,
                "EmployeeIds" TEXT NOT NULL,
                "Active" INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS "EmployeeLiveLocations" (
                "EmployeeId" TEXT NOT NULL CONSTRAINT "PK_EmployeeLiveLocations" PRIMARY KEY,
                "Latitude" REAL NOT NULL,
                "Longitude" REAL NOT NULL,
                "AccuracyMeters" REAL NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Geolocation", table: "Punches");
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS "EmployeeLiveLocations";
            DROP TABLE IF EXISTS "WorkLocations";
            DROP TABLE IF EXISTS "GeolocationSettings";
            """);
    }
}
