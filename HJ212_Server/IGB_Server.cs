using Communication;
using HJ212_Server.Model;
using TopPortLib;

namespace HJ212_Server
{
    public interface IGB_Server
    {
        /// <summary>国标版本</summary>
        Version Version { get; set; }
        /// <summary>设备是否监听</summary>
        public bool IsListened { get; }
        /// <summary>打开监听</summary>
        Task StartAsync();
        /// <summary>关闭监听</summary>
        Task StopAsync();
        event ClientConnectEventHandler OnClientConnect;
        event ClientDisconnectEventHandler OnClientDisconnect;
        event RequestedLogServerEventHandler OnSentData;
        event RespondedLogServerEventHandler OnReceivedData;

        /// <summary>C1设置超时时间及重发次数</summary>
        Task SetTimeoutAndRetryAsync(int clientId, string mn, string pw, ST st, int overTime, int reCount, int timeOut = 5000);

        /// <summary>C2提取现场机时间</summary>
        Task<DateTime> GetSystemTimeAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C3设置现场机时间</summary>
        Task SetSystemTimeAsync(int clientId, string mn, string pw, ST st, string polId, DateTime systemTime, int timeOut = 5000);

        /// <summary>C4现场机时间校准请求</summary>
        event ActivelyPushDataServerEventHandler<(string PolId, RspInfo RspInfo)>? OnAskSetSystemTime;

        /// <summary>C5提取实时数据间隔</summary>
        Task<int> GetRealTimeDataIntervalAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>C6设置实时数据间隔</summary>
        Task SetRealTimeDataIntervalAsync(int clientId, string mn, string pw, ST st, int interval, int timeOut = 5000);

        /// <summary>C7提取分钟数据间隔</summary>
        Task<int> GetMinuteDataIntervalAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>C8设置分钟数据间隔</summary>
        Task SetMinuteDataIntervalAsync(int clientId, string mn, string pw, ST st, int interval, int timeOut = 5000);

        /// <summary>C9设置现场机访问密码</summary>
        Task SetNewPWAsync(int clientId, string mn, string pw, ST st, string newPW, int timeOut = 5000);

        /// <summary>C10取污染物实时数据</summary>
        Task StartRealTimeDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);
    }
}
