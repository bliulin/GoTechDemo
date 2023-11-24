using Products.DataModels;

namespace Orders.API
{
    public interface IProductsClient
    {
        Task RemoveFromStock(string productId, int count);
        Task<Product[]> GetAvailableProducts();
    }
}