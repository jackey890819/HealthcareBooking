using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Infrastructure.Interceptors;

/// <summary>
/// 審計攔截器 (Audit Interceptor)
/// 
/// 這個攔截器會在 SaveChanges 時自動處理實體的審計資訊 (CreatedAt、UpdatedAt) 以及軟刪除 (IsDeleted、DeletedAt)。
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // 取得所有發生變動的實體 (Change Tracker)
        var entries = dbContext.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // 處理 IAuditable (新增或修改)
            if (entry.Entity is IAuditable auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    // 在這裡設定 CreatedAt 的時間 (記得用 UTC 時間！)
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // 在這裡設定 UpdatedAt 的時間
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            // 處理 ISoftDeletable (軟刪除)
            if (entry.Entity is ISoftDeletable softDeletableEntity && entry.State == EntityState.Deleted)
            {
                // 將狀態從 Deleted 強制改回 Modified (這樣 EF 才會下 UPDATE 指令而不是 DELETE)
                entry.State = EntityState.Modified;
                // 將 IsDeleted 設為 true
                entry.Property("IsDeleted").CurrentValue = true;
                // 記錄 DeletedAt 時間
                entry.Property("IsDeleted").CurrentValue = true;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
