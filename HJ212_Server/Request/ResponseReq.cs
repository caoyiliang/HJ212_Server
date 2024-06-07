using HJ212_Server.Model;
using System.Text;
using TopPortLib.Interfaces;

namespace HJ212_Server.Request
{
    internal class ResponseReq(RspInfo rspInfo) : IByteStream
    {
        public byte[] ToBytes()
        {
            var rs = $"{rspInfo.QN};ST=91;CN={(int)CN_Server.通知应答};{rspInfo.PW};{rspInfo.MN};Flag={0 | (int)GB_Server._version};CP=&&&&";
            rs = GB_Server.GetGbCmd(rs);
            return Encoding.ASCII.GetBytes(rs);
        }
    }
}
