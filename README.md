# Virtuesoft.Framework.Gateaway
微型高效率接口框架 简单,干净尽量减少了依赖项. 框架自带接口文档,通过调用默认的api.doc方法可以查看所有的接口. 只需要添加类就可以完成接口的开发,让程序员全心的只专注于业务逻辑. 适用于简单逻辑的接口编写,无状态的接口. 默认验证用户权限是通过签名方式,支持可以自定义签名验证. 支持IP黑白名单和简单的过滤. 新增支持回调函数不验证签名 新增回调函数不格式化返回数据 新增不验证签名和格式化参数的方法 新增日志保存到数据库
简单使用:



<code>
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGateaways();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseGateaway();
            
        }
    }
    </code>
	
	
功能配置


    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        //配置编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            app.UseGateaway(option => {
            //签名验证
                option.OnVerifySign = OnVerifySign;
                //必要参数验证
                option.OnVerifyPrameters = OnVerifyPrameters;
                //看名字
                option.OnBeginRequest = BeginRequest;
                //看名字
                option.OnEndReqeust = EndReqeust;
                //验证ip,可以设置白名单,黑名单功能
                option.OnVerifyIP = OnVerifyIP;
                //返回参数的格式化
                option.OnFormatResult = OnFormatResult;
                //设置不验证参数和格式化的方法
                option.NotVerifyForMethods = Configuration["notveriymethods"].Split(',');
            });
        }
        
<b>前端使用:</b>


        post->https://gateaway.virtuesoft.cn
        header->Content-Type:application/json
        body->
        {
    "method": "api.doc",
    "参数1":"自己下载源码去玩"
    }
新增接口(案列):

<code>
	
	public class Member:GateawayBase
    {
        public override string Controller => "member";
        public async Task<int> Online()
        {
            return await Task.FromResult(0);
        }
        //像我这种懒人一般都直接返回 Task<Object>. 
        public User[] Paged() {
            return new User[] {
                new User(){ name="好",phone="1111111",sex="男"},
                new User(){ name="多",phone="2222222",sex="女" },
                new User(){ name="吊",phone="3333333",sex="男"},
                new User(){ name="毛",phone="4444444",sex="女"},
                new User(){ name="啊",phone="5555555",sex="男"},
            };
        }
    }
 
	
</code>	
	
	
    public class User {
        public string name { get; set; }
        public string sex { get; set; }
        public string phone { get; set; }
    }
	
	
    接口访问:
    
    
    post->https://gateaway.virtuesoft.cn
        header->Content-Type:application/json
        body->
        {
    "method": "member.paged"
    }


这套简单的接口框架公司自己在用于内部服务接口,目前最高日承受交易量2000多万,平均响应实效600ms,估计内部数据计算还有点优化的空间,光接口响应是在20ms左右.
