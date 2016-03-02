using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TgpBugTracker.Startup))]
namespace TgpBugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
