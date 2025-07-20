namespace Dapper.WebAPI.Model;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; } // 1 a 5 estrelas
}