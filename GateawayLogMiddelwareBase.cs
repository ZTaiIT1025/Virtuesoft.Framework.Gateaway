using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Virtuesoft.Framework.Gateaway.Extensions;

namespace Virtuesoft.Framework.Gateaway
{
   
    /// <summary>
    /// 日志记录中间件
    /// </summary>
   public class GateawayLogMiddelware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public RequestDelegate Next { get; }
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="next"></param>
        public GateawayLogMiddelware(RequestDelegate next)
        {
            Next = next;
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task Invoke(HttpContext context)
        {
            var config = context.RequestServices.GetService<GateawayOption>();
            var query = context.Request.Path.ToString().Split('/');
            var data = string.Empty;
            try
            {
                context.Request.EnableRewind();
                if (context.Request.Method.ToLower() == "post")
                    data = context.Forms().ToUrl();
                else
                    data = context.QueryString().ToUrl();

            }
            catch (Exception ex)
            {
                data = $"数据读取错误:{ex.Message}";
            }
            try
            {
                await config.BeginRequest(new Operation()
                {
                    Id = context.TraceIdentifier,
                    Method = context.Request.Method,
                    IP = context.UserIpAddress(),
                    UserID = context.Forms().Any(t=>t.Key=="method")? context.Forms()["method"]:"",
                    Path = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}",
                    OperationName = context.Request.Path.ToString(),
                    Data = data,
                    Creator = context.Request.Host.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault()
                }, context);
            }
            catch (Exception ex)
            {
                await context.Log(ex.Message, "app.Use/log");
            }
            try
            {
                using (var readStream = new MemoryStream())
                {
                    var originalResponseBody = context.Response.Body;
                    context.Response.Body = readStream;
                    await Next(context);
                    readStream.Seek(0, SeekOrigin.Begin);
                    //read
                    using (var reader = new StreamReader(readStream, System.Text.Encoding.UTF8))
                    {
                        var body = reader.ReadToEnd();
                        await config.EndReqeust(new Operation()
                        {
                            Id = context.TraceIdentifier,
                            UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                            Method = context.Request.Method,
                            IP = context.UserIpAddress(),
                            UserID = context.Forms().Any(t => t.Key == "method") ? context.Forms()["method"] : "",
                            Path = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}",
                            OperationName = "output",
                            Data = body,
                            Creator = context.Request.Host.ToString()
                        }, context);
                        readStream.Seek(0, SeekOrigin.Begin);
                        await readStream.CopyToAsync(originalResponseBody);
                        context.Response.Body = originalResponseBody;
                    }
                }
            }
            catch (Exception ex)
            {
                await context.Log(ex.Message, "app.Use/log end");
            }


        }
        
    }
    /// <summary>
    /// 操作详细
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 当前操作的IP
        /// </summary>
  
        public string IP { get; set; }
        /// <summary>
        /// 操作方法
        /// </summary>

        public string Method { get; set; } = "get";
        /// <summary>
        /// 访问路径
        /// </summary>

        public string Path { get; set; } = "/";
        /// <summary>
        /// 操作名称
        /// </summary>

        public string OperationName { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
  
        public string UserID { get; set; } = "";
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 用户浏览器代理
        /// </summary>
 
        public string UserAgent { get; set; }
    }
    /// <summary>
    /// 输入流
    /// </summary>
    internal class MemoryWrappedHttpResponseStream : MemoryStream
    {
        private Stream _innerStream;
        public MemoryWrappedHttpResponseStream(Stream innerStream)
        {
            this._innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }
        public override void Flush()
        {
            this._innerStream.Flush();
            base.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                base.Write(buffer, offset, count);
                this._innerStream.Write(buffer, offset, count);
            }
            catch
            {

            }

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this._innerStream.Dispose();
            }
        }

        public override void Close()
        {
            base.Close();
            this._innerStream.Close();
        }
    }
}
