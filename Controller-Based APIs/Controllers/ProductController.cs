using Controller_Based_APIs.Data;
using Controller_Based_APIs.Models;
using Controller_Based_APIs.Requests;
using Controller_Based_APIs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpPost]
        public IActionResult CreateProduct(CreateProductRequest request)
        {
            if (repository.ExistsByName(request.Name))
                return Conflict($"A product with the name '{request.Name}' already exists.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price
            };

            repository.AddProduct(product);

            return CreatedAtRoute(routeName: nameof(GetProductById),
                                  routeValues: new { productId = product.Id },
                                  value: ProductResponse.FromModel(product));
        }


        [HttpPut("{productId:guid}")]
        public IActionResult Put(Guid productId, UpdateProductRequest request)
        {
            var product = repository.GetProductById(productId);

            if (product is null)
                return NotFound($"Product with Id '{productId}' not found");

            product.Name = request.Name;
            product.Price = request.Price ?? 0;

            var succeeded = repository.UpdateProduct(product);

            if (!succeeded)
                return StatusCode(500, "Failed to update product");

            return NoContent();
        }

        [HttpPatch("{productId:guid}")]
        public IActionResult Patch(Guid productId, JsonPatchDocument<UpdateProductRequest>? patchDoc)
        {
            if (patchDoc is null)
                return BadRequest("Invalid patch document.");

            var product = repository.GetProductById(productId);

            if (product is null)
                return NotFound($"Product with Id '{productId}' not found.");

            var updateModel = new UpdateProductRequest
            {
                Name = product.Name,
                Price = product.Price
            };

            patchDoc.ApplyTo(updateModel);

            product.Name = updateModel.Name;
            product.Price = updateModel.Price ?? 0;

            var succeeded = repository.UpdateProduct(product);

            if (!succeeded)
                return StatusCode(500, "Failed to patch product");

            return NoContent();
        }

        [HttpDelete("{productId:guid}")]
        public IActionResult Delete(Guid productId)
        {
            if (!repository.ExistsById(productId))
                return NotFound($"Product with Id '{productId}' not found");

            var succeeded = repository.DeleteProduct(productId);

            if (!succeeded)
                return StatusCode(500, "Failed to update product");

            return NoContent();
        }



    }
}
