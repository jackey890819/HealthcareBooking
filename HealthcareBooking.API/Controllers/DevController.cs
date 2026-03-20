using HealthcareBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DevController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("reset")]
    [EndpointSummary("重置資料庫（僅開發環境）")]
    [EndpointDescription("清空所有資料並重新填入預設 Seed 資料，僅在 Development 環境下有效。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reset()
    {
        if (!_env.IsDevelopment())
            return Forbid();

        await AppDbContextSeeder.ResetAndSeedAsync(_context);
        return NoContent();
    }
}