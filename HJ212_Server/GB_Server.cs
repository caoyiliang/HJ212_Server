using Communication;
using Communication.Interfaces;
using HJ212_Server.Model;
using HJ212_Server.Request;
using HJ212_Server.Response;
using LogInterface;
using Parser.Parsers;
using System.Text;
using TopPortLib;
using TopPortLib.Interfaces;
using Utils;

namespace HJ212_Server
{
    public class GB_Server : IGB_Server
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<GB_Server>();
        private readonly ICondorPort _condorPort;

        internal static Version _version;
        public Version Version { get => _version; set => _version = value; }

        public bool IsListened { get; private set; }

        public event ClientConnectEventHandler? OnClientConnect { add => _condorPort.OnClientConnect += value; remove => _condorPort.OnClientConnect -= value; }
        public event ClientDisconnectEventHandler? OnClientDisconnect { add => _condorPort.OnClientDisconnect += value; remove => _condorPort.OnClientDisconnect -= value; }
        public event RequestedLogServerEventHandler? OnSentData { add => _condorPort.OnSentData += value; remove => _condorPort.OnSentData -= value; }
        public event RespondedLogServerEventHandler? OnReceivedData { add => _condorPort.OnReceivedData += value; remove => _condorPort.OnReceivedData -= value; }

