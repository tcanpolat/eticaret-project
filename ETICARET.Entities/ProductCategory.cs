namespace ETICARET.Entities
{
    public class ProductCategory
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; } // Navigation property
        public int ProductId { get; set; }
        public Product Product { get; set; } // Navigation property
    }
}