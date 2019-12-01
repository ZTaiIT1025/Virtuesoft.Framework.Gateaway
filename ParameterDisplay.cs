using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 参数描述
    /// </summary>
   public class ParameterDisplay
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool required { get; set; } = true;
        /// <summary>
        /// 默认值
        /// </summary>
        public string @default { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string display { get; set; }
        /// <summary>
        /// 所有属性
        /// </summary>
        public IEnumerable<ParameterDisplay> Propertys { get; set; }
    }
}
