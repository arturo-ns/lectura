using Microsoft.AspNetCore.Mvc;
using pc2_7420_u20231f226.sale.domain.service;
using pc2_7420_u20231f226.sale.interfaces.REST.resources;
using Swashbuckle.AspNetCore.Annotations;

namespace pc2_7420_u20231f226.sale.interfaces.REST.controllers;

/// <summary>
/// Bills controller for handling bill operations
/// </summary>
/// <remarks>
/// Alex Sanchez Ponce
/// </remarks>
[ApiController]
[Route("api/v1/bills")]
public class BillsController : ControllerBase
{
    private readonly IBillCommandService _commandService;

    /// <summary>
    /// Initializes a new instance of BillsController
    /// </summary>
    /// <param name="commandService">Bill command service</param>
    public BillsController(IBillCommandService commandService)
    {
        _commandService = commandService;
    }

    /// <summary>
    /// Creates a new bill
    /// </summary>
    /// <param name="resource">The bill creation data</param>
    /// <returns>The created bill resource</returns>
    /// <response code="201">Returns the newly created bill</response>
    /// <response code="400">If the bill data is invalid</response>
    /// <response code="409">If a bill with the same invoice already exists</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new bill",
        Description = "Creates a new bill with the provided data. Returns the created bill without plate and adviser information."
    )]
    [ProducesResponseType(typeof(BillResource), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateBill([FromBody] CreateBillResource resource)
    {
        try
        {
            var result = await _commandService.Handle(resource);
            return CreatedAtAction(nameof(CreateBill), new { billNumber = result.BillNumber }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception (you might want to use ILogger here)
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }
}