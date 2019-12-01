using System;
using System.Collections.Generic;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 方法描述集合
    /// </summary>
    public class MethodDescriptorCollection
    {
        public MethodDescriptorCollection(IReadOnlyList<MethodDescriptor> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }
        /// <summary>
        /// 获取所有接口方法
        /// </summary>
        public IReadOnlyList<MethodDescriptor> Items { get; }

    }
}
