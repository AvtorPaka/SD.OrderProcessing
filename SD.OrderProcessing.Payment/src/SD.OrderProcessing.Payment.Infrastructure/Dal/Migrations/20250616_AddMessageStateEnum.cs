using FluentMigrator;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Migrations;

[Migration(version:20250616, TransactionBehavior.Default)]
public class AddMessageStateEnum: Migration 
{
    public override void Up()
    {
        const string sql = @"
DO
    $$ BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'message_state_enum') THEN
            CREATE TYPE message_state_enum as ENUM
            (
                'pending',
                'done'
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
        DROP TYPE IF EXISTS message_state_enum;
    END 
$$;
";
        
        Execute.Sql(sql);
    }
}