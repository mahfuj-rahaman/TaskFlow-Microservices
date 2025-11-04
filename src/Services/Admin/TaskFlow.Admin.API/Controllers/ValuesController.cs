using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Admin.API.Controllers;

/// <summary>
/// Sample Values controller for testing Admin service
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ValuesController : ControllerBase
{
    /// <summary>
    /// Get all values
    /// </summary>
    /// <response code="200">Returns list of values</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new[] { "admin-value1", "admin-value2", "admin-value3" });
    }

    /// <summary>
    /// Get value by ID
    /// </summary>
    /// <param name="id">The value ID</param>
    /// <response code="200">Returns the value</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get(int id)
    {
        return Ok($"admin-value-{id}");
    }

    /// <summary>
    /// Create a new value
    /// </summary>
    /// <param name="value">The value to create</param>
    /// <response code="201">Value created successfully</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult Post([FromBody] string value)
    {
        return CreatedAtAction(nameof(Get), new { id = 1 }, value);
    }

    /// <summary>
    /// Update a value
    /// </summary>
    /// <param name="id">The value ID</param>
    /// <param name="value">The updated value</param>
    /// <response code="204">Value updated successfully</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Put(int id, [FromBody] string value)
    {
        return NoContent();
    }

    /// <summary>
    /// Delete a value
    /// </summary>
    /// <param name="id">The value ID</param>
    /// <response code="204">Value deleted successfully</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Delete(int id)
    {
        return NoContent();
    }

    /// <summary>
    /// Get service health status
    /// </summary>
    /// <response code="200">Service is healthy</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            service = "Admin Service",
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
