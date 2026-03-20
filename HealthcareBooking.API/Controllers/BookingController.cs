using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HealthcareBooking.API.Attributes;
using HealthcareBooking.API.DTOs;
using HealthcareBooking.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBooking.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly ILogger<BookingController> _logger;
    private readonly BookService _bookService;

    public BookingController(ILogger<BookingController> logger, BookService bookService)
    {
        _logger = logger;
        _bookService = bookService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("掛號")]
    [EndpointDescription("依病患編號與門診編號建立預約，並同步更新門診已掛號人數。若門診不存在或已滿額會回傳錯誤。")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IdResponse>> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var id = await _bookService.BookAppointmentAsync(request.PatientId, request.ClinicId);
        return Ok(new IdResponse { Id = id });
    }
}

[Description("快速掛號使用的請求內容。系統會依門診狀態建立預約並更新已掛號人數。")]
[OpenApiExample("""
{
  "patientId": 1,
  "clinicId": 3
}
""")]
public record BookAppointmentRequest
{
    [Required(ErrorMessage = "PatientId is required.")]
    [Description("要掛號的病患編號。")]
    public int PatientId { get; init; }

    [Required(ErrorMessage = "ClinicId is required.")]
    [Description("要掛號的門診編號。")]
    public int ClinicId { get; init; }
}
