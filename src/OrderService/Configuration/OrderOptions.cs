namespace OrderService.Configuration;

public class OrderOptions
{
    public const string SectionName = "Orders";

    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
}
