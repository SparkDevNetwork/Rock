using System;
using Microsoft.Owin;
using Owin;
using System.Web;


public class Startup
{
    public void Configuration( IAppBuilder app )
    {
        app.MapSignalR();
    }
}
