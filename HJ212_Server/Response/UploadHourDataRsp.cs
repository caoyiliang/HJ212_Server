﻿using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadHourDataRsp : IAsyncResponse_Server<(DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo)>, IRspEnumerable
    {
        private DateTime _dataTime;
        private readonly List<StatisticsData> _data = [];
        private readonly RspInfo _rspInfo = new();
        private int? PNUM;
        private int? PNO;
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
            if (int.TryParse(dataInfo.FirstOrDefault(item => item.Contains("PNUM"))?.Split('=')[1], out var pnum))
            {
                PNUM = pnum;
            }
            if (int.TryParse(dataInfo.FirstOrDefault(item => item.Contains("PNO"))?.Split('=')[1], out var pno))
            {
                PNO = pno;
            }
            var dataList = data[1].Split([";", "&&"], StringSplitOptions.RemoveEmptyEntries).Where(item => item.Contains('='));
            if (!DateTime.TryParseExact(dataList.SingleOrDefault(item => item.Contains("DataTime"))?.Split('=')[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _dataTime))
            {
                throw new ArgumentException($"GB_Server HJ212 UploadHourData DataTime Error");
            }
            foreach (var item in dataList)
            {
                if (item.Contains("DataTime")) continue;
                var rtd = item.Split(',');
                var rtdata = new StatisticsData(rtd[0].Split('-')[0]);
                foreach (var prop in rtd)
                {
                    if (prop.Contains("Cou")) rtdata.Cou = prop.Split('=')[1];
                    else if (prop.Contains("Min")) rtdata.Min = prop.Split('=')[1];
                    else if (prop.Contains("Avg")) rtdata.Avg = prop.Split('=')[1];
                    else if (prop.Contains("Max")) rtdata.Max = prop.Split('=')[1];
                    else if (prop.Contains("Flag")) rtdata.Flag = prop.Split('=')[1];
                    else if (prop.Contains("Data")) rtdata.Data = prop.Split('=')[1];
                }
                _data.Add(rtdata);
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传污染物小时数据}")).Any(), default);
        }

        public (DateTime DataTime, List<StatisticsData> Data, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _data, _rspInfo);
        }

        public async Task<bool> IsFinish() => await Task.FromResult(PNUM == null || PNO == null || PNO == PNUM);

        public bool NeedCheck()
        {
            return false;
        }
    }
}
