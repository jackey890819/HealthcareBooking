using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HealthcareBooking.API.Attributes;
using HealthcareBooking.API.DTOs;
using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBooking.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorController : ControllerBase
{
    private readonly DoctorService _doctorService;

    public DoctorController(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [EndpointSummary("取得所有醫生")]
    [EndpointDescription("回傳系統中所有醫生資料。")]
    [ProducesResponseType(typeof(List<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("依編號取得醫生")]
    [EndpointDescription("根據醫生編號取得單筆醫生資料。")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        return Ok(doctor);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("建立醫生")]
    [EndpointDescription("建立一筆新的醫生資料。")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IdResponse>> CreateDoctor([FromBody] DoctorUpsertRequest request)
    {
        var doctor = new Doctor(request.Name);
        var id = await _doctorService.AddDoctorAsync(doctor);
        return CreatedAtAction(nameof(GetDoctor), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("更新醫生")]
    [EndpointDescription("更新指定編號醫生的名稱。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorUpsertRequest request)
    {
        await _doctorService.UpdateDoctorAsync(id, request.Name);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("刪除醫生")]
    [EndpointDescription("刪除指定編號的醫生資料。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        await _doctorService.DeleteDoctorAsync(id);
        return NoContent();
    }
}

[Description("建立或更新醫生資料時使用的請求內容。")]
[OpenApiExample("""
{
  "name": "王小明醫師"
}
""")]
public record DoctorUpsertRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [Description("醫生姓名。")]
    public string Name { get; init; } = string.Empty;
}
