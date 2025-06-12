using System.Data;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Migrations;

[Migration(version:20250615, TransactionBehavior.Default)]
public class InitSchema: Migration 
{
    public override void Up()
    {
        Create.Table("user_orders")
            .WithColumn("id").AsInt64().PrimaryKey("user_orders_pk").Identity()
            .WithColumn("user_id").AsInt64().NotNullable()
            .WithColumn("amount").AsDecimal(21, 5).NotNullable()
            .WithColumn("description").AsString(256).NotNullable()
            .WithColumn("status").AsCustom("order_status_enum").WithDefaultValue("pending").NotNullable();

        Create.Table("order_payment_messages")
            .WithColumn("id").AsInt64().PrimaryKey("order_payment_messages_pk").Identity()
            .WithColumn("order_id").AsInt64().NotNullable().Unique()
            .WithColumn("user_id").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("amount").AsDecimal(21, 5).NotNullable()
            .WithColumn("state").AsCustom("message_state_enum").WithDefaultValue("pending").NotNullable();

        Create.ForeignKey("FK_order_payment_messages")
            .FromTable("order_payment_messages").ForeignColumn("order_id")
            .ToTable("user_orders").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("order_payment_messages");
        Delete.Table("user_orders");
    }
}