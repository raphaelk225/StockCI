using api_stockci.Models;
using api_stockci.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_stockci.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        return Ok(await _productService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Product product)
    {
        await _productService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, Product product)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        await _productService.UpdateAsync(id, product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
