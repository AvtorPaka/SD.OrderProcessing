using FluentMigrator;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250614, TransactionBehavior.Default)]
public class AddBalanceAccountType: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'balance_account_type') THEN
            CREATE TYPE balance_account_type as
            (
                id                  bigint,
                user_id             bigint,
                balance             numeric(21, 5),
                version             bigint
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
        DROP TYPE IF EXISTS balance_account_type;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}