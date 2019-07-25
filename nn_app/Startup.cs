using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(nn_app.Startup))]
namespace nn_app
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
