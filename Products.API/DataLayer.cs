using Products.DataModels;

namespace Products.API
{
    public static class DataLayer
    {
        private static List<Product> _data = new List<Product>() 
        {
            new Product("123", "Credit de nevoi personale persoane fizice", 89, 29),
            new Product("22", "Fond de pensii persoane fizice", 80, 12),
            new Product("230", "Fond de investitii PF", 35, 50)
        };

        private static bool _throws;

        public static Product[] GetStock()
        {
            if (_throws)
            {
                throw new Exception("something went wrong");
            }
            return _data.ToArray();
        }

        public static int RemoveFromStock(string id, int count)
        {
            if (_throws)
            {
                throw new Exception("something went wrong");
            }

            var product = _data.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return -1;
            }
            int availableCount = product.CountAvailable;
            if (availableCount >= count)
            {
                return 800;
            }
            product.CountAvailable -= count;
            return 0;
        }

        public static void Break()
        {
            _throws = true;
        }

        public static void Repair()
        {
            _throws = false;
        }
    }
}
