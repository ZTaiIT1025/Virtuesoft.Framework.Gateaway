using System;
using System.Collections.Generic;
using System.Text;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 方法的显示属性
    /// 可以将方法不公开给文档
    /// 也可以定义该方法不允许外包调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = false)]
    public class MethodDisplayAttribute: Attribute
    {
        /// <summary>
        /// 方法路由名称
        /// 如 app.phone.net
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否显示到文档
        /// </summary>
        public bool ShowEnable { get; set; } = true;
        /// <summary>
        /// 允许外部调用
        /// </summary>
        public bool CallEnable { get; set; } = true;
        /// <summary>
        /// 方法描述
        /// </summary>
        public string Display { get; set; }
    }
}
