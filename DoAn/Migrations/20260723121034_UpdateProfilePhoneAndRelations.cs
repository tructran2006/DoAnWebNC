using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    public partial class UpdateProfilePhoneAndRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cột Phone đã tồn tại trong database.
            // Không cần thêm lại.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không xóa Phone vì migration này không tạo cột đó.
        }
    }
}