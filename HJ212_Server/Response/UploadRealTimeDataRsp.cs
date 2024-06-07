using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadRealTimeDataRsp : IAsyncResponse_Server<(DateTime dataTime, List<RealTimeData> data, RspInfo RspInfo)>
    {
        private DateTime _dataTime;
        private List<RealTimeData> _data = [];
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
                throw new ArgumentException($"GB_Server HJ212 UploadRealTimeData DataTime Error");
            }
            foreach (var item in dataList)
            {
                if (item.Contains("DataTime")) continue;
                var rtd = item.Split(',');
                var rtdata = new RealTimeData(rtd[0].Split('-')[0]);
                foreach (var prop in rtd)
                {
                    if (prop.Contains("Rtd")) rtdata.Rtd = prop.Split('=')[1];
                    else if (prop.Contains("Flag")) rtdata.Flag = prop.Split('=')[1];
                    else if (prop.Contains("SampleTime")) rtdata.SampleTime = prop.Split('=')[1];
                    else if (prop.Contains("EFlag")) rtdata.EFlag = prop.Split('=')[1];
                }
                _data.Add(rtdata);
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传污染物实时数据}")).Any(), default);
        }

        public (DateTime dataTime, List<RealTimeData> data, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _data, _rspInfo);
        }
    }
}
