using FluentMigrator;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Migrations;

[Migration(version:20250617, TransactionBehavior.Default)]
public class AddOrderPaymentMessageType: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_payment_message_type') THEN
            CREATE TYPE order_payment_message_type as
            (
                id              bigint,
                order_id        bigint,
                user_id         bigint,
                created_at      timestamp with time zone,
                amount          numeric(21,5),
                state           message_state_enum
            );
        END IF;
    END 
$$;
";
        
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO
    $$ BEGIN
        DROP TYPE IF EXISTS order_payment_message_type;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}