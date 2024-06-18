using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadInfoRsp : IAsyncResponse_Server<(DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo)>, IRspEnumerable
    {
        private DateTime _dataTime;
        private string _polId = null!;
        private List<DeviceInfo> _DeviceInfos = [];
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
                throw new ArgumentException($"GB_Server HJ212 UploadInfo DataTime Error");
            }
            _polId = dataList.SingleOrDefault(item => item.Contains("PolId"))?.Split('=')[1] ?? throw new ArgumentException($"GB_Server HJ212 UploadInfo PolId Error");
            foreach (var item in dataList.Where(item => item.Contains("-Info")))
            {
                _DeviceInfos.Add(new(item.Split('-')[0], item.Split('=')[1]));
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes);
            var rs = str.Split(';');
            return ((!str.Contains("i11001")) && rs.Where(item => item.Contains($"CN={(int)CN_Client.上传现场机信息}")).Any(), default);
        }

        public (DateTime DataTime, string PolId, List<DeviceInfo> DeviceInfos, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _polId, _DeviceInfos, _rspInfo);
        }

        public async Task<bool> IsFinish() => await Task.FromResult(true);

        public bool NeedCheck()
        {
            return false;
        }
    }
}
