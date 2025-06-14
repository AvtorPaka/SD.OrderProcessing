using FluentMigrator;
using FluentMigrator.Postgres;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250617, TransactionBehavior.Default)]
public class AddTransactionalMessagesScheme: Migration 
{
    public override void Up()
    {
        Create.Table("balance_withdraw_updates")
            .WithColumn("id").AsInt64().PrimaryKey("balance_withdraw_updates_pk").Identity()
            .WithColumn("order_id").AsInt64().NotNullable()
            .WithColumn("user_id").AsInt64().NotNullable()
            .WithColumn("amount").AsDecimal(21, 5).NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("state").AsCustom("message_state_enum").WithDefaultValue("pending").NotNullable();


        Create.Index("balance_withdraw_updates_order_id_uidx")
            .OnTable("balance_withdraw_updates")
            .OnColumn("order_id")
            .Unique();
        
        Create.Table("payment_status_messages")
            .WithColumn("id").AsInt64().PrimaryKey("payment_status_messages_pk").Identity()
            .WithColumn("order_id").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("order_status").AsCustom("order_status_enum").NotNullable()
            .WithColumn("state").AsCustom("message_state_enum").WithDefaultValue("pending").NotNullable();

    }

    public override void Down()
    {
        Delete.Table("balance_withdraw_updates");
        Delete.Table("payment_status_messages");
    }
}