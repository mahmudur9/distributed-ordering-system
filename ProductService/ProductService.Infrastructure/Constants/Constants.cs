namespace ProductService.Infrastructure.Constants;

public static class Constants
{
    // Product picture type
    public const int PictureFromLink = 1;
    public const int PictureFromFile = 2;
    
    // Caching keys
    public const string ProductCacheKey = "product:";
    
    // Caching indexes
    public const string ProductCacheIndex = "idx:products";
}