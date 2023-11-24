namespace Products.DataModels
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal PricePerUnit { get; set; }
        public int CountAvailable { get; set; }

        public Product(string id, string name, decimal pricePerUnit, int countAvailable)
        {
            Id = id;
            Name = name;
            PricePerUnit = pricePerUnit;
            CountAvailable = countAvailable;
        }
    }
}