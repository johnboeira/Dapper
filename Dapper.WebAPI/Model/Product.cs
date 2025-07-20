namespace Dapper.WebAPI.Model;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<Review> Reviews { get; set; } = new();
}