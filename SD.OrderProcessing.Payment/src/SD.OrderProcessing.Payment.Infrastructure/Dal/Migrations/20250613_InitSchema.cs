using FluentMigrator;
using FluentMigrator.Postgres;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250613, TransactionBehavior.Default)]
public class InitSchema: Migration
{
    public override void Up()
    {
        Create.Table("balance_accounts")
            .WithColumn("id").AsInt64().PrimaryKey("balance_accounts_pk").Identity()
            .WithColumn("user_id").AsInt64().NotNullable()
            .WithColumn("balance").AsDecimal(21, 5).NotNullable()
            .WithColumn("version").AsInt64().NotNullable();

        Create.Index("balance_accounts_user_id_uidx")
            .OnTable("balance_accounts")
            .OnColumn("user_id")
            .Unique();
    }

    public override void Down()
    {
        Delete.Table("balance_accounts");
    }
}