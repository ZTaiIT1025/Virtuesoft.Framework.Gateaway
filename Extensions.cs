using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Virtuesoft.Framework.Gateaway.Extensions
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Extensions
    {
        #region 基础扩展
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="hasTime"></param>
        /// <returns></returns>
        public static Task Log(this HttpContext httpContext, string message, string title = null, bool hasTime = true)
        {
            try
            {
                var mssqlLog = httpContext.RequestServices.GetRequiredService<ILoggerFactory>()?.CreateLogger<ILogger>();
                if (mssqlLog == null)
                    return Log(message, title, hasTime);
                if (title.IsNotNullOrEmpty())
                    mssqlLog.LogError($"{title}:{message}");
                else
                    mssqlLog.LogError($"{message}");
            }
            catch (Exception)
            {
                return Log(message, title, hasTime);
            }
            

            return Task.CompletedTask;
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="gateaway"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="hasTime"></param>
        /// <returns></returns>
        public static Task Log(this GateawayBase gateaway, string message, string title = null, bool hasTime = true) {
           return gateaway.Context.Log(message, title, hasTime);
        }
        /// <summary>
        /// 保存日志到文件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="hasTime"></param>
        /// <returns></returns>
        private static Task Log(string message, string title = null, bool hasTime = true) {
            var directory = $@"{ Directory.GetCurrentDirectory()}\log";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var path = $@"{directory}\{DateTime.Now.ToString("yyMMdd")}.txt";
            try
            {
                using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        if (hasTime)
                            writer.WriteLine(DateTime.Now.ToString());
                        if (title.IsNotNullOrEmpty())
                            writer.WriteLine($"=================================={title}===================================");
                        writer.WriteLine(message);
                        writer.Flush();
                        writer.Close();
                    }
                    stream.Flush();
                    stream.Close();
                }
            }
            catch
            {

            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取用户的IP地址
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string UserIpAddress(this HttpContext httpContext)
        {
            var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }

        /// <summary>
        /// 将字典转换为xml
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToXML(this IDictionary<string, string> @this)
        {
            XElement element = new XElement("xml");
            foreach (var kv in @this)
            {
                element.Add(new XElement(kv.Key, kv.Value));
            }
            return element.ToString();
        }
        /// <summary>
        /// 将字典排序后转换为url类型
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToUrl(this IDictionary<string, string> @this)
        {
            return string.Join("&", @this.OrderBy(t => t.Key)
                  //.Where(t => t.Key != "sign")
                  .Select(t => $"{t.Key}={t.Value}"));
        }
        /// <summary>
        /// 将xml字符串转换为字典
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IDictionary<string, string> XmlToDictionary(this string @this)
        {
            XElement element = XElement.Load(new StringReader(@this));
            var result = new Dictionary<string, string>();
            foreach (var node in element.Elements())
            {
                if (!result.ContainsKey(node.Name.LocalName))
                    result.Add(node.Name.LocalName, node.Value);
            }
            return result;
        }
        /// <summary>
        /// 将xml对象转换为字典
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this XElement @this)
        {
            XElement element = @this;
            var result = new Dictionary<string, string>();
            foreach (var node in element.Elements())
            {
                if (!result.ContainsKey(node.Name.LocalName))
                    result.Add(node.Name.LocalName, node.Value);
            }
            return result;
        }
        /// <summary>
        /// 转换为指定小数位数的数字
        /// </summary>
        /// <param name="this"></param>
        /// <param name="poit"></param>
        /// <returns></returns>
        public static decimal Format(this decimal @this,int poit=2)
        {
            var p = (decimal)Math.Pow(10, poit);
            return (Math.Truncate(@this * p)) / p;
        }
        /// <summary>
        /// 写入 httpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Write(this HttpContext httpContext, string message)
        {
            var response = httpContext.Response;
            if (message.IsNotNullOrEmpty())
            {
                response.ContentLength = message.ToBytes().Length;
                await response.WriteAsync(message);
                await response.Body.FlushAsync();
            }
        }
        /// <summary>
        /// 写入 httpContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContext"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Write<T>(this HttpContext httpContext, T message)
        {
            if(message is string)
                await httpContext.Write(message as string);
            else
            await httpContext.Write(JsonConvert.SerializeObject(message));
        }
        /// <summary>
        /// 执行成功
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="d"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Success(this HttpContext httpContext, object d = null, string message = "ok")
        {
            var config = httpContext.RequestServices.GetService<GateawayOption>();
            var result = config.FormatResult(true, 200, message, d);
            await httpContext.Write(result);
        }
        /// <summary>
        /// 执行失败
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static async Task Error(this HttpContext httpContext, string message, int code = 500, object d = null)
        {
            var config = httpContext.RequestServices.GetService<GateawayOption>();
            var result = config.FormatResult(false, code, message, d);
            await httpContext.Write(result);
        }
        /// <summary>
        /// 获取post过来的所有参数
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Forms(this HttpContext httpContext)
        {
            if (httpContext.Items.ContainsKey("forms"))
                return httpContext.Items["forms"] as Dictionary<string, string>;
            var forms = new Dictionary<string, string>();
            if (httpContext.Request.Method.ToUpper() != "POST" || !httpContext.Request.Body.CanRead)
                return forms;
            try
            {
                httpContext.Request.EnableRewind();
                if (httpContext.Request.ContentType == "application/json")
                {
                    //HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(httpContext.Request.Body))
                    {
                        var json = sr.ReadToEnd();
                        //HttpContext.Log(xml, "xml");
                        forms = JObject.Parse(json).ToObject<Dictionary<string, string>>()
                            .Where(t => t.Value.IsNotNullOrEmpty())
                            .ToDictionary(t => t.Key, t => t.Value);
                        httpContext.Items.Add("forms", forms);
                        return forms;
                    }
                }
                if (httpContext.Request.ContentType == "application/xml")
                {
                    var encoding = Encoding.UTF8;
                    var option = httpContext.RequestService<GateawayOption>();
                    if (option!=null && option.Encoding!=null)
                        encoding = option.Encoding;
                    using (var sr = new StreamReader(httpContext.Request.Body, encoding))
                    {
                        var xml = sr.ReadToEnd();
                        //HttpContext.Log(xml, "xml");
                        var dir = xml.XmlToDictionary();

                        foreach (var form in dir)
                        {
                            forms.Add(form.Key, form.Value);
                        }
                    }
                    httpContext.Items.Add("forms", forms);
                    return forms;
                }
                if (httpContext.Request.ContentType == "application/x-www-form-urlencoded")
                {
                    using (var sr = new StreamReader(httpContext.Request.Body))
                    {
                        var url = sr.ReadToEnd();
                        var dir = url.Split('&');
                        foreach (var form in dir)
                        {
                            var kv = form.Split('=');
                            forms.Add(kv[0], kv[1]);
                        }
                    }
                    httpContext.Items.Add("forms", forms);
                    return forms;
                }

                foreach (var form in httpContext.Request.Form)
                {
                    forms.Add(form.Key, form.Value.ToString());
                }
                httpContext.Items.Add("forms", forms);
                return forms;

            }
            catch (Exception ex)
            {
                httpContext.Log(ex.Message, "HttpContext.Forms");
                httpContext.Log(httpContext.Request.ContentType, "", false);
                httpContext.Log($"canread:{httpContext.Request.Body.CanRead}", "", false);
            }
            httpContext.Items.Add("forms", forms);
            return forms;
        }
        /// <summary>
        /// 获取url传递过来的参数
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryString(this HttpContext httpContext)
        {
            if (httpContext.Items.ContainsKey("query"))
                return httpContext.Items["query"] as Dictionary<string, string>;
            var forms = new Dictionary<string, string>();
            if (httpContext.Request.Method.ToUpper() != "GET" && !httpContext.Request.Query.Any())
                return forms;
            try
            {
                if (httpContext.Request.Query.Any())
                {
                    foreach (var form in httpContext.Request.Query)
                    {
                        forms.Add(form.Key.ToLower(), form.Value.ToString().ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                httpContext.Log(ex.Message, "HttpContext.QueryString");
                httpContext.Log(httpContext.Request.ContentType, "", false);
            }
            httpContext.Items.Add("query", forms);
            return forms;
        }
        /// <summary>
        /// 字符串是否是空或者null
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string @this) {
            return !string.IsNullOrEmpty(@this);
        }
        /// <summary>
        /// 转换为json字符串
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToJson(this object @this) {
            try
            {
                return JsonConvert.SerializeObject(@this, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 转换为byte[]
        /// </summary>
        /// <param name="this"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string @this,string encoding="utf-8") {
            if (string.IsNullOrEmpty(@this)) return new byte[] { };
            var coding = Encoding.GetEncoding(encoding);
            if (coding == null) coding = Encoding.UTF8;
            return coding.GetBytes(@this);
        }
        #endregion


        /// <summary>
        /// 注册一个接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterGateaway<T>(this IServiceCollection services) where T: IGateaway, new()
        {
            services.AddTransient(typeof(T));
            if(!services.Any(t=>t.ServiceType==typeof(GateawayOption)))
                services.AddSingleton<GateawayOption>();
            if (!services.Any(t => t.ServiceType == typeof(IGateawayRoute)))
                services.AddSingleton(provider => {
                    return ActivatorUtilities.CreateInstance<GateawayRoute>(provider, services) as IGateawayRoute;
                });
            return services;
        }
        /// <summary>
        /// 注册所有接口服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGateaways(this IServiceCollection services) {
            var assemblys = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(t => t.GetTypes().Any(a => a.BaseType == typeof(GateawayBase)))
                .Select(t=>t.GetTypes().Where(a => a.BaseType == typeof(GateawayBase)))
                .ToList();
            assemblys.ForEach(assembly => {
                foreach (var gateaway in assembly)
                {
                    try
                    {
                        services.AddTransient(gateaway);
                    }
                    catch
                    {
                        continue;
                    }
                    
                }
            });
            if (!services.Any(t => t.ServiceType == typeof(GateawayOption)))
                services.AddSingleton<GateawayOption>();
            if (!services.Any(t => t.ServiceType == typeof(IGateawayRoute)))
                services.AddSingleton<IGateawayRoute>(provider=> {
                    return ActivatorUtilities.CreateInstance<GateawayRoute>(provider, services) as IGateawayRoute;
                });
            return services;
        }
        /// <summary>
        /// 注册接口中间件
        /// </summary>
        /// <param name="app"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGateaway(this IApplicationBuilder app,Action<GateawayOption> option=null)
        {
            GateawayOption config = app.ApplicationServices.GetService<GateawayOption>(); ;
            option?.Invoke(config);
            app.UseMiddleware<GateawayLogMiddelware>();
            app.UseMiddleware<GateawayMiddleware>();
            return app;
        }
        /// <summary>
        /// 获取一个服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static T RequestService<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<T>();

        }
        /// <summary>
        /// 返回执行结果
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object ExcuteResult(this HttpContext httpContext,object data=null) {
            var option = httpContext.RequestService<GateawayOption>();
            return option.FormatResult(httpContext.Items.ContainsKey("status") ? (bool)httpContext.Items["status"] : false,
                httpContext.Items.ContainsKey("code") ? (int)httpContext.Items["code"] : 500,
                httpContext.Items.ContainsKey("message") ? (string)httpContext.Items["message"] : "error",
                data );
        }
        /// <summary>
        /// 获取参数的描述信息
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static ParameterDisplay Display(this ParameterInfo parameter) {
            var arribute = parameter.GetCustomAttribute<ParameterDisplayAttribute>();
            if (arribute == null)
                return new ParameterDisplay()
                {
                    name = parameter.Name,
                    type = parameter.ParameterType.Name,
                    required = !parameter.IsOptional
                };
            return new ParameterDisplay()
            {
                name = arribute.Name,
                type = arribute.Type,
                required = arribute.Required,
                 @default= arribute.Default,
                 display= arribute.Display
            };
        }
        /// <summary>
        /// 获取所有参数的描述信息
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public static IEnumerable< ParameterDisplay> Parameters(this MethodDescriptor actionDescriptor)
        {
            var pas = actionDescriptor.Method.GetCustomAttributes<ParameterDisplayAttribute>();
            return actionDescriptor.Method.GetParameters()
                .Select(t => {
                    if (!pas.Any(p => p.Name == t.Name))
                        return new ParameterDisplay() {
                            name=t.Name,
                            type=t.ParameterType.Name,
                            required=!t.IsOptional
                        };
                    var arribute = pas.FirstOrDefault(p => p.Name == t.Name);
                    var propertys = new ParameterDisplay[] { };
                    if (t.ParameterType.IsArray)
                        propertys = t.ParameterType.GetElementType().GetProperties()
                        .Select(filed =>
                        {
                            var p = filed.GetCustomAttribute<ParameterDisplayAttribute>(true);
                            if (p == null) return new ParameterDisplay()
                            {
                                name = filed.Name,
                                type = filed.PropertyType.Name
                            };
                            return new ParameterDisplay()
                            {
                                name = p.Name,
                                type = p.Type,
                                required = p.Required,
                                @default = p.Default,
                                display = p.Display
                            };
                        }).ToArray();
                    return new ParameterDisplay()
                    {
                        name = arribute.Name,
                        type = arribute.Type,
                        required = arribute.Required,
                        @default = arribute.Default,
                        display = arribute.Display,
                         Propertys= propertys
                    };
                });
        }
        /// <summary>
        /// 返回值描述
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public static ParameterDisplay ResultParameters(this MethodDescriptor actionDescriptor) {
            var returnType = actionDescriptor.Method.ReturnType;
            var display = returnType.GetCustomAttribute<ParameterDisplayAttribute>();
            if(display==null)
                display= new ParameterDisplayAttribute()
                {
                    Name = actionDescriptor.Method.ReturnParameter.Name,
                    Type = actionDescriptor.Method.ReturnType.Name
                };
            var returnParameter = new ParameterDisplay()
            {
                name = display.Name,
                @default = display.Default,
                display = display.Display,
                required = display.Required,
                type = display.Type
            };
            if (returnType.IsPrimitive || returnType.ReflectedType == typeof(string) || returnType.IsEnum)
                return returnParameter;
            var properties = returnType.GetProperties();
            if (returnType.IsGenericType)
                properties = returnType.GetGenericArguments()[0]?.GetProperties();
            if (returnType.IsArray)
                properties = returnType.GetElementType().GetProperties();
            if (returnType == typeof(Task))
                return returnParameter;
            returnParameter.Propertys = properties.Select(filed =>
            {
                var p = filed.GetCustomAttribute<ParameterDisplayAttribute>(true);
                if (p == null) return new ParameterDisplay()
                {
                    name = filed.Name,
                    type = filed.PropertyType.Name
                };
                return new ParameterDisplay()
                {
                    name = p.Name,
                    type = p.Type,
                    required = p.Required,
                    @default = p.Default,
                    display = p.Display
                };
            });
            return returnParameter;
        }
        /// <summary>
        /// 是否是回调函数
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static bool IsCallback(this HttpContext httpContext)
        {
            GateawayOption config = httpContext.RequestService<GateawayOption>(); ;
            if (httpContext.Forms().ContainsKey("method"))
                if (httpContext.Forms()["method"].Split('.').Any(t=> config.NotVerifyForMethods.Contains(t)))
                    return true;
            return httpContext.Request.Path.ToString().ToLower().Split('/').Any(t=> config.NotVerifyForMethods.Contains(t));
        }
    }
}
