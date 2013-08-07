//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Web.UI;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [Description( "Redirects user to a new url after a specific number of idle seconds" )]
    [TextField( "New Location", "The new location URL to send user to after idle time" )]
    [IntegerField( "Idle Seconds", "How many seconds of idle time to wait before redirecting user", false, 20 )]
    public partial class IdleRedirect : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            CurrentPage.AddScriptLink( this.Page, "~/Scripts/idle-timer.min.js" );
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            int idleSeconds;

            if ( !int.TryParse( GetAttributeValue( "IdleSeconds" ), out idleSeconds ) )
                idleSeconds = 20;

            int ms = idleSeconds * 1000;
            string script = string.Format( @"
$(function () {{
    $.idleTimer({0});
    $(document).bind('idle.idleTimer', function() {{
        window.location = '{1}';
    }});
}});
", ms, GetAttributeValue( "NewLocation" ) );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "idle-timeout", script, true );
        }
   }
}