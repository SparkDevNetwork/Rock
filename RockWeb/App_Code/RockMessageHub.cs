﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

/// <summary>
/// This is the hub for sending/receiving messages. Javascript clients need to
/// implement a receiveNotification handler as shown below and
/// ensure to add the signalR-2.1.2.min.js before including /SignalR/hubs
/// <code>
///     // in OnInit
///     RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.1.2.min.js", fingerprint: false );
///
///     /* in the ascx */
///     <script src="/SignalR/hubs"></script>
///     <script type="text/javascript">
///     $(function () {
///         var proxy = $.connection.rockMessageHub;
///         proxy.client.receiveNotification = function (message) {
///             //do something here...
///         }
///     })
/// </code>
/// </summary>
namespace RockWeb
{
    [HubName( "rockMessageHub" )]
    public class RockMessageHub : Hub
    {
        public void Send( string name, string message )
        {
            Clients.All.receiveNotification( name, message );
        }
    }
}