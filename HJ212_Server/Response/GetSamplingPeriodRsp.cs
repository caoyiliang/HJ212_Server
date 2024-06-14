using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class GetSamplingPeriodRsp : IAsyncResponse_Server<(TimeOnly CstartTime, int CTime, RspInfo RspInfo)>, IRspEnumerable
    {
        private TimeOnly _cstartTime;
        private int _cTime;
        private readonly RspInfo _rspInfo = new();
        public async Task AnalyticalData(string clientInfo, byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes.Skip(6).ToArray());
            var data = str.Split("CP=&&");
            var dataInfo = data[0].Split([";", ",", "&&"], StringSplitOptions.RemoveEmptyEntries);
            _rspInfo.QN = dataInfo.FirstOrDefault(item => item.Contains("QN"));
            _rspInfo.ST = dataInfo.FirstOrDefault(item => item.Contains("ST"));
            _rspInfo.PW = dataInfo.FirstOrDefault(item => item.Contains("PW"));
            _rspInfo.MN = dataInfo.FirstOrDefault(item => item.Contains("MN"));
            if (int.TryParse(dataInfo.FirstOrDefault(item => item.Contains("Flag"))?.Split('=')[1], out var flag))
            {
                _rspInfo.Flag = flag;
            }
            var dataList = data[1].Split([";", ",", "&&"], StringSplitOptions.RemoveEmptyEntries).Where(item => item.Contains('='));
            if (!TimeOnly.TryParseExact(dataList.SingleOrDefault(item => item.Contains("CstartTime"))?.Split('=')[1], "HHmmss", null, System.Globalization.DateTimeStyles.None, out _cstartTime))
            {
                throw new ArgumentException($"GB_Server HJ212 GetSamplingPeriod CstartTime Error");
            }
            if (!int.TryParse(dataList.SingleOrDefault(item => item.Contains("CTime"))?.Split('=')[1], out _cTime))
            {
                throw new ArgumentException($"GB_Server HJ212 GetSamplingPeriod CTime Error");
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            var qn = rs.SingleOrDefault(item => item.Contains("QN"))?.Split('=')[1];
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传采样时间周期}")).Any(), qn == null ? default : Encoding.ASCII.GetBytes(qn));
        }

        public (TimeOnly CstartTime, int CTime, RspInfo RspInfo) GetResult()
        {
            return (_cstartTime, _cTime, _rspInfo);
        }

        public async Task<bool> IsFinish()
        {
            return await Task.FromResult(true);
        }

        public bool NeedCheck()
        {
            return true;
        }
    }
}
