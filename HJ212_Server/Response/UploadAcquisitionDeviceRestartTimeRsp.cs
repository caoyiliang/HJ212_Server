using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadAcquisitionDeviceRestartTimeRsp : IAsyncResponse_Server<(DateTime dataTime, DateTime restartTime, RspInfo RspInfo)>
    {
        private DateTime _dataTime;
        private DateTime _restartTime;
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
                throw new ArgumentException($"GB_Server HJ212 UploadAcquisitionDeviceRestartTime DataTime Error");
            }
            if (!DateTime.TryParseExact(dataList.SingleOrDefault(item => item.Contains("RestartTime"))?.Split('=')[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _restartTime))
            {
                throw new ArgumentException($"GB_Server HJ212 UploadAcquisitionDeviceRestartTime RestartTime Error");
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传数采仪开机时间}")).Any(), default);
        }

        public (DateTime dataTime, DateTime restartTime, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _restartTime, _rspInfo);
        }
    }
}
