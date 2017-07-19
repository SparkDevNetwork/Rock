// <copyright>
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
using System.ComponentModel;
using System.Web.UI;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Idle Redirect" )]
    [Category( "Utility" )]
    [Description( "Redirects user to a new url after a specific number of idle seconds." )]

    [TextField( "New Location", "The new location URL to send user to after idle time" )]
    [IntegerField( "Idle Seconds", "How many seconds of idle time to wait before redirecting user", false, 20 )]
    public partial class IdleRedirect : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/idle-timer.min.js" );
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            int idleSeconds;

            if ( !int.TryParse( GetAttributeValue( "IdleSeconds" ), out idleSeconds ) )
                idleSeconds = 30;

            int ms = idleSeconds * 1000;
            string script = string.Format( @"
            $(function () {{
                Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {{
                    $.idleTimer('destroy');
                }});

                $.idleTimer({0});
                $(document).bind('idle.idleTimer', function() {{
                    window.location = '{1}';
                }});
            }});
            ", ms, ResolveRockUrl( GetAttributeValue( "NewLocation" ) ) );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "idle-timeout", script, true );
        }
   }
}