using FluentMigrator;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Migrations;

[Migration(version:20250613, TransactionBehavior.Default)]
public class AddOrderStatusEnum: Migration
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_status_enum') THEN
            CREATE TYPE order_status_enum as ENUM
            (
                'failed',
                'balancenotexists',
                'insufficientfunds',
                'pending',
                'finished'
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
        DROP TYPE IF EXISTS order_status_enum;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}