﻿using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Response
{
    internal class GetSystemTimeRsp : IAsyncResponse_Server<(string PolId, DateTime SystemTime, RspInfo RspInfo)>, IRspEnumerable
    {
        private string _polId = null!;
        private DateTime _systemTime;
        private readonly RspInfo _rspInfo = new();
        public async Task AnalyticalData(string clientInfo, byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes.Skip(6).ToArray());
            var datalist = str.Split([";", ",", "&&"], StringSplitOptions.RemoveEmptyEntries).Where(item => item.Contains('=') && !item.Contains("CP"));
            _rspInfo.QN = datalist.FirstOrDefault(item => item.Contains("QN"));
            _rspInfo.ST = datalist.FirstOrDefault(item => item.Contains("ST"));
            _rspInfo.PW = datalist.FirstOrDefault(item => item.Contains("PW"));
            _rspInfo.MN = datalist.FirstOrDefault(item => item.Contains("MN"));
            _polId = datalist.SingleOrDefault(item => item.Contains("PolId"))?.Split('=')[1] ?? throw new ArgumentException($"GB_Server:{clientInfo} HJ212 GetSystemTime PolId Error");
            if (!DateTime.TryParseExact(datalist.SingleOrDefault(item => item.Contains("SystemTime"))?.Split('=')[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _systemTime))
            {
                throw new ArgumentException($"GB_Server HJ212 GetSystemTime SystemTime Error");
            }
            await Task.CompletedTask;
        }

        public (bool Type, byte[]? CheckBytes) Check(string clientInfo, byte[] bytes)
        {
            var rs = Encoding.ASCII.GetString(bytes).Split(';');
            var qn = rs.SingleOrDefault(item => item.Contains("QN"))?.Split('=')[1];
            return (rs.Where(item => item.Contains($"CN={(int)CN_Client.上传现场机时间}")).Any(), qn == null ? default : Encoding.ASCII.GetBytes(qn));
        }

        public (string PolId, DateTime SystemTime, RspInfo RspInfo) GetResult()
        {
            return (_polId, _systemTime, _rspInfo);
        }

        public async Task<bool> IsFinish()
        {
            return await Task.FromResult(true);
        }
    }
}
