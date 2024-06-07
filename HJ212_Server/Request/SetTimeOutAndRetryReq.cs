using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Request
{
    internal class SetTimeOutAndRetryReq(string mn, string pw, ST st, int overTime, int reCount) : IAsyncRequest
    {
        private readonly string _QN = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        public byte[]? Check()
        {
            return Encoding.ASCII.GetBytes(_QN);
        }

        public byte[] ToBytes()
        {
            var rs = $"QN={_QN};ST={(int)st};CN={(int)CN_Server.设置超时时间及重发次数};PW={pw};MN={mn};Flag={1 | (int)GB_Server._version};CP=&&OverTime={overTime};ReCount={reCount}&&";
            rs = GB_Server.GetGbCmd(rs);
            return Encoding.ASCII.GetBytes(rs);
        }
    }
}
