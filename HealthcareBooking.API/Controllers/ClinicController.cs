using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HealthcareBooking.API.Attributes;
using HealthcareBooking.API.DTOs;
using HealthcareBooking.API.Hubs;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HealthcareBooking.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClinicController : ControllerBase
{
    private readonly ClinicService _clinicService;

    public ClinicController(ClinicService clinicService)
    {
        _clinicService = clinicService;
    }

    [HttpGet]
    [EndpointSummary("取得所有門診")]
    [EndpointDescription("回傳系統中所有門診資料。")]
    [ProducesResponseType(typeof(List<Clinic>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Clinic>>> GetClinics()
    {
        var clinics = await _clinicService.GetAllClinicsAsync();
        return Ok(clinics);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("依編號取得門診")]
    [EndpointDescription("根據門診編號取得單筆門診資料。")]
    [ProducesResponseType(typeof(Clinic), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Clinic>> GetClinic(int id)
    {
        var clinic = await _clinicService.GetClinicByIdAsync(id);
        return Ok(clinic);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("建立門診")]
    [EndpointDescription("建立一筆新的門診資料。")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IdResponse>> CreateClinic([FromBody] ClinicUpsertRequest request)
    {
        var clinic = new Clinic(request.DoctorId, request.ClinicDate, request.MaxQuota);
        var id = await _clinicService.AddClinicAsync(clinic);
        return CreatedAtAction(nameof(GetClinic), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("更新門診")]
    [EndpointDescription("更新指定編號門診的醫生、看診時間與名額。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateClinic(int id, [FromBody] ClinicUpsertRequest request)
    {
        await _clinicService.UpdateClinicAsync(id, request.DoctorId, request.ClinicDate, request.MaxQuota);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("刪除門診")]
    [EndpointDescription("刪除指定編號的門診資料。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClinic(int id)
    {
        await _clinicService.DeleteClinicAsync(id);
        return NoContent();
    }

    [HttpPost("{clinicId:int}/next-patient")]
    [EndpointSummary("呼叫指定號碼的病患")]
    [EndpointDescription("呼叫指定號碼的病患，並透過 SignalR Hub 通知前端更新叫號資訊。")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallNextPatient(
        [FromRoute] int clinicId, 
        [FromBody] int nextPatientNumber,
        [FromServices] IHubContext<ClinicHub> hubContext)
    {
        // TODO: 更新門診的目前叫號資訊 (目前尚未實作，僅模擬呼叫)
        //await _clinicService.UpdateCurrentAsync(clinicId, nextPatientNumber);
        // 模擬呼叫 SignalR Hub 通知前端更新叫號資訊
        await hubContext.Clients.Group($"Clinic_{clinicId}").SendAsync("ReceiveNextPatient", nextPatientNumber);

        return Ok(new { Message = $"已經成功呼叫第 {nextPatientNumber} 號" });
    }
}

[Description("建立或更新門診時使用的請求內容。")]
[OpenApiExample("""
{
  "doctorId": 1,
  "clinicDate": "2026-03-25T09:00:00+08:00",
  "maxQuota": 20
}
""")]
public record ClinicUpsertRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "DoctorId must be greater than 0.")]
    [Description("看診醫生編號。")]
    public int DoctorId { get; init; }

    [Description("門診開始時間。")]
    public DateTime ClinicDate { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "MaxQuota must be greater than 0.")]
    [Description("門診最大可掛號人數。")]
    public int MaxQuota { get; init; }
}
