namespace HJ212_Server.Model
{
    /// <summary>
    /// 基础信息
    /// </summary>
    public class RspInfo
    {
        /// <summary>请求编码</summary>
        public string? QN { get; internal set; }
        /// <summary>系统编码</summary>
        public string? ST { get; internal set; }
        /// <summary>访问密码</summary>
        public string? PW { get; internal set; }
        /// <summary>设备唯一标识</summary>
        public string? MN { get; internal set; }
        /// <summary>拆分包及应答标志</summary>
        public int? Flag { get; internal set; }
    }
}
