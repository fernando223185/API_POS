using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class RefreshCashierShiftHtmlTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ReportTemplates
SET HtmlTemplate = NULL,
    SectionsJson = '[]',
    UpdatedAt = GETUTCDATE()
WHERE ReportType = 'CashierShift'
  AND IsDefault = 1
  AND IsActive = 1;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ReportTemplates
SET UpdatedAt = GETUTCDATE()
WHERE ReportType = 'CashierShift'
  AND IsDefault = 1
  AND IsActive = 1;
");
        }
    }
}
