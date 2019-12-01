using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 参数属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | 
        AttributeTargets.ReturnValue | 
        AttributeTargets.Field | 
        AttributeTargets.Property|
         AttributeTargets.Class, AllowMultiple = true)]
    public class ParameterDisplayAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="Display"></param>
        /// <param name="Required"></param>
        /// <param name="Default"></param>
        public ParameterDisplayAttribute(string Name, string Type, string Display = "", bool Required = false, string Default = "")
        {
            this.Name = Name;
            this.Type = Type;
            this.Display = Display;
            this.Required = Required;
            this.Default = Default;
        }
        /// <summary>
        /// 
        /// </summary>
        public ParameterDisplayAttribute() : this("", "", "", false, "")
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        public ParameterDisplayAttribute(string Name) : this(Name, "", "", false, "")
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        public ParameterDisplayAttribute(string Name, string Type) : this(Name, Type, "", false, "")
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="Display"></param>
        public ParameterDisplayAttribute(string Name, string Type, string Display = "") : this(Name, Type, Display, false, "")
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="Display"></param>
        /// <param name="Required"></param>
        public ParameterDisplayAttribute(string Name, string Type, string Display = "", bool Required = false) : this(Name, Type, Display, Required, "")
        {

        }
        
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 参数描述
        /// </summary>
        public string Display { get; set; }
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string Default { get; set; }



    }
}
