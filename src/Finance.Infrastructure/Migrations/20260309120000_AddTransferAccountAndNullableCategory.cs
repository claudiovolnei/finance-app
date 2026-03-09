using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Migrations
{
    public partial class AddTransferAccountAndNullableCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TransferAccountId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferAccountId",
                table: "Transactions",
                column: "TransferAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_TransferAccountId",
                table: "Transactions",
                column: "TransferAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_TransferAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransferAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransferAccountId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
