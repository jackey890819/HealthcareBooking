using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// 取得快取資料
    /// </summary>
    /// <typeparam name="T">快取資料的類型</typeparam>
    /// <returns>快取資料的值，如果不存在則為 null</returns>
    Task<T?> GetAsync<T>();

    /// <summary>
    /// 設定快取資料，並指定過期時間
    /// </summary>
    /// <typeparam name="T">快取資料的類型</typeparam>
    /// <param name="key">快取資料的 key</param>
    /// <param name="value">快取資料的值</param>
    /// <param name="expirationTime">快取資料的過期時間</param>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, TimeSpan expirationTime);

    /// <summary>
    /// 刪除指定 key 的快取資料
    /// </summary>
    /// <param name="key">要刪除的快取資料的 key</param>
    /// <returns></returns>
    Task RemoveAsync(string key);
}
