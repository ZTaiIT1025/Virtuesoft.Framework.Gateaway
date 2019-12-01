using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口
    /// </summary>
    public interface IGateaway
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        string Display { get; }
        /// <summary>
        /// 接口控制器名称
        /// 如order
        /// </summary>
        string Controller { get; }
        /// <summary>
        /// 获取 HttpContext
        /// </summary>
        HttpContext Context { get; set; }
    }
}
