using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class GetMinuteDataIntervalRsp : IAsyncResponse_Server<(int MinInterval, RspInfo RspInfo)>, IRspEnumerable
    {
        private int _minInterval;
        private readonly RspInfo _rspInfo = new();
        public async Task AnalyticalData(string clientInfo, byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes.Skip(6).ToArray());
            var datalist = str.Split([";", ",", "&&"], StringSplitOptions.RemoveEmptyEntries).Where(item => item.Contains('=') && !item.Contains("CP"));
            _rspInfo.QN = datalist.FirstOrDefault(item => item.Contains("QN"));
            _rspInfo.ST = datalist.FirstOrDefault(item => item.Contains("ST"));
            _rspInfo.PW = datalist.FirstOrDefault(item => item.Contains("PW"));
            _rspInfo.MN = datalist.FirstOrDefault(item => item.Contains("MN"));
            if (!int.TryParse(datalist.SingleOrDefault(item => item.Contains("MinInterval"))?.Split('=')[1], out _minInterval))
            {
                throw new ArgumentException($"GB_Server:{clientInfo} HJ212 GetMinuteDataInterval MinInterval Error");
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            var qn = rs.SingleOrDefault(item => item.Contains("QN"))?.Split('=')[1];
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传分钟数据间隔}")).Any(), qn == null ? default : Encoding.ASCII.GetBytes(qn));
        }

        public (int MinInterval, RspInfo RspInfo) GetResult()
        {
            return (_minInterval, _rspInfo);
        }

        public async Task<bool> IsFinish()
        {
            return await Task.FromResult(true);
        }
    }
}
