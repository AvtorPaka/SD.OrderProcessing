using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Models.Enums;
using SD.OrderProcessing.Payment.Infrastructure.Configuration.Options;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Infrastructure;

internal static class Postgres
{
    private static readonly INpgsqlNameTranslator Translator = new NpgsqlSnakeCaseNameTranslator();

    public static void ConfigureTypeMapOptions()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public static void AddDataSource(IServiceCollection services, PostgreConnectionOptions connectionOptions,
        bool isDevelopment)
    {
        services.AddNpgsqlDataSource(
            connectionString: connectionOptions.ConnectionString,
            builder =>
            {
                builder.MapEnum<OrderStatus>("order_status_enum");
                builder.MapEnum<MessageState>("message_state_enum");
                
                builder.MapComposite<BalanceAccountEntity>("balance_account_type");
                builder.MapComposite<BalanceWithdrawUpdateEntity>("balance_withdraw_update_type");
                builder.MapComposite<PaymentStatusMessageEntity>("payment_status_message_type");

                if (isDevelopment)
                {
                    builder.EnableParameterLogging();
                }
            }
        );
    }

    public static void AddMigrations(IServiceCollection services, PostgreConnectionOptions connectionOptions)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(r => r
                .AddPostgres()
                .WithGlobalConnectionString(connectionStringOrName: connectionOptions.ConnectionString)
                .ScanIn(typeof(Postgres).Assembly).For.Migrations()
            );
    }
}