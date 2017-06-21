using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Test.Entity
{
    /// <summary>
    /// thrift返回值基类
    /// </summary>
    public class ThriftResultBase
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 操作失败或异常返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public long Code { get; set; }
    }
}
