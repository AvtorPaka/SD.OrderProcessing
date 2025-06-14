using FluentMigrator;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250619, TransactionBehavior.Default)]
public class AddPaymentStatusMessageType: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'payment_status_message_type') THEN
            CREATE TYPE payment_status_message_type as
            (
                id              bigint,
                order_id        bigint,
                created_at      timestamp with time zone,
                order_status    order_status_enum,
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
        DROP TYPE IF EXISTS payment_status_message_type;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}