using Dapper;
using Microsoft.Data.SqlClient;
using look.Models;

namespace look.repository;

public class Repository
{
    private readonly string _connectionString =
        "Server=.\\SQLEXPRESS;Database=lookTABLE;Trusted_Connection=True;TrustServerCertificate=True;";

    public async Task AddSale(Sales sale)
    {
        sale.TaxAmount = sale.Amount * 0.10m;

        sale.Amount = sale.Amount - sale.TaxAmount;

        sale.IsTaxPaid = false;

        const string sql = """
            INSERT INTO Sales
                (Id, Amount, TaxAmount, IsTaxPaid)
            VALUES
                (@Id, @Amount, @TaxAmount, @IsTaxPaid);
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, sale);
    }

    public async Task PayTax()
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            const string sql = """
                SELECT COALESCE(SUM(TaxAmount), 0)
                FROM Sales WITH (TABLOCKX, HOLDLOCK)
                WHERE IsTaxPaid = 0;
                """;

            decimal taxs =
                await connection.QuerySingleAsync<decimal>(
                    sql,
                    transaction: transaction
                );

            if (taxs > 0)
            {
                const string sql1 = """
                    UPDATE Sales
                    SET IsTaxPaid = 1
                    WHERE IsTaxPaid = 0;

                    INSERT INTO TaxPayments (Amount)
                    VALUES (@Amount , GETDATE() );
                    """;

                await connection.ExecuteAsync(
                    sql1,
                    new { Amount = taxs },
                    transaction: transaction
                );
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}