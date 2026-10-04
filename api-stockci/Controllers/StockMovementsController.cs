using api_stockci.Models;
using api_stockci.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_stockci.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StockMovement>>> GetAll()
    {
        return Ok(await _stockMovementService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockMovement>> GetById(string id)
    {
        var movement = await _stockMovementService.GetByIdAsync(id);
        if (movement is null) return NotFound();
        return Ok(movement);
    }

    [HttpPost]
    public async Task<ActionResult> Create(StockMovement movement)
    {
        var result = await _stockMovementService.CreateAsync(movement);
        return result switch
        {
            StockMovementResult.ProductNotFound => NotFound(new { message = "Produit introuvable." }),
            StockMovementResult.UserNotFound => NotFound(new { message = "Utilisateur introuvable." }),
            StockMovementResult.InsufficientStock => BadRequest(new { message = "Stock insuffisant pour cette sortie." }),
            _ => CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement)
        };
    }
}
