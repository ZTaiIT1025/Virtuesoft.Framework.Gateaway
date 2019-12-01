using Microsoft.AspNetCore.Http;

namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 接口基类,实现基本方法
    /// </summary>
    public abstract class GateawayBase : IGateaway
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        public virtual string Display => "接口";
        /// <summary>
        /// 接口唯一标识
        /// </summary>
        public virtual string Controller => "api";

        /// <summary>
        /// 上下文对象
        /// </summary>
        public HttpContext Context { get; set; }
        /// <summary>
        /// 返回执行结果
        /// </summary>
        /// <param name="s">执行状态</param>
        /// <param name="c">状态码</param>
        /// <param name="m">消息</param>
        /// <param name="d">返回数据</param>
        /// <returns>object</returns>
        protected object Result(bool s, int c, string m, object d)
        {
            Context.Items["status"] = s;
            Context.Items["code"] = c;
            Context.Items["message"] = m;
            return d;
        }
        /// <summary>
        /// 返回执行错误
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="d">数据</param>
        /// <returns>object</returns>
        protected object Error(string message = "error", object d = null)
        {
            return Result(false, 200, message, d);
        }
        /// <summary>
        /// 返回错误
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected T Error<T>(string message = "error", object d = null) where T:class
        {
            return Result(false, 200, message, d) as T;
        }
        /// <summary>
        /// 返回执行成功
        /// </summary>
        /// <param name="d">返回数据</param>
        /// <param name="message">执行消息</param>
        /// <returns>object</returns>
        protected object Success(object d = null, string message = "ok")
        {
            return Result(true, 200, message, d);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected T Success<T>(object d = null, string message = "ok") where T:class
        {
            return Result(true, 200, message, d) as T;
        }
    }
}
