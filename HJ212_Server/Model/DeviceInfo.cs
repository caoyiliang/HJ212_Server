﻿namespace HJ212_Server.Model;

/// <summary>
/// 设备信息
/// </summary>
/// <param name="infoId">参数名</param>
/// <param name="info">参数值</param>
public class DeviceInfo(string infoId, string info)
{
    /// <summary>参数名</summary>
    public string InfoId { get; set; } = infoId;
    /// <summary>参数值</summary>
    public string Info { get; set; } = info;
}
