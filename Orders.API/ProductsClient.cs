using Microsoft.Net.Http.Headers;
using Products.DataModels;
using System;

namespace Orders.API
{
    public class ProductsClient : IProductsClient
    {
        private readonly HttpClient _httpClient;

        public ProductsClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;

            _httpClient.BaseAddress = new Uri("https://localhost:7221");

            _httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent, "HttpRequestsSample");
        }

        public async Task RemoveFromStock(string productId, int count)
        {
            var response = await _httpClient.DeleteAsync($"/product-stock/{productId}/{count}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Product[]> GetAvailableProducts()
        {
            //https://localhost:7221/product-stock
            var list = await _httpClient.GetFromJsonAsync<IEnumerable<Product>>("product-stock");
            return list!.ToArray();
        }
    }
}
