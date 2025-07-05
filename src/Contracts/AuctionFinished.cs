namespace Contracts;

public class AuctionFinished
{
    public required string AuctionId { get; set; }
    public bool ItemSold { get; set; }
    public string? Winner { get; set; }
    public required string Seller { get; set; }
    public int? Amount { get; set; }
}
