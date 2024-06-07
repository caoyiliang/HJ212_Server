using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Request
{
    internal class SetRealTimeDataIntervalReq(string mn, string pw, ST st, int interval) : IAsyncRequest
    {
        private readonly string _QN = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        public byte[]? Check()
        {
            return Encoding.ASCII.GetBytes(_QN);
        }

        public byte[] ToBytes()
        {
            var rs = $"QN={_QN};ST={(int)st};CN={(int)CN_Server.设置实时数据间隔};PW={pw};MN={mn};Flag={1 | (int)GB_Server._version};CP=&&RtdInterval={interval}&&";
            rs = GB_Server.GetGbCmd(rs);
            return Encoding.ASCII.GetBytes(rs);
        }
    }
}
