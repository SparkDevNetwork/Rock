using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

/// <summary>
/// This is the hub for sending/receiving messages. Javascript clients need to
/// implement a receiveNotification handler:
/// <code>
///     proxy.client.receiveNotification = function (message) {
///            //do something here...
///        }
/// </code>
/// </summary>
namespace RockWeb
{
    [HubName( "rockMessageHub" )]
    public class RockMessageHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.receiveNotification( message );
        }
    }
}