using Dapper.WebAPI.Infra.Common;
using Dapper.WebAPI.Model;

namespace Dapper.WebAPI.Infra;

public class ReviewRepository(IDbConnectionFactory connection)
{
    private readonly IDbConnectionFactory dbConnectionFactory = connection;

    public async Task<int> CreateAsync(Review review, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var sql = "INSERT INTO Reviews (ProductId, Rating) VALUES (@ProductId, @Rating); SELECT last_insert_rowid();";
        var command = new CommandDefinition(sql, review, transaction: transaction, cancellationToken: token);

        var dbResult = await connection.ExecuteScalarAsync<int>(command);

        transaction.Commit();

        return dbResult;
    }

    public async Task<bool> UpdateAsync(Review review, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var sql = "UPDATE Reviews SET ProductId = @ProductId, Rating = @Rating WHERE Id = @Id";
        var command = new CommandDefinition(sql, review, transaction: transaction, cancellationToken: token);

        //Rollback explicitly if an exception occurs
        try
        {
            var rows = await connection.ExecuteAsync(command);
            transaction.Commit();
            return rows > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var sql = "DELETE FROM Reviews WHERE Id = @Id";
        var command = new CommandDefinition(sql, new { Id = id }, transaction: transaction, cancellationToken: token);

        var rows = await connection.ExecuteAsync(command);

        transaction.Commit();

        return rows > 0;
    }

    public async Task<IEnumerable<Review>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        var sql = "SELECT * FROM Reviews";
        var command = new CommandDefinition(sql, cancellationToken: token);

        return await connection.QueryAsync<Review>(command);
    }

    public async Task<Review?> GetByIdAsync(int id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);

        var sql = "SELECT * FROM Reviews WHERE Id = @Id";
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: token);

        return await connection.QueryFirstOrDefaultAsync<Review>(command);
    }
}