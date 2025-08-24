using Controller_Based_APIs.Data;
using Controller_Based_APIs.Models;
using Controller_Based_APIs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controller_Based_APIs.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController(ProductRepository repository) : ControllerBase
    {
        

        [HttpOptions]
        public IActionResult OptionsProducts()
        {
            Response.Headers.Append("Allow", "GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS");
            return NoContent();
        }

  
        [HttpHead("{productId:guid}")]
        public IActionResult HeadProduct(Guid productId)
        {
            return repository.ExistsById(productId) ? Ok() : NotFound();
        }
       
        

        [HttpGet("{productId:guid}", Name = "GetProductById")]
        public ActionResult<ProductResponse> GetProductById(Guid productId, bool includeReviews = false)
        {
            var product = repository.GetProductById(productId);

            if (product is null)
                return NotFound($"Product with Id '{productId}' not found");

            List<ProductReview>? reviews = null;

            if (includeReviews == true)
            {
                reviews = repository.GetProductReviews(productId);
            }

            return ProductResponse.FromModel(product, reviews);
        }


        [HttpGet]
        public IActionResult GetPaged(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalCount = repository.GetProductsCount();

            var products = repository.GetProductsPage(page, pageSize);

            var pagedResult = PagedResult<ProductResponse>.Create(
                ProductResponse.FromModels(products),
                totalCount,
                page,
                pageSize);

            return Ok(pagedResult);
        }
    }
}
