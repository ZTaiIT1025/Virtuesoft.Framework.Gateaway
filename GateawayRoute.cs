using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Virtuesoft.Framework.Gateaway.Extensions;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 默认路由接口
    /// </summary>
    public class GateawayRoute : IGateawayRoute
    {
        /// <summary>
        /// 接口描述缓存器
        /// </summary>
        protected MethodDescriptorCollection actionDescriptorCollection;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serviceCollection"></param>
        public GateawayRoute(IServiceProvider serviceProvider, IServiceCollection serviceCollection)
        {
            var gateways = serviceCollection.Where(t => t.ServiceType.BaseType == typeof(GateawayBase));
            actionDescriptorCollection = new MethodDescriptorCollection(ActionDescriptorParser(serviceProvider, gateways));
        }
        /// <summary>
        /// 过滤并保存所有满足条件的接口方法
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public virtual List<MethodDescriptor> ActionDescriptorParser(IServiceProvider serviceProvider, IEnumerable<ServiceDescriptor> services)
        {
            var list = new List<MethodDescriptor>();
            foreach (var serviceDescriptor in services)
            {
                var service = (IGateaway)serviceProvider.GetService(serviceDescriptor.ServiceType);
                if (service == null)
                    throw new Exception("请在Startup中使用app.UseGateaway()注册服务");
                try
                {
                    var methods = service
                   .GetType()
                   .GetMethods(BindingFlags.Public | BindingFlags.InvokeMethod |
                   BindingFlags.Instance | BindingFlags.DeclaredOnly)
                   .Where(t => !t.IsSpecialName)
                   .Where(t => {
                       var dispay = t.GetCustomAttribute<MethodDisplayAttribute>();
                       if (dispay == null) return true;
                       if (!dispay.CallEnable) return false;
                       return true;
                   })
                   .ToDictionary((method) => {
                       var methodDisplay = method.GetCustomAttribute<MethodDisplayAttribute>();
                       var name = method.Name.ToLower();
                       if (methodDisplay != null && methodDisplay.Name.IsNotNullOrEmpty())
                           name = methodDisplay.Name;
                       return name;
                   }, t => t);
                    foreach (var method in methods)
                    {
                        var methodDescriptor = new MethodDescriptor(service, method.Value);
                        if (list.Any(t => t.Name == methodDescriptor.Name))
                            throw new Exception($"方法名称重复:{methodDescriptor.Name}");
                        list.Add(methodDescriptor);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("初始化所有接口错误,请检查是否有接口名称重复,默认方法名不支持重载");
                }
               
            }
            return list;
        }
        /// <summary>
        /// 匹配满足条件的接口方法
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual MethodDescriptor Invoke(HttpContext httpContext)
        {
            var forms = httpContext.Forms();
            //处理回调
            var path = httpContext.Request.Path.ToString().ToLower();
            if (httpContext.IsCallback()) {
                var callbackMethod = path.Split('/').Where(t => t != null && t.Length > 0).Last();
                if (!forms.Any(t => t.Key == "method")&&!forms.Any(t => t.Value == callbackMethod))
                    forms.Add("method", callbackMethod);
            }
            if (!forms.Any(t => t.Key == "method"))
                forms.Add("method", "home.index");
            var method = forms["method"];
            if (!actionDescriptorCollection.Items.Any(t => t.Name == method))
                return null;
            return actionDescriptorCollection.Items.Where(t => t.Name == method).First();
        }
        /// <summary>
        /// 获取所有已经缓存的接口方法
        /// </summary>
        public IReadOnlyList<MethodDescriptor> Items => actionDescriptorCollection.Items;
    }
}
  

