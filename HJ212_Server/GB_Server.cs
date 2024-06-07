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
            _logger.Trace($"GB_Server {await GetClientInfos(clientId)} Rec:<-- {StringByteUtils.BytesToString(data)}");
        }

        private async Task CondorPort_OnSentData(int clientId, byte[] data)
        {
            _logger.Trace($"GB_Server {await GetClientInfos(clientId)} Send:--> {StringByteUtils.BytesToString(data)}");
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
            await _condorPort.SendAsync(clientId, new ResponseReq(rs.RspInfo));
            if (OnAskSetSystemTime is not null)
                await OnAskSetSystemTime.Invoke(clientId, rs);
        }
        #endregion

        #region c5
        public async Task<int> GetRealTimeDataIntervalAsync(int clientId, string mn, string pw, ST st, int timeOut = 5000)
        {
            var rs = await _condorPort.RequestAsync<GetRealTimeDataIntervalReq, CN9011Rsp, GetRealTimeDataIntervalRsp, CN9012Rsp>(clientId, new GetRealTimeDataIntervalReq(mn, pw, st), timeOut);
            return rs.Rsp2.ToList()[0].GetResult().RtdInterval;
        }
        #endregion
    }
}
