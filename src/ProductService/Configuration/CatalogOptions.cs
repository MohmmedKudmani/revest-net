namespace ProductService.Configuration;

public class CatalogOptions
{
    public const string SectionName = "Catalog";

    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
}