        public event ActivelyPushDataServerEventHandler<(string PolId, RspInfo RspInfo)>? OnAskSetSystemTime;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RealTimeData> data, RspInfo RspInfo)>? OnUploadRealTimeData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RunningStateData> data, RspInfo RspInfo)>? OnUploadRunningStateData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)>? OnUploadMinuteData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)>? OnUploadHourData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo)>? OnUploadDayData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, List<RunningTimeData> data, RspInfo RspInfo)>? OnUploadRunningTimeData;
        public event ActivelyPushDataServerEventHandler<(DateTime dataTime, DateTime restartTime, RspInfo RspInfo)>? OnUploadAcquisitionDeviceRestartTime;

        public GB_Server(IPhysicalPort_Server physicalPort_Server, Version version = Version.HJT212_2017)
        {
            Version = version;
            _condorPort = new CondorPort(this, new TopPort_Server(physicalPort_Server, async () => await Task.FromResult(new FootParser([0x0d, 0x0a]))))
            {
                CheckEvent = async (byte[] bytes) =>
                {
                    var data = bytes.Skip(6).ToArray();
                    var dstr = Encoding.ASCII.GetString(data).Trim();
                    return await Task.FromResult(StringByteUtils.BytesToString(CRC.GBcrc16(data, data.Length - 6)).Replace(" ", "") == dstr[^4..]);
                }
            };
            _condorPort.OnSentData += CondorPort_OnSentData;
            _condorPort.OnReceivedData += CondorPort_OnReceivedData;
        }

        private async Task CondorPort_OnReceivedData(int clientId, byte[] data)
        {
            _logger.Trace($"GB_Server {await GetClientInfos(clientId)} Rec:<-- {Encoding.UTF8.GetString(data)}\n{StringByteUtils.BytesToString(data)}");
        }

        private async Task CondorPort_OnSentData(int clientId, byte[] data)
        {
            _logger.Trace($"GB_Server {await GetClientInfos(clientId)} Send:--> {Encoding.UTF8.GetString(data)}");
        }

        internal async Task<string?> GetClientInfos(int clientId)
        {
            return await _condorPort.PhysicalPort.GetClientInfos(clientId);
        }

        public async Task StartAsync()
        {
            await _condorPort.StartAsync();
            IsListened = true;
        }

        public async Task StopAsync()
        {
            await _condorPort.StopAsync();
            IsListened = false;
        }

        internal static string GetGbCmd(string rs)
        {
            var brs = Encoding.ASCII.GetBytes(rs);
            return $"##{rs.Length.ToString().PadLeft(4, '0')}{rs}{StringByteUtils.BytesToString(CRC.GBcrc16(brs, brs.Length)).Replace(" ", "")}\r\n";
        }

        internal static bool NeedReturn(int? number)
        {
            if (number is null) return true;
            return (number & 1) == 1;
        }

        #region c1
        public async Task SetTimeoutAndRetryAsync(int clientId, string mn, string pw, ST st, int overTime, int reCount, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetTimeOutAndRetryReq, CN9011Rsp, CN9012Rsp>(clientId, new SetTimeOutAndRetryReq(mn, pw, st, overTime, reCount), timeOut);
        }
        #endregion

        #region c2
        public async Task<DateTime> GetSystemTimeAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetSystemTimeReq, CN9011Rsp, GetSystemTimeRsp, CN9012Rsp>(clientId, new GetSystemTimeReq(mn, pw, st, polId), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().SystemTime;
        }
        #endregion

        #region c3
        public async Task SetSystemTimeAsync(int clientId, string mn, string pw, ST st, string polId, DateTime systemTime, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetSystemTimeReq, CN9011Rsp, CN9012Rsp>(clientId, new SetSystemTimeReq(mn, pw, st, polId, systemTime), timeOut);
        }
        #endregion

        #region c4
        private async Task AskSetSystemTimeRspEvent(int clientId, (string PolId, RspInfo RspInfo) rs)
        {
            if (OnAskSetSystemTime is not null)
            {
                await _condorPort.SendAsync(clientId, new ResponseReq(rs.RspInfo));
                await OnAskSetSystemTime.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c5
        public async Task<int> GetRealTimeDataIntervalAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetReq, CN9011Rsp, GetRealTimeDataIntervalRsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.提取实时数据间隔), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().RtdInterval;
        }
        #endregion

        #region c6
        public async Task SetRealTimeDataIntervalAsync(int clientId, string mn, string pw, ST st, int interval, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetRealTimeDataIntervalReq, CN9011Rsp, CN9012Rsp>(clientId, new SetRealTimeDataIntervalReq(mn, pw, st, interval), timeOut);
        }
        #endregion

        #region c7
        public async Task<int> GetMinuteDataIntervalAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetReq, CN9011Rsp, GetMinuteDataIntervalRsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.提取分钟数据间隔), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().MinInterval;
        }
        #endregion

        #region c8
        public async Task SetMinuteDataIntervalAsync(int clientId, string mn, string pw, ST st, int interval, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetMinuteDataIntervalReq, CN9011Rsp, CN9012Rsp>(clientId, new SetMinuteDataIntervalReq(mn, pw, st, interval), timeOut);
        }
        #endregion

        #region c9
        public async Task SetNewPWAsync(int clientId, string mn, string pw, ST st, string newPW, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetNewPWReq, CN9011Rsp, CN9012Rsp>(clientId, new SetNewPWReq(mn, pw, st, newPW), timeOut);
        }
        #endregion

        #region c10
        public async Task StartRealTimeDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<GetReq, CN9011Rsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.取污染物实时数据), timeOut);
        }
        #endregion

        #region c11
        public async Task StopRealTimeDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<GetReq, CN9013Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.停止察看污染物实时数据), timeOut);
        }
        #endregion

        #region c12
        public async Task StartRunningStateDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<GetReq, CN9011Rsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.取设备运行状态数据), timeOut);
        }
        #endregion

        #region c13
        public async Task StopRunningStateDataAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<GetReq, CN9013Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.停止察看设备运行状态), timeOut);
        }
        #endregion

        #region c14、c25
        private async Task UploadRealTimeDataRspEvent(int clientId, (DateTime dataTime, List<RealTimeData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadRealTimeData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadRealTimeData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c15
        private async Task UploadRunningStateDataRspEvent(int clientId, (DateTime dataTime, List<RunningStateData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadRunningStateData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadRunningStateData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c16、c26
        private async Task UploadMinuteDataRspEvent(int clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadMinuteData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadMinuteData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c17、c27
        private async Task UploadHourDataRspEvent(int clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadHourData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadHourData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c18、c28
        private async Task UploadDayDataRspEvent(int clientId, (DateTime dataTime, List<StatisticsData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadDayData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadDayData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c19
        private async Task UploadRunningTimeDataRspEvent(int clientId, (DateTime dataTime, List<RunningTimeData> data, RspInfo RspInfo) rs)
        {
            if (OnUploadRunningTimeData is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadRunningTimeData.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c20
        public async Task<List<HistoryData>> GetMinuteDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var reqRs = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadMinuteDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物分钟数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in reqRs.Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.dataTime, itemRs.data));
            }
            return rs;
        }
        #endregion

        #region c21
        public async Task<List<HistoryData>> GetHourDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var reqRs = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadHourDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物小时数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in reqRs.Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.dataTime, itemRs.data));
            }
            return rs;
        }
        #endregion

        #region c22
        public async Task<List<HistoryData>> GetDayDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var reqRs = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadDayDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物日历史数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in reqRs.Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.dataTime, itemRs.data));
            }
            return rs;
        }
        #endregion

        #region c23
        public async Task<List<RunningTimeHistory>> GetRunningTimeDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var reqRs = await _condorPort.RequestAsync<GetRunningTimeDataReq, CN9011Rsp, UploadRunningTimeDataRsp, CN9012Rsp>(clientId, new GetRunningTimeDataReq(mn, pw, st, beginTime, endTime), timeOut);
            var rs = new List<RunningTimeHistory>();
            foreach (var item in reqRs.Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.dataTime, itemRs.data));
            }
            return rs;
        }
        #endregion

        #region c24
        private async Task UploadAcquisitionDeviceRestartTimeRspEvent(int clientId, (DateTime dataTime, DateTime restartTime, RspInfo RspInfo) rs)
        {
            if (OnUploadAcquisitionDeviceRestartTime is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadAcquisitionDeviceRestartTime.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c30
        public async Task CalibrateAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<CalibrateReq, CN9011Rsp, CN9012Rsp>(clientId, new CalibrateReq(mn, pw, st, polId), timeOut);
        }
        #endregion
    }
}
