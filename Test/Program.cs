// See https://aka.ms/new-console-template for more information
using Communication.Bus;
using HJ212_Server;

Console.WriteLine("Hello, World!");
IGB_Server gb = new GB_Server(new TcpServer("0.0.0.0", 2756));
gb.OnClientConnect += Gb_OnClientConnect;

gb.OnAskSetSystemTime += Gb_OnAskSetSystemTime;
gb.OnUploadRealTimeData += Gb_OnUploadRealTimeData;

async Task Gb_OnUploadRealTimeData(int clientId, (DateTime dataTime, List<HJ212_Server.Model.RealTimeData> data, HJ212_Server.Model.RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

async Task Gb_OnAskSetSystemTime(int clientId, (string PolId, HJ212_Server.Model.RspInfo RspInfo) objects)
{
    await Task.CompletedTask;
}

await gb.StartAsync();

async Task Gb_OnClientConnect(int clientId)
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
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

Console.ReadLine();