//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [Description( "Redirects user to a new url after a specific number of idle seconds" )]
    [TextField( 0, "New Location", "The new location URL to send user to after idle time", true )]
    [IntegerField( 1, "Idle Seconds", "20", "IdleSeconds", "", "How many seconds of idle time to wait before redirecting user" )]
    public partial class IdleRedirect : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            CurrentPage.AddScriptLink( this.Page, "~/scripts/idle-timer.min.js" );
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int idleSeconds = 20;
            if ( !int.TryParse( GetAttributeValue( "IdleSeconds" ), out idleSeconds ) )
                idleSeconds = 20;

            int ms = idleSeconds * 1000;

            string script = string.Format( @"

    $.idleTimer({0});
    $(document).bind('idle.idleTimer', function(){{
        window.location = '{1}';
    }});
                
", ms, GetAttributeValue( "NewLocation" ) );
            Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "idle-timeout", script, true );
        }
   }
}