using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Luminary.Startup))]

namespace Luminary
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();       
        }
    }
}
