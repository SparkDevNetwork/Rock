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
using Rock;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Idle Redirect" )]
    [Category( "Utility" )]
    [Description( "Redirects user to a new url after a specific number of idle seconds." )]

    [TextField( "New Location", "The new location URL to send user to after idle time" )]
    [IntegerField( "Idle Seconds", "How many seconds of idle time to wait before redirecting user", false, 20 )]
    public partial class IdleRedirect : RockBlock, IIdleRedirectBlock
    {
        #region IIdleRedirectBlock

        /// <summary>
        /// local var for whether this idle redirect block has been disabled/enabled
        /// </summary>
        private bool _enabled
        {
            get
            {
                return ViewState["_enabled"] as bool? ?? true;
            }

            set
            {
                ViewState["_enabled"] = value;
            }
        }

        /// <summary>
        /// Disables the idle redirect block if disable = true, or re-enables it if disable = false
        /// </summary>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        public void Disable( bool disable )
        {
            _enabled = !disable;
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/idle-timer.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            // only register the script if this block is enabled and visible (IIdleRedirectBlock)
            if ( _enabled )
            {
                RegisterIdleRedirectJavaScript();
            }
        }

        /// <summary>
        /// Registers the idle redirect java script.
        /// </summary>
        private void RegisterIdleRedirectJavaScript()
        {
            int idleSeconds = GetAttributeValue( "IdleSeconds" ).AsIntegerOrNull() ?? 30;

            int idleMilliseconds = idleSeconds * 1000;

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
            ", idleMilliseconds, ResolveRockUrl( GetAttributeValue( "NewLocation" ) ) );

            ScriptManager.RegisterStartupScript( Page, this.GetType(), "idle-timeout", script, true );
        }
    }
}