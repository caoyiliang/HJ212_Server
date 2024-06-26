﻿// See https://aka.ms/new-console-template for more information
using Communication.Bus;
using HJ212_Server;
using HJ212_Server.Model;

Console.WriteLine("Hello, World!");
IGB_Server gb = new GB_Server(new TcpServer("0.0.0.0", 2756));
gb.OnClientConnect += Gb_OnClientConnect;

gb.OnAskSetSystemTime += Gb_OnAskSetSystemTime;
gb.OnUploadRealTimeData += Gb_OnUploadRealTimeData;
gb.OnUploadRunningStateData += Gb_OnUploadRunningStateData;
gb.OnUploadMinuteData += Gb_OnUploadMinuteData;
gb.OnUploadHourData += Gb_OnUploadHourData;
gb.OnUploadDayData += Gb_OnUploadDayData;
gb.OnUploadRunningTimeData += Gb_OnUploadRunningTimeData;
gb.OnUploadAcquisitionDeviceRestartTime += Gb_OnUploadAcquisitionDeviceRestartTime;
gb.OnUploadSN += Gb_OnUploadSN;
gb.OnUploadLog += Gb_OnUploadLog;
gb.OnUploadInfo += Gb_OnUploadInfo;

async Task Gb_OnUploadInfo(Guid clientId, (DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadLog(Guid clientId, (DateTime DataTime, string? PolId, string Log, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadSN(Guid clientId, (DateTime DataTime, string PolId, string SN, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadAcquisitionDeviceRestartTime(Guid clientId, (DateTime dataTime, DateTime restartTime, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadRunningTimeData(Guid clientId, (DateTime dataTime, List<RunningTimeData> data, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadDayData(Guid clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadHourData(Guid clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadMinuteData(Guid clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadRunningStateData(Guid clientId, (DateTime dataTime, List<RunningStateData> data, RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnUploadRealTimeData(Guid clientId, (DateTime dataTime, List<HJ212_Server.Model.RealTimeData> data, HJ212_Server.Model.RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnAskSetSystemTime(Guid clientId, (string PolId, HJ212_Server.Model.RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

await gb.StartAsync();

async Task Gb_OnClientConnect(Guid clientId)
{
    try
    {
        #region c1
        //await gb.SetTimeoutAndRetryAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, 10, 3); 
        #endregion

        #region c2
        //var sysTime = await gb.GetSystemTimeAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        //Console.WriteLine(sysTime);
        #endregion

        #region c3
        //await gb.SetSystemTimeAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001", DateTime.Now);
        #endregion

        #region c5
        //var interval = await gb.GetRealTimeDataIntervalAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        //Console.WriteLine(interval);
        #endregion

        #region c6
        //await gb.SetRealTimeDataIntervalAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, 10);
        #endregion

        #region c7
        //var minInterval = await gb.GetMinuteDataIntervalAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        //Console.WriteLine(minInterval);
        #endregion

        #region c8
        //await gb.SetMinuteDataIntervalAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, 20);
        #endregion

        #region c9
        //await gb.SetNewPWAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "123456");
        #endregion

        #region c10
        //await gb.StartRealTimeDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        #endregion

        #region c11
        //await gb.StopRealTimeDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        #endregion

        #region c12
        //await gb.StartRunningStateDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        #endregion

        #region c13
        //await gb.StopRunningStateDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        #endregion

        #region c20
        //var rs = await gb.GetMinuteDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, DateTime.Now.AddDays(5), DateTime.Now);
        //foreach (var item in rs)
        //{
        //    Console.WriteLine(item.DataTime);
        //    foreach (var data in item.Data)
        //    {
        //        Console.WriteLine($"Name:{data.Name}{(data.Cou is null ? "" : $" Cou:{data.Cou}")}{(data.Min is null ? "" : $" Min:{data.Min}")}{(data.Avg is null ? "" : $" Avg:{data.Avg}")}{(data.Max is null ? "" : $" Max:{data.Max}")}{(data.Flag is null ? "" : $" Flag:{data.Flag}")}");
        //    }
        //}
        #endregion

        #region c21
        //var rs = await gb.GetHourDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, DateTime.Now.AddDays(5), DateTime.Now);
        //foreach (var item in rs)
        //{
        //    Console.WriteLine(item.DataTime);
        //    foreach (var data in item.Data)
        //    {
        //        Console.WriteLine($"Name:{data.Name}{(data.Cou is null ? "" : $" Cou:{data.Cou}")}{(data.Min is null ? "" : $" Min:{data.Min}")}{(data.Avg is null ? "" : $" Avg:{data.Avg}")}{(data.Max is null ? "" : $" Max:{data.Max}")}{(data.Flag is null ? "" : $" Flag:{data.Flag}")}");
        //    }
        //}
        #endregion

        #region c22
        //var rs = await gb.GetDayDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, DateTime.Now.AddDays(5), DateTime.Now);
        //foreach (var item in rs)
        //{
        //    Console.WriteLine(item.DataTime);
        //    foreach (var data in item.Data)
        //    {
        //        Console.WriteLine($"Name:{data.Name}{(data.Cou is null ? "" : $" Cou:{data.Cou}")}{(data.Min is null ? "" : $" Min:{data.Min}")}{(data.Avg is null ? "" : $" Avg:{data.Avg}")}{(data.Max is null ? "" : $" Max:{data.Max}")}{(data.Flag is null ? "" : $" Flag:{data.Flag}")}");
        //    }
        //}
        #endregion

        #region c23
        //var rs = await gb.GetRunningTimeDataAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, DateTime.Now.AddDays(5), DateTime.Now);
        //foreach (var item in rs)
        //{
        //    Console.WriteLine(item.DataTime);
        //    foreach (var data in item.Data)
        //    {
        //        Console.WriteLine($"Name:{data.Name} RT:{data.RT}");
        //    }
        //}
        #endregion

        #region c30
        //await gb.CalibrateAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        #endregion

        #region c31
        //await gb.RealTimeSamplingAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        #endregion

        #region c32
        //await gb.StartCleaningOrBlowbackAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        #endregion

        #region c33
        //await gb.ComparisonSamplingAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        #endregion

        #region c34
        //var rs = await gb.OutOfStandardRetentionSampleAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源);
        //Console.WriteLine(rs);
        #endregion

        #region c35
        //await gb.SetSamplingPeriodAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001", new TimeOnly(12, 30), 2);
        #endregion

        #region c36
        //var rs = await gb.GetSamplingPeriodAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        //Console.WriteLine(rs);
        #endregion

        #region c37
        //var rs = await gb.GetSampleExtractionTimeAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        //Console.WriteLine(rs);
        #endregion

        #region c38
        //var rs = await gb.GetSNAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a34001");
        //Console.WriteLine(rs);
        #endregion

        #region c41
        //var rs = await gb.GetLogInfosAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a1001", DateTime.Now.AddDays(-5), DateTime.Now);
        //foreach (var item in rs)
        //{
        //    Console.WriteLine($"DataTime:{item.DataTime}{(item.PolId == null ? "" : $" PolId:{item.PolId}")} Log:{item.Info}");
        //}
        #endregion

        #region c43
        //var rs = await gb.GetStateInfoAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a1001");
        //Console.WriteLine(rs);
        #endregion

        #region c45
        //var rs = await gb.GetArgumentInfoAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a1001");
        //Console.WriteLine(rs);
        #endregion

        #region c46
        await gb.SetInfoAsync(clientId, "1234567890123456", "123456", ST.大气环境污染源, "a1001", "i13004", "106.5");
        #endregion
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

Console.ReadLine();