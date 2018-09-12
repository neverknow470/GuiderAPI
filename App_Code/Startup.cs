using System;
using Config;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(AppsCore.Startup))]

namespace AppsCore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = WebApiConfig.GetConfig();
            app.UseWebApi(config);
        }
    }
}