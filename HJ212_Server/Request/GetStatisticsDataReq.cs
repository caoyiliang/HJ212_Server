using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Request
{
    internal class GetStatisticsDataReq(string mn, string pw, ST st, CN_Server cn, DateTime beginTime, DateTime endTime) : IAsyncRequest
    {
        private readonly string _QN = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        public byte[]? Check()
        {
            return Encoding.ASCII.GetBytes(_QN);
        }

        public byte[] ToBytes()
        {
            var rs = $"QN={_QN};ST={(int)st};CN={(int)cn};PW={pw};MN={mn};Flag={1 | (int)GB_Server._version};CP=&&BeginTime={beginTime:yyyyMMddHHmmss};EndTime={endTime:yyyyMMddHHmmss}&&";
            rs = GB_Server.GetGbCmd(rs);
            return Encoding.ASCII.GetBytes(rs);
        }
    }
}
