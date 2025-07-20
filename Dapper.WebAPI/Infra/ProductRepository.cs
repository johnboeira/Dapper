using Dapper.WebAPI.Infra.Common;
using Dapper.WebAPI.Model;

namespace Dapper.WebAPI.Infra;

public class ProductRepository(IDbConnectionFactory connection)
{
    private readonly IDbConnectionFactory _dbConnectionFactory = connection;

    public async Task<int> CreateAsync(Product product, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        //TODO jogar para uma classe estatica de constantes
        var sql = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price); SELECT last_insert_rowid();";

        //Rollback implicitly if an exception occurs
        var command = new CommandDefinition(sql, product, transaction: transaction, cancellationToken: token);

        try
        {
            var dbResult = await connection.ExecuteScalarAsync<int>(command);

            transaction.Commit();

            return dbResult;
        }
        catch (Exception)
        {
            transaction.Rollback();
            return 0;
        }
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var sql = "UPDATE Products SET Name = @Name, Price = @Price WHERE Id = @Id";

        var command = new CommandDefinition(sql, product, transaction: transaction, cancellationToken: token);

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
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var sql = "DELETE FROM Products WHERE Id = @Id";

        var command = new CommandDefinition(sql, new { Id = id }, transaction: transaction, cancellationToken: token);

        var rows = await connection.ExecuteAsync(command);

        transaction.Commit();

        return rows > 0;
    }

    public async Task<bool> BulkUpdateResetNameAsync(List<Product> products, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        products.ForEach(p => p.Name = "Reseted Name");

        try
        {
            var sql = @"
                UPDATE Products
                SET Name = @Name
                WHERE Id = @Id";

            var command = new CommandDefinition(sql, products, transaction, cancellationToken: token);

            await connection.ExecuteAsync(command);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var sql = "SELECT * FROM Products";

        var command = new CommandDefinition(sql, cancellationToken: token);

        return await connection.QueryAsync<Product>(command);
    }

    public async Task<Product?> GetByIdWithReviewsAsync(int id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var sqlProduct = "SELECT * FROM Products WHERE Id = @Id";
        var sqlReviews = "SELECT * FROM Reviews WHERE ProductId = @ProductId";

        var commandProduct = new CommandDefinition(sqlProduct, new { Id = id }, cancellationToken: token);
        var product = await connection.QuerySingleOrDefaultAsync<Product>(commandProduct);

        if (product is not null)
        {
            var commandReviews = new CommandDefinition(sqlReviews, new { ProductId = product.Id }, cancellationToken: token);
            var reviews = await connection.QueryAsync<Review>(commandReviews);
            product.Reviews = reviews.ToList();
        }

        return product;
    }

    public async Task<string> GetProductNameByIdAsync(int id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var sql = "SELECT Name FROM Products WHERE Id = @Id";

        var commandProduct = new CommandDefinition(sql, new { Id = id }, cancellationToken: token);
        var product = await connection.QuerySingleAsync<string>(commandProduct);

        return product;
    }

    public async Task<int> GetCountAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var sql = "SELECT COUNT(*) FROM Products";

        var commandProduct = new CommandDefinition(sql, cancellationToken: token);
        var product = await connection.QuerySingleAsync<int>(commandProduct);

        return product;
    }
}