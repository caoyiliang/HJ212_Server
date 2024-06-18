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
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RealTimeData> Data, RspInfo RspInfo)>? OnUploadRealTimeData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RunningStateData> Data, RspInfo RspInfo)>? OnUploadRunningStateData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)>? OnUploadMinuteData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)>? OnUploadHourData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)>? OnUploadDayData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, List<RunningTimeData> Data, RspInfo RspInfo)>? OnUploadRunningTimeData;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, DateTime RestartTime, RspInfo RspInfo)>? OnUploadAcquisitionDeviceRestartTime;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, string PolId, string SN, RspInfo RspInfo)>? OnUploadSN;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, string? PolId, string Log, RspInfo RspInfo)>? OnUploadLog;
        public event ActivelyPushDataServerEventHandler<(DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo)>? OnUploadInfo;

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
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetSystemTimeReq, CN9011Rsp, GetSystemTimeRsp, CN9012Rsp>(clientId, new GetSystemTimeReq(mn, pw, st, polId), timeOut);
            return Rsp2.ToList()[0].GetResult().SystemTime;
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
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetReq, CN9011Rsp, GetRealTimeDataIntervalRsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.提取实时数据间隔), timeOut);
            return Rsp2.ToList()[0].GetResult().RtdInterval;
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
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetReq, CN9011Rsp, GetMinuteDataIntervalRsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.提取分钟数据间隔), timeOut);
            return Rsp2.ToList()[0].GetResult().MinInterval;
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
        private async Task UploadRealTimeDataRspEvent(int clientId, (DateTime DataTime, List<RealTimeData> Data, RspInfo RspInfo) rs)
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
        private async Task UploadRunningStateDataRspEvent(int clientId, (DateTime DataTime, List<RunningStateData> Data, RspInfo RspInfo) rs)
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
        private async Task UploadMinuteDataRspEvent(int clientId, (DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo) rs)
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
        private async Task UploadHourDataRspEvent(int clientId, (DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo) rs)
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
        private async Task UploadDayDataRspEvent(int clientId, (DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo) rs)
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
        private async Task UploadRunningTimeDataRspEvent(int clientId, (DateTime DataTime, List<RunningTimeData> Data, RspInfo RspInfo) rs)
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
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadMinuteDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物分钟数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.DataTime, itemRs.Data));
            }
            return rs;
        }
        #endregion

        #region c21
        public async Task<List<HistoryData>> GetHourDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadHourDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物小时数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.DataTime, itemRs.Data));
            }
            return rs;
        }
        #endregion

        #region c22
        public async Task<List<HistoryData>> GetDayDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetStatisticsDataReq, CN9011Rsp, UploadDayDataRsp, CN9012Rsp>(clientId, new GetStatisticsDataReq(mn, pw, st, CN_Server.取污染物日历史数据, beginTime, endTime), timeOut);
            var rs = new List<HistoryData>();
            foreach (var item in Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.DataTime, itemRs.Data));
            }
            return rs;
        }
        #endregion

        #region c23
        public async Task<List<RunningTimeHistory>> GetRunningTimeDataAsync(int clientId, string mn, string pw, ST st, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetRunningTimeDataReq, CN9011Rsp, UploadRunningTimeDataRsp, CN9012Rsp>(clientId, new GetRunningTimeDataReq(mn, pw, st, beginTime, endTime), timeOut);
            var rs = new List<RunningTimeHistory>();
            foreach (var item in Rsp2)
            {
                var itemRs = item.GetResult();
                rs.Add(new(itemRs.DataTime, itemRs.Data));
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

        #region c31
        public async Task RealTimeSamplingAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<RealTimeSamplingReq, CN9011Rsp, CN9012Rsp>(clientId, new RealTimeSamplingReq(mn, pw, st, polId), timeOut);
        }
        #endregion

        #region c32
        public async Task StartCleaningOrBlowbackAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<StartCleaningOrBlowbackReq, CN9011Rsp, CN9012Rsp>(clientId, new StartCleaningOrBlowbackReq(mn, pw, st, polId), timeOut);
        }
        #endregion

        #region c33
        public async Task ComparisonSamplingAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<ComparisonSamplingReq, CN9011Rsp, CN9012Rsp>(clientId, new ComparisonSamplingReq(mn, pw, st, polId), timeOut);
        }
        #endregion

        #region c34
        public async Task<(DateTime DataTime, string VaseNo)> OutOfStandardRetentionSampleAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetReq, CN9011Rsp, OutOfStandardRetentionSampleRsp, CN9012Rsp>(clientId, new GetReq(mn, pw, st, CN_Server.超标留样), timeOut);
            var rs2 = Rsp2.ToList()[0].GetResult();
            return (rs2.DataTime, rs2.VaseNo);
        }
        #endregion

        #region c35
        public async Task SetSamplingPeriodAsync(int clientId, string mn, string pw, ST st, string polId, TimeOnly cstartTime, int ctime, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetSamplingPeriodReq, CN9011Rsp, CN9012Rsp>(clientId, new SetSamplingPeriodReq(mn, pw, st, polId, cstartTime, ctime), timeOut);
        }
        #endregion

        #region c36
        public async Task<(TimeOnly CstartTime, int CTime)> GetSamplingPeriodAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetSamplingPeriodReq, CN9011Rsp, GetSamplingPeriodRsp, CN9012Rsp>(clientId, new GetSamplingPeriodReq(mn, pw, st, polId), timeOut);
            var rs2 = Rsp2.ToList()[0].GetResult();
            return (rs2.CstartTime, rs2.CTime);
        }
        #endregion

        #region c37
        public async Task<int> GetSampleExtractionTimeAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetSampleExtractionTimeReq, CN9011Rsp, GetSampleExtractionTimeRsp, CN9012Rsp>(clientId, new GetSampleExtractionTimeReq(mn, pw, st, polId), timeOut);
            var rs2 = Rsp2.ToList()[0].GetResult();
            return rs2.Stime;
        }
        #endregion

        #region c38
        public async Task<string> GetSNAsync(int clientId, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetSNReq, CN9011Rsp, GetSNRsp, CN9012Rsp>(clientId, new GetSNReq(mn, pw, st, polId), timeOut);
            var rs2 = Rsp2.ToList()[0].GetResult();
            return rs2.SN;
        }
        #endregion

        #region c39
        private async Task UploadSNRspEvent(int clientId, (DateTime DataTime, string PolId, string SN, RspInfo RspInfo) rs)
        {
            if (OnUploadSN is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadSN.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c40
        private async Task UploadLogRspEvent(int clientId, (DateTime DataTime, string? PolId, string Log, RspInfo RspInfo) rs)
        {
            if (OnUploadLog is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadLog.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c41
        public async Task<List<LogInfo>> GetLogInfosAsync(int clientId, string mn, string pw, ST st, string? polId, DateTime beginTime, DateTime endTime, int timeOut = 5000)
        {
            var (_, Rsp2, _) = await _condorPort.RequestAsync<GetLogInfosReq, CN9011Rsp, UploadLogRsp, CN9012Rsp>(clientId, new GetLogInfosReq(mn, pw, st, polId, beginTime, endTime), timeOut);
            var rs = new List<LogInfo>();
            foreach (var item in Rsp2)
            {
                var iRs = item.GetResult();
                rs.Add(new(iRs.Log, iRs.DataTime) { PolId = iRs.PolId });
            }
            return rs;
        }
        #endregion

        #region c42
        private async Task UploadInfoRspEvent(int clientId, (DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo) rs)
        {
            if (OnUploadInfo is not null)
            {
                if (!rs.RspInfo.Flag.HasValue || NeedReturn(rs.RspInfo.Flag))
                    await _condorPort.SendAsync(clientId, new DataReq(rs.RspInfo));
                await OnUploadInfo.Invoke(clientId, rs);
            }
        }
        #endregion

        #region c43
        public async Task<string> GetStateInfoAsync(int client, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetInfoReq, CN9011Rsp, UploadInfoRsp, CN9012Rsp>(client, new GetInfoReq(mn, pw, st, polId, "i12001"), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().DeviceInfos[0].Info;
        }
        #endregion

        #region c45
        public async Task<string> GetArgumentInfoAsync(int client, string mn, string pw, ST st, string polId, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetInfoReq, CN9011Rsp, UploadInfoRsp, CN9012Rsp>(client, new GetInfoReq(mn, pw, st, polId, "i13004"), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().DeviceInfos[0].Info;
        }
        #endregion

        #region c46
        public async Task SetInfoAsync(int client, string mn, string pw, ST st, string polId, string infoId, string info, int timeOut = 5000)
        {
            await _condorPort.RequestAsync<SetInfoReq, CN9011Rsp, CN9012Rsp>(client, new SetInfoReq(mn, pw, st, polId, infoId, info), timeOut);
        }
        #endregion
    }
}
