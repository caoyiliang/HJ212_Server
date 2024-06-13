namespace HJ212_Server.Model;

/// <summary>
/// 统计数据
/// </summary>
/// <param name="name">污染物名称</param>
public class StatisticsData(string name)
{
    /// <summary>污染物名称</summary>
    public string Name { get; set; } = name;
    /// <summary>污染物指定时间内累计值</summary>
    public string? Cou { get; set; }
    /// <summary>污染物指定时间内最小值</summary>
    public string? Min { get; set; }
    /// <summary>污染物指定时间内平均值</summary>
    public string? Avg { get; set; }
    /// <summary>污染物指定时间内最大值</summary>
    public string? Max { get; set; }
    /// <summary>监测仪器数据标记</summary>
    public string? Flag { get; set; }
    /// <summary>昼夜等效升级</summary>
    public string? Data { get; set; }
    /// <summary>昼间等效升级</summary>
    public string? DayData { get; set; }
    /// <summary>夜间等效升级</summary>
    public string? NightData { get; set; }
}
