﻿using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class UploadLogRsp : IAsyncResponse_Server<(DateTime DataTime, string? PolId, string Log, RspInfo RspInfo)>, IRspEnumerable
    {
        private DateTime _dataTime;
        private string? _polId;
        private string _log = null!;
        private readonly RspInfo _rspInfo = new();
        private int? PNUM;
        private int? PNO;
        public async Task AnalyticalData(string clientInfo, byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes.Skip(6).ToArray());
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
                throw new ArgumentException($"GB_Server HJ212 UploadLog DataTime Error");
            }
            _polId = dataList.SingleOrDefault(item => item.Contains("PolId"))?.Split('=')[1];
            _log = dataList.SingleOrDefault(item => item.Contains("-Info"))?.Split('=')[1].Replace("//", "") ?? throw new ArgumentException($"GB_Server HJ212 UploadLog Info Error");
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            var rs = str.Split(';');
            return (str.Contains("i11001") && rs.Where(item => item.Contains($"CN={(int)CN_Client.上传现场机信息}")).Any(), default);
        }

        public (DateTime DataTime, string? PolId, string Log, RspInfo RspInfo) GetResult()
        {
            return (_dataTime, _polId, _log, _rspInfo);
        }

        public async Task<bool> IsFinish() => await Task.FromResult(PNUM == null || PNO == null || PNO == PNUM);

        public bool NeedCheck()
        {
            return false;
        }
    }
}
