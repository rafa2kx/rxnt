using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RXNT.API.Data;

#nullable disable

namespace RXNT.API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251030190000_CreateBulkUpdateAppointmentStatus")]
    public partial class CreateBulkUpdateAppointmentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"DECLARE @sql NVARCHAR(MAX) = N'
CREATE OR ALTER PROC dbo.BulkUpdateAppointmentStatus
    @Status NVARCHAR(32),
    @UpdatedDate DATETIME2,
    @Ids NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;	
    DROP TABLE IF EXISTS #idList
	CREATE TABLE #idList ([Id] INT NOT NULL)
	INSERT INTO #idList
	SELECT CAST([VALUE] AS INT) WsId
	FROM  STRING_SPLIT(@Ids, '','')
    UPDATE a
    SET a.Status = @Status,
        a.UpdatedDate = @UpdatedDate
    FROM dbo.Appointments AS a
    INNER JOIN #idList AS i ON a.Id = i.Id;
END';
EXEC sys.sp_executesql @sql;", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('dbo.BulkUpdateAppointmentStatus', 'P') IS NOT NULL DROP PROC dbo.BulkUpdateAppointmentStatus;");
        }
    }
}
