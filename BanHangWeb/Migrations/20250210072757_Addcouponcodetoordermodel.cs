using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHangWeb.Migrations
{
    public partial class Addcouponcodetoordermodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

           
        }
    }
}
