using Dapper.WebAPI.Infra;
using Dapper.WebAPI.Infra.Common;
using Dapper.WebAPI.Model;

var builder = WebApplication.CreateBuilder(args);
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddSingleton<IDbConnectionFactory>(sp => new SqlLiteConnectionFactory(connectionString!));
    builder.Services.AddScoped<ProductRepository>();
    builder.Services.AddScoped<ReviewRepository>();
    builder.Services.AddSingleton<DbInitializer>();
}

var app = builder.Build();

app.MapPost("/product", async (ProductRepository productRepository, Product product) =>
{
    var id = await productRepository.CreateAsync(product);
    var uri = $"/product/{id}";
    return Results.Created(uri, id);
});

app.MapPut("/product", async (ProductRepository productRepository, Product product) =>
{
    var updatedResult = await productRepository.UpdateAsync(product);
    return Results.Ok($"Update result:{updatedResult}");
});

app.MapDelete("/product/{id:int}", async (ProductRepository productRepository, int id) =>
{
    var result = await productRepository.DeleteAsync(id);
    return Results.Ok($"Deleted result: {result}");
});

app.MapGet("/product/{id:int}", async (ProductRepository productRepository, int id, CancellationToken cancellationToken) =>
{
    var product = await productRepository.GetByIdWithReviewsAsync(id, cancellationToken);
    return Results.Ok(product);
});

app.MapGet("/productName/{id:int}", async (ProductRepository productRepository, int id, CancellationToken cancellationToken) =>
{
    var productName = await productRepository.GetProductNameByIdAsync(id, cancellationToken);
    return Results.Ok(productName);
});

app.MapPut("/products", async (ProductRepository productRepository, List<Product> products) =>
{
    var updatedResult = await productRepository.BulkUpdateResetNameAsync(products);
    return Results.Ok($"Update result:{updatedResult}");
});

//REVIEWS

app.MapGet("/count", async (ProductRepository productRepository, CancellationToken cancellationToken) =>
{
    var count = await productRepository.GetCountAsync(cancellationToken);
    return Results.Ok(count);
});

app.MapGet("/products", async (ProductRepository productRepository) =>
{
    var products = await productRepository.GetAllAsync();
    return Results.Ok(products);
});

app.MapPost("/review", async (ReviewRepository reviewRepository, Review review) =>
{
    var id = await reviewRepository.CreateAsync(review);
    var uri = $"/review/{id}";
    return Results.Created(uri, id);
});

app.MapPut("/review", async (ReviewRepository reviewRepository, Review review) =>
{
    var updatedResult = await reviewRepository.UpdateAsync(review);
    return Results.Ok($"Update result:{updatedResult}");
});

app.MapDelete("/review/{id:int}", async (ReviewRepository reviewRepository, int id) =>
{
    var result = await reviewRepository.DeleteAsync(id);
    return Results.Ok($"Deleted result: {result}");
});

app.MapGet("/review/{id:int}", async (ReviewRepository reviewRepository, int id, CancellationToken cancellationToken) =>
{
    var review = await reviewRepository.GetByIdAsync(id, cancellationToken);
    return Results.Ok(review);
});

app.MapGet("/reviews", async (ReviewRepository reviewRepository) =>
{
    var reviews = await reviewRepository.GetAllAsync();
    return Results.Ok(reviews);
});

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();