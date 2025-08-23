namespace Controller_Based_APIs.Models
{
    public class ProductReview
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? Reviewer { get; set; }
        public int Stars { get; set; }
    }
}
