using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口
    /// </summary>
    public interface IGateawayMiddleware
    {
       /// <summary>
       /// 下一个中间件
       /// </summary>
        RequestDelegate Next { get; }
        /// <summary>
        /// 调用执行
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task Invoke(HttpContext httpContext);
    }
}
