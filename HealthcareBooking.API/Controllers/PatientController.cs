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
public class PatientController : ControllerBase
{
    private readonly PatientService _patientService;

    public PatientController(PatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    [EndpointSummary("取得所有病患")]
    [EndpointDescription("回傳系統中所有病患資料。")]
    [ProducesResponseType(typeof(List<Patient>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Patient>>> GetPatients()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("依編號取得病患")]
    [EndpointDescription("根據病患編號取得單筆病患資料。")]
    [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        return Ok(patient);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("建立病患")]
    [EndpointDescription("建立一筆新的病患資料。")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IdResponse>> CreatePatient([FromBody] PatientUpsertRequest request)
    {
        var patient = new Patient
        {
            Name = request.Name
        };

        var id = await _patientService.AddPatientAsync(patient);
        return CreatedAtAction(nameof(GetPatient), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("更新病患")]
    [EndpointDescription("更新指定編號病患的基本資料。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpsertRequest request)
    {
        await _patientService.UpdatePatientAsync(id, new Patient
        {
            Name = request.Name
        });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("刪除病患")]
    [EndpointDescription("刪除指定編號的病患資料。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(int id)
    {
        await _patientService.DeletePatientAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/history")]
    [EndpointSummary("取得病患預約歷史")]
    [EndpointDescription("根據病患編號取得病患的預約歷史資料。")]
    [ProducesResponseType(typeof(PatientHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientHistoryDto>> GetPatientHistory([FromRoute] int id, [FromServices] PatientQueryService patientQueryService)
    {
        var patientHistory = await patientQueryService.GetPatientHistoryAsync(id);
        return Ok(patientHistory);
    }
}

[Description("建立或更新病患資料時使用的請求內容。")]
[OpenApiExample("""
{
  "name": "陳美玲"
}
""")]
public record PatientUpsertRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [Description("病患姓名。")]
    public string Name { get; init; } = string.Empty;
}
