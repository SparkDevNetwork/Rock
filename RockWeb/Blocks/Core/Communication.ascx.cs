//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for creating a new communication
    /// </summary>
    public partial class Communication : RockBlock
    {
        /// <summary>
        /// Gets or sets the current channel.
        /// </summary>
        /// <value>
        /// The current channel.
        /// </value>
        public int CurrentChannelId
        {
            get { return ViewState["CurrentChannel"] as int? ?? 0; }
            set { ViewState["CurrentChannel"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BindChannels();
        }


        /// <summary>
        /// Handles the Click event of the lbChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbChannel_Click( object sender, EventArgs e )
        {
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int channelId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out channelId ) )
                {
                    CurrentChannelId = channelId;

                    var definedValue = DefinedValueCache.Read(channelId);

                    phContent.Controls.Clear();
                    string controlPath = definedValue.GetAttributeValue( "ControlPath" );
                    if ( !string.IsNullOrWhiteSpace(controlPath) )
                    {
                        var control = phContent.LoadControl( controlPath );
                        phContent.Controls.Add( control );
                    }

                    BindChannels();
                }
            }
        }

        protected void BindChannels()
        {
            rptChannels.DataSource = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.COMMUNICATION_CHANNEL ) ).DefinedValues;
            rptChannels.DataBind();
        }

    }
}
