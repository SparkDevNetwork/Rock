using System;
using Microsoft.Owin;
using Owin;
using System.Web;

namespace RockInstaller
{
    public class Startup
    {
        public void Configuration( IAppBuilder app )
        {
            app.MapSignalR();
        }
    }
}