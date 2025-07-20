namespace Dapper.WebAPI.Infra.Common;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var sql = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL
            );
        ";

        await connection.ExecuteAsync(sql);

        var sqlReview = @"
            CREATE TABLE IF NOT EXISTS Reviews (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                Rating INTEGER NOT NULL CHECK(Rating >= 1 AND Rating <= 5),
                FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
            );";

        await connection.ExecuteAsync(sqlReview);
    }
}