using api_stockci.Models;
using api_stockci.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_stockci.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Category category)
    {
        await _categoryService.CreateAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, Category category)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        await _categoryService.UpdateAsync(id, category);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
