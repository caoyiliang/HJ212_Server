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
        event ActivelyPushDataServerEventHandler<(string PolId, RspInfo RspInfo)> OnAskSetSystemTime;

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

        /// <summary>C11停止察看污染物实时数据</summary>
        Task StopRealTimeDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>C12取设备运行状态数据</summary>
        Task StartRunningStateDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>C13停止察看设备运行状态</summary>
        Task StopRunningStateDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>
        /// C14上传污染物实时数据
        /// (C25上传噪声声级实时数据 同)
        /// (C29上传工况实时数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RealTimeData> data, RspInfo RspInfo)> OnUploadRealTimeData;

        /// <summary>C15上传设备运行状态数据</summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RunningStateData> data, RspInfo RspInfo)> OnUploadRunningStateData;

        /// <summary>
        /// C16上传污染物分钟数据
        /// (C26上传噪声声级分钟数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)> OnUploadMinuteData;

        /// <summary>
        /// C17上传污染物小时数据
        /// (C27上传噪声声级小时数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)> OnUploadHourData;

        /// <summary>
        /// C18上传污染物日历史数据
        /// (C28上传噪声声级日历史数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)> OnUploadDayData;

        /// <summary>C19上传设备运行时间日历史数据</summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RunningTimeData> data, RspInfo RspInfo)> OnUploadRunningTimeData;

        /// <summary>
        /// C20取污染物分钟历史数据
        /// 遵循C47-C50的规则
        /// </summary>
        Task<List<HistoryData>> GetMinuteDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000);

        /// <summary>
        /// C21取污染物小时历史数据
        /// 遵循C47-C50的规则
        /// </summary>
        Task<List<HistoryData>> GetHourDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000);

        /// <summary>
        /// C22取污染物日历史数据
        /// 遵循C47-C50的规则
        /// </summary>
        Task<List<HistoryData>> GetDayDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000);

        /// <summary>
        /// C23取设备运行时间日历史数据
        /// 遵循C47-C50的规则
        /// </summary>
        Task<List<RunningTimeHistory>> GetRunningTimeDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000);

        /// <summary>C24上传数采仪开机时间</summary>
        event ActivelyPushDataServerEventHandler<(DateTime dataTime, DateTime restartTime, RspInfo RspInfo)> OnUploadAcquisitionDeviceRestartTime;

        /// <summary>C30零点校准量程校准</summary>
        Task CalibrateAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);
    }
}
