using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.API.Middlewares;

/// <summary>
/// 全局例外處理器，捕獲未處理的例外並進行統一處理
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 建立統一的錯誤響應格式
        ProblemDetails problemDetails = GenerateProblemDetails(exception);
        // 寫入回應
        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        // 返回 true 表示已經處理了這個例外，不需要再進行其他處理
        return true;
    }

    /// <summary>
    /// 產生 ProblemDetails 物件，根據不同的例外類型返回不同的錯誤訊息和狀態碼
    /// </summary>
    /// <param name="exception">繼承自 Exception 的例外物件</param>
    /// <returns>返回對應的 ProblemDetails 物件</returns>
    private ProblemDetails GenerateProblemDetails(Exception exception)
    {
        // 這裡可以根據不同的例外類型返回不同的 ProblemDetails
        // 例如：ArgumentException 可以返回 400 Bad Request 的 ProblemDetails
        // detail 可能是具體的錯誤訊息，但若是不想暴露內部細節，可以返回一個通用的訊息
        return exception switch
        {
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = exception.Message
            },
            KeyNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = exception.Message
            },
            DbUpdateConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "A concurrency conflict occurred while updating the database."
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };
    }
}
