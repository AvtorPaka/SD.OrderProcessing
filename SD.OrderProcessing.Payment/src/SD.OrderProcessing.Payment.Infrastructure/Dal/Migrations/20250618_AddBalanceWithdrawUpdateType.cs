using FluentMigrator;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250618, TransactionBehavior.Default)]
public class AddBalanceWithdrawUpdateType: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'balance_withdraw_update_type') THEN
            CREATE TYPE balance_withdraw_update_type as
            (
                id              bigint,
                order_id        bigint,
                user_id         bigint,
                amount          numeric(21,5),
                created_at      timestamp with time zone,
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
        DROP TYPE IF EXISTS balance_withdraw_update_type;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}