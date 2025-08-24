using Controller_Based_APIs.Data;
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

    }
}
