using FluentMigrator;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Migrations;

[Migration(version:20250616, TransactionBehavior.Default)]
public class AddUserOrderType: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_order_type') THEN
            CREATE TYPE user_order_type as
            (
                id           bigint,
                user_id      bigint,
                amount       numeric(21,5),
                description  varchar(256),
                status       order_status_enum
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
        DROP TYPE IF EXISTS user_order_type;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}