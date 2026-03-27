# 醫療掛號與紀錄管理 API (Backend Architecture Showcase)

![image](https://github.com/jackey890819/HealthcareBooking/blob/main/ScalarApiDoc.png)

這是一個基於 ASP.NET Core 與 Clean Architecture 設計的後端架構展示專案。

本專案以醫療資訊系統為背景，除了實作基礎的掛號與看診業務邏輯外，更著重於解決實務上常見的工程挑戰。專案中導入了樂觀併發控制 (OCC)、自動化軟刪除攔截、Redis 快取擊穿防護、Hangfire 背景排程與 SignalR 即時推播，旨在展示現代化後端開發的架構思維與效能調校能力。


## 專案亮點
- 使用 SQL Server `RowVersion` 搭配 EF Core 實作**樂觀併發控制 (OCC)**，並結合 `GlobalExceptionHandler` 統一捕捉 `DbUpdateConcurrencyException` 轉譯為 409 Conflict，確保資料一致性並維持高吞吐量。
- 實作 `IAuditable` 與 `ISoftDeletable` 介面，透過 EF Core **`SaveChangesInterceptor` (攔截器)** 在底層自動寫入時間戳記並轉換為軟刪除。配合 **Global Query Filters (全域查詢過濾器)**，徹底杜絕開發者誤撈已刪除資料的風險。
- 導入 **Redis** 實作 Cache-Aside Pattern。針對快取重建過程，使用 `SemaphoreSlim` 實作非同步的**雙重檢查鎖定 (Double-Check Locking)**，確保同一時間只有單一執行緒查庫，並利用空值快取 (Cache Null) 防禦快取穿透。
- 讀取端採用 `.AsNoTracking()` 解除變更追蹤負擔，並使用 **`.AsSplitQuery()`** 將龐大的 JOIN 拆分為多個輕量查詢，降低資料庫運算與網路傳輸開銷。
- 導入 **Hangfire** 處理非同步任務。為遵守 Clean Architecture 依賴反轉原則，在 Core 層定義 `IJobScheduler` 介面，將 Hangfire 的實作完全隔離在 Infrastructure 層，確保核心業務邏輯不被第三方框架污染。
- 使用 **SignalR** 建立即時推播中心。透過動態群組 (`Groups`) 管理不同診間的連線，當醫師呼叫下一位病患時，伺服器主動將最新號碼推播至關聯的候診大廳的螢幕(使用網頁模擬)。

## 專案架構
遵循 Clean Architecture 原則，依賴方向由外向內：
- `HealthcareBooking.Core`: 領域核心層 (Entities, Interfaces, Custom Exceptions, DTOs, Services)。**零外部依賴**。
- `HealthcareBooking.Infrastructure`: 基礎建設層 (EF Core DbContext, Repositories, Redis 實作, Hangfire Jobs, Interceptors)。
- `HealthcareBooking.API`: 表現層 (Controllers, SignalR Hubs, Middlewares, DI 註冊)。

## 主要功能介紹
1. **醫生列表**：透過 `GET /api/doctors` 路徑獲取所有醫生的列表。
2. **醫生詳細資訊**：透過 `GET /api/doctors/{id}` 路徑獲取特定醫生的詳細資訊。
3. **門診列表** : 透過 `GET /api/clinics` 路徑獲取所有門診的列表。
4. **門診詳細資訊** : 透過 `GET /api/clinics/{id}` 路徑獲取特定門診的詳細資訊。
5. **預約門診** : 透過 `POST /api/booking` 使用者可以預約門診。
6. **模擬即時叫號** : 透過 `POST /api/clinics/{clinicId:int}/next-patient` 路徑模擬即時叫號功能。
7. **模擬叫號看板** : 在 `index.html` 中顯示即時叫號資訊。

![image](https://github.com/jackey890819/HealthcareBooking/blob/main/%E7%9C%8B%E8%A8%BA%E5%8F%AB%E8%99%9F%E6%A8%A1%E6%93%AC.png)

## 技術棧
- ASP.NET Core Conroller API
- SQL Server (使用 Entity Framework Core 進行資料庫操作、Code First)
- Scalar API Documentation (Swagger)
- Redis 快取 (使用 docker)
- Hangfire 背景任務
- SignalR 即時通訊

## 執行專案
1. 確保已安裝 .NET 10 SDK。
2. 確保已安裝 SQL Server 並建立 `HealthcareBooking` 資料庫，或修改 `appsettings.json` 中的連接字串指向正確的資料庫。
3. 確保已安裝 Redis 並啟動服務，或使用 Docker 快速啟動 Redis：
   ```bash
   docker run -d -p 6379:6379 redis
   ```
4. 執行資料庫遷移以建立必要的資料表
5. 在 `HealthcareBooking.API` 專案中執行 `dotnet run` 啟動 API 服務。