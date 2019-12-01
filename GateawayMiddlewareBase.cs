using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using Virtuesoft.Framework.Gateaway.Extensions;
namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口中间件
    /// </summary>
    public class GateawayMiddleware : IGateawayMiddleware
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        protected virtual string Name => "Gateaway";
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public RequestDelegate Next { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="services"></param>
        public GateawayMiddleware(RequestDelegate next, IServiceProvider services)
        {
           
            Next = next;
        }
        /// <summary>
        /// 执行结果
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected object Result(bool s, int c, string m, object d) {
            return new { s = s, c = c, m = m, d = d };
        }
        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected object Error(string message = "error", object d = null) {
            return Result(true, 200, message, d);
        }
        /// <summary>
        /// 执行成功
        /// </summary>
        /// <param name="d"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected object Success(object d = null, string message = "ok")
        {
            return Result(true, 200, message, d);
        }

        /// <summary>
        /// 执行接口
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <param name="prameters"></param>
        protected virtual async Task<object> Execute(MethodInfo method, object instance,object[] prameters)
        {
           var result=method.Invoke(instance, prameters);
            //if(result.GetType()==typeof( dynamic))return result;
            if (result!=null&& result.GetType().IsGenericType)
            {
                var resultT = result.GetType().GetProperty("Result");
                if (resultT == null) return result;
                var value=resultT.GetValue(result);
                return value;
            }
            if (result is Task)
            {
                await (result as Task);
                return null;
            }
            return result;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task Invoke(HttpContext httpContext)
        {
            var config = httpContext.RequestServices.GetService<GateawayOption>();
            if (!config.VerifyIP(httpContext, httpContext.UserIpAddress()))
            {
                await httpContext.Error("ip地址不允许访问");
                return;
            }
            var forms = httpContext.Forms();
            var parameterResult = config.VerifyPrameters(forms, httpContext);
            if (!parameterResult.status)
            { await httpContext.Error(parameterResult.message); return; }
            
            try
            {
                var result=await Execute(httpContext);
                if(result!=null)
                await httpContext.Write(result);
            }
            catch (Exception ex)
            {
               await httpContext.Log(ex.Message, "GateawayMiddelware.Invoke");
               await httpContext.Log(ex.StackTrace, "StackTrace", false);
               await httpContext.Log(ex.Source, "Source", false);
               await httpContext.Error("调用错误!");
            }
        }
        /// <summary>
        /// 执行并返回结果
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual async Task<object> Execute(HttpContext httpContext)
        {
            IGateawayRoute route = httpContext.RequestService<IGateawayRoute>();
            MethodDescriptor actionDescriptor = route.Invoke(httpContext);
            if (actionDescriptor == null)
                return Error(message: "接口不存在");
            var config = httpContext.RequestServices.GetService<GateawayOption>();
            //如果是请求api文档,则不验证签名
            if (!actionDescriptor.Name.StartsWith("api.")&& !httpContext.IsCallback() && !config.VerifySign(httpContext.Forms(), httpContext))
            { return Error("签名错误"); }
            try
            {
                var gateaway = (GateawayBase)httpContext.RequestServices.GetService(actionDescriptor.Gateaway);
                var parameters = actionDescriptor.CreateParameters(httpContext);
                gateaway.Context = httpContext;
                gateaway.Context.Items["status"] = true;
                gateaway.Context.Items["message"] = string.Empty;
                gateaway.Context.Items["code"] = 200;
                var result= await Execute(actionDescriptor.Method, gateaway, parameters);
                await httpContext.Log(result.ToJson(), "remote.result", false);
                //如果是回调函数,就不使用格式化数据
                if (httpContext.IsCallback())
                    return result;
                return gateaway.Context.ExcuteResult(result);
                
            }
            catch (Exception ex)
            {
                await httpContext.Log(ex.Message, "GateawayMiddlewareBase.Invoke");
                await httpContext.Log(ex.StackTrace, "", false);
                await httpContext.Log(ex.Source, "", false);
                return Error($"{ex.Message}");
            }
            
        }
    }
    /// <summary>
    /// 默认接口中间件
    /// </summary>
    public class DefaultGateawayMiddleware : GateawayMiddleware {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="services"></param>
        public DefaultGateawayMiddleware(RequestDelegate next, IServiceProvider services):base(next, services) {

        }
    }
}
