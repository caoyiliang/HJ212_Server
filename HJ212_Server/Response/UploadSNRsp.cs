using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadSNRsp : IAsyncResponse_Server<(DateTime DataTime, string PolId, string SN, RspInfo RspInfo)>
    {
        private DateTime _dataTime;
        private string _polId = null!;
        private string _sn = null!;
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
            var dataList = data[1].Split([";", "&&"], StringSplitOptions.RemoveEmptyEntries).Where(item => item.Contains('='));
            if (!DateTime.TryParseExact(dataList.SingleOrDefault(item => item.Contains("DataTime"))?.Split('=')[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _dataTime))
            {
                throw new ArgumentException($"GB_Server HJ212 UploadSN DataTime Error");
            }
            _polId = dataList.SingleOrDefault(item => item.Contains("PolId"))?.Split('=')[1] ?? throw new ArgumentException($"GB_Server HJ212 UploadSN PolId Error");
            _sn = dataList.SingleOrDefault(item => item.Contains("-SN"))?.Split('=')[1] ?? throw new ArgumentException($"GB_Server HJ212 UploadSN SN Error");
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes);
            var rs = str.Split(';');
            return (str.Contains("DataTime") && rs.Where(item => item.Contains($"CN={(int)CN_Client.上传设备唯一标识}")).Any(), default);
        }

        public (DateTime DataTime, string PolId, string SN, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _polId, _sn, _rspInfo);
        }
    }
}
