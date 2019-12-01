using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Virtuesoft.Framework.Gateaway.Extensions;


namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口方法描述器
    /// </summary>
    public class MethodDescriptor
    {
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="gateaway"></param>
        /// <param name="method"></param>
        public MethodDescriptor(IGateaway gateaway, MethodInfo method) {
            Name =$"{gateaway.Controller}.{method.Name.ToLower()}" ;
            Display = method.GetCustomAttribute<DisplayAttribute>()?.Name;
            Gateaway = gateaway.GetType();
            Method = method;
            Parameter = method.GetParameters();
            Result = method.ReturnParameter;
            IsShow = true;
            var dispay = method.GetCustomAttribute<MethodDisplayAttribute>();
            if (dispay != null)
            {
                if (!dispay.ShowEnable)
                    IsShow = false;
                if (dispay.Name.IsNotNullOrEmpty())
                    Name = $"{gateaway.Controller}.{dispay.Name.ToLower()}";
                if (dispay.Display.IsNotNullOrEmpty())
                    Display = dispay.Display;
            }
        }
        /// <summary>
        /// 是否显示到文档
        /// </summary>
        public bool IsShow { get; }
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 方法描述
        /// </summary>
        public string Display { get;  }
        /// <summary>
        /// 所属接口类型
        /// </summary>
        public Type Gateaway { get;}
        /// <summary>
        /// 方法
        /// </summary>
        public MethodInfo Method { get; }
        /// <summary>
        /// 方法参数
        /// </summary>
        public ParameterInfo[] Parameter { get; }
        /// <summary>
        /// 创建方法执行参数
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public object[] CreateParameters(HttpContext httpContext) {
            var forms = httpContext.Forms();
            var list = new List<object>();
            foreach (var arg in Parameter)
            {
                var name = arg.Name.ToLower();
                if (forms.ContainsKey(name))
                    try
                    {
                        //var value = forms[name];
                        var v = JObject.FromObject(forms)[name].ToObject(arg.ParameterType);
                        list.Add(v);
                    }
                    catch
                    {
                        list.Add(Assembly.GetExecutingAssembly().CreateInstance(arg.ParameterType.ToString()));
                    }
                else
                    try
                    {
                        var v = JObject.FromObject(forms).ToObject(arg.ParameterType);
                        list.Add(v);
                    }
                    catch (Exception)
                    {
                        //list.Add(new object());
                        list.Add(Assembly.GetExecutingAssembly().CreateInstance(arg.ParameterType.ToString()));
                    }
                    
                
            }
            return list.ToArray();
        }
        /// <summary>
        /// 方法返回值
        /// </summary>
        public ParameterInfo Result { get; }
    }
}
