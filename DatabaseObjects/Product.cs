
namespace InventoryAPI.DatabaseObjects
{
    public class Product
    {
        public int Id { get; set; }
        public int InstrumentType_Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLocation { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
    }
}
