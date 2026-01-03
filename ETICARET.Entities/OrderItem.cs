namespace ETICARET.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } // Navigation property
        public int ProductId { get; set; }
        public Product Product { get; set; } // Navigation property
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}