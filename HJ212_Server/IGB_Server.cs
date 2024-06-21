using Communication;
using HJ212_Server.Model;
using ProtocolInterface;

namespace HJ212_Server
{
    /// <summary>
    /// 国标服务端
    /// </summary>
    public interface IGB_Server : IProtocol_Server
    {
        /// <summary>国标版本</summary>
        Version Version { get; set; }

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
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RealTimeData> Data, RspInfo RspInfo)> OnUploadRealTimeData;

        /// <summary>C15上传设备运行状态数据</summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RunningStateData> Data, RspInfo RspInfo)> OnUploadRunningStateData;

        /// <summary>
        /// C16上传污染物分钟数据
        /// (C26上传噪声声级分钟数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)> OnUploadMinuteData;

        /// <summary>
        /// C17上传污染物小时数据
        /// (C27上传噪声声级小时数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)> OnUploadHourData;

        /// <summary>
        /// C18上传污染物日历史数据
        /// (C28上传噪声声级日历史数据 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)> OnUploadDayData;

        /// <summary>C19上传设备运行时间日历史数据</summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RunningTimeData> Data, RspInfo RspInfo)> OnUploadRunningTimeData;

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
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, DateTime RestartTime, RspInfo RspInfo)> OnUploadAcquisitionDeviceRestartTime;

        /// <summary>C30零点校准量程校准</summary>
        Task CalibrateAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C31即时采样</summary>
        Task RealTimeSamplingAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C32启动清洗/反吹</summary>
        Task StartCleaningOrBlowbackAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C33比对采样</summary>
        Task ComparisonSamplingAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C34超标留样</summary>
        Task<(DateTime DataTime, string VaseNo)> OutOfStandardRetentionSampleAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000);

        /// <summary>C35设置采样时间周期(单位：小时)</summary>
        Task SetSamplingPeriodAsync(int clientId, string mn, string pw, ST st, string polId, TimeOnly cstartTime, int ctime, int timeOut = 5000);

        /// <summary>C36提取采样时间周期(单位：小时)</summary>
        Task<(TimeOnly CstartTime, int CTime)> GetSamplingPeriodAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C37提取出样时间(单位：分钟)</summary>
        Task<int> GetSampleExtractionTimeAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C38提取设备唯一标识</summary>
        Task<string> GetSNAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C39上传设备唯一标识</summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, string PolId, string SN, RspInfo RspInfo)> OnUploadSN;

        /// <summary>C40上传现场机信息（日志）</summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, string? PolId, string Log, RspInfo RspInfo)> OnUploadLog;

        /// <summary>C41提取现场机信息（日志）</summary>
        Task<List<LogInfo>> GetLogInfosAsync(int clientId, string mn, string pw, ST st, string? polId, DateTime beginTime, DateTime endTime, int timeOut = 5000);

        /// <summary>
        /// C42上传现场机信息（状态）
        /// (C44上传现场机信息（参数） 同)
        /// </summary>
        event ActivelyPushDataServerEventHandler<(DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo)> OnUploadInfo;

        /// <summary>C43提取现场机信息（状态）</summary>
        Task<string> GetStateInfoAsync(int client, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C45提取现场机信息（参数）</summary>
        Task<string> GetArgumentInfoAsync(int client, string mn, string pw, ST st, string polId, int timeOut = 5000);

        /// <summary>C46设置现场机参数</summary>
        Task SetInfoAsync(int client, string mn, string pw, ST st, string polId, string infoId, string info, int timeOut = 5000);
    }
}
