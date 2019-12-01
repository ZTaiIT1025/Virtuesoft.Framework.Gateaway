using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Virtuesoft.Framework.Gateaway.Extensions;
namespace Virtuesoft.Framework.Gateaway
{
    /// <summary>
    /// 默认接口
    /// 用于显示接口文档
    /// api.doc
    /// api.query
    /// api.catalog
    /// </summary>
    public class ApiGateaway : GateawayBase
    {
        /// <summary>
        /// 接口描述
        /// </summary>
        public override string Display => "默认主页接口";
        /// <summary>
        /// 接口名称
        /// </summary>
        public override string Controller => "api";

        /// <summary>
        /// 接口文档
        /// </summary>
        /// <returns></returns>
        [Display(Name = "显示所有接口文档")]
        public ApiDocResult[] Doc()
        {
            var gateaways = Context.RequestServices.GetService<IGateawayRoute>();
            return gateaways.Items
                .Where(t => t.IsShow)
                 .Select(t => new ApiDocResult()
                 {
                     name = t.Name,
                     display = t.Display,
                     parameters = t.Parameters().ToArray(),
                     result = t.ResultParameters()
                 })
                 .OrderBy(t => t.name)
                 .ToArray();
        }
        /// <summary>
        /// 查询接口
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Display(Name = "查询单个接口详细内容")]
        public ApiDetaileResult Query(string name) {
            var gateaways = Context.RequestServices.GetService<IGateawayRoute>();
            return Success<ApiDetaileResult>(gateaways.Items
                .Where(t => t.IsShow)
                .Where(t=>t.Name==name)
                .Select(t => new ApiDetaileResult
                {
                    name = t.Name,
                    display = t.Display,
                    parameters = t.Parameters().ToArray(),
                    result = t.ResultParameters()
                }).FirstOrDefault());
        }
        /// <summary>
        /// 接口文档目录
        /// </summary>
        /// <returns></returns>
       // [Display(Name = "接口文档目录,只返回接口名称和描述")]
        [MethodDisplay(Display= "接口文档目录,只返回接口名称和描述",Name ="catalog")]
        public ApiCatalogResult[] Catalog() {
            var gateaways = Context.RequestServices.GetService<IGateawayRoute>();
            return Success<ApiCatalogResult[]>(gateaways.Items
                .Where(t=>t.IsShow)
                 .Select(t => new ApiCatalogResult
                 {
                     name = t.Name,
                     display = t.Display
                 })
                 .OrderBy(t => t.name)
                 .ToArray());
        }
    }
    [ParameterDisplay(Name = "CatalogResult", Display = "api文档目录", Type = "object", Required = true)]
    public class ApiCatalogResult
    {
        [ParameterDisplay(Name = "name", Display = "method 名称", Type = "string", Required = true)]
        public string name { get; set; }
        [ParameterDisplay(Name = "display", Display = "接口描述", Type = "string", Required = true)]
        public string display { get; set; }
    }


    [ParameterDisplay(Name = "DetaileResult", Display = "接口详细", Type = "object", Required = true)]
    public class ApiDetaileResult
    {
        [ParameterDisplay(Name = "name", Display = "method 名称", Type = "string", Required = true)]
        public string name { get; set; }
        [ParameterDisplay(Name = "display", Display = "接口描述", Type = "string", Required = true)]
        public string display { get; set; }
        [ParameterDisplay(Name = "parameters", Display = "接口参数", Type = "object[]", Required = true)]
        public ParameterDisplay[] parameters { get; set; }
        [ParameterDisplay(Name = "result", Display = "接口返回值", Type = "object[]", Required = true)]
        public ParameterDisplay result { get; set; }
    }

    public class ApiDocResult {
        public string name { get; set; }
        public string display { get; set; }

        public  ParameterDisplay[] parameters { get; set; }

        public ParameterDisplay result { get; set; }
    }
}
