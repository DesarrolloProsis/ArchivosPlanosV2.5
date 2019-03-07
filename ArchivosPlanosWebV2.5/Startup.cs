using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArchivosPlanosWebV2._5.Startup))]
namespace ArchivosPlanosWebV2._5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
