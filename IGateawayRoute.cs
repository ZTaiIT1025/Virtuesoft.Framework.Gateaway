using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口路由器
    /// </summary>
    public interface IGateawayRoute
    {
        /// <summary>
        /// 方法过滤器
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        List<MethodDescriptor> ActionDescriptorParser(IServiceProvider serviceProvider, IEnumerable<ServiceDescriptor> services);
        /// <summary>
        /// 查找满足条件的接口方法
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        MethodDescriptor Invoke(HttpContext httpContext);
        /// <summary>
        /// 方法缓存器
        /// </summary>
        IReadOnlyList<MethodDescriptor> Items { get; }


    }
}
