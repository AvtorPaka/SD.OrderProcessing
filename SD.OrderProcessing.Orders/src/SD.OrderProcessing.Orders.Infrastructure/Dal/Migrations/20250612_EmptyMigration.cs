using FluentMigrator;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Migrations;

[Migration(version:20250612, TransactionBehavior.None)]
public class EmptyMigration: Migration 
{
    public override void Up()
    {
    }

    public override void Down()
    {
    }
}