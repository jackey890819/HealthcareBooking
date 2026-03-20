using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HealthcareBooking.API.Attributes;
using HealthcareBooking.API.DTOs;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareBooking.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    public AppointmentController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    [EndpointSummary("取得所有預約")]
    [EndpointDescription("回傳系統中所有預約資料。")]
    [ProducesResponseType(typeof(List<Appointment>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Appointment>>> GetAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("依編號取得預約")]
    [EndpointDescription("根據預約編號取得單筆預約資料。")]
    [ProducesResponseType(typeof(Appointment), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        return Ok(appointment);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("建立預約")]
    [EndpointDescription("手動建立一筆預約資料。若需要同時檢查門診名額，建議使用 Booking API。")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IdResponse>> CreateAppointment([FromBody] AppointmentUpsertRequest request)
    {
        var appointment = new Appointment
        {
            AppointmentDate = request.AppointmentDate,
            PatientId = request.PatientId,
            ClinicId = request.ClinicId
        };

        var id = await _appointmentService.AddAppointmentAsync(appointment);
        return CreatedAtAction(nameof(GetAppointment), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("更新預約")]
    [EndpointDescription("更新指定編號預約的病患、門診或預約時間。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentUpsertRequest request)
    {
        await _appointmentService.UpdateAppointmentAsync(id, new Appointment
        {
            AppointmentDate = request.AppointmentDate,
            PatientId = request.PatientId,
            ClinicId = request.ClinicId
        });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("刪除預約")]
    [EndpointDescription("刪除指定編號的預約資料。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        await _appointmentService.DeleteAppointmentAsync(id);
        return NoContent();
    }
}

[Description("建立或更新預約時使用的請求內容。")]
[OpenApiExample("""
{
  "appointmentDate": "2026-03-25T09:15:00+08:00",
  "patientId": 1,
  "clinicId": 3
}
""")]
public record AppointmentUpsertRequest
{
    [Description("預約時間。")]
    public DateTime AppointmentDate { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0.")]
    [Description("病患編號。")]
    public int PatientId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "ClinicId must be greater than 0.")]
    [Description("門診編號。")]
    public int ClinicId { get; init; }
}
