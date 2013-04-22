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
using Rock.Communication;
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
        /// Gets or sets the channel id.
        /// </summary>
        /// <value>
        /// The channel id.
        /// </value>
        public int? ChannelId 
        {
            get { return ViewState["ChannelId"] as int?; }
            set { ViewState["ChannelId"] = value; }
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
                    ShowChannel( channelId );
                }

                BindChannels();
            }
        }

        /// <summary>
        /// Binds the channels.
        /// </summary>
        protected void BindChannels()
        {
            var channels = new Dictionary<int, string>();
            foreach ( var item in ChannelContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    channels.Add( entityType.Id, entityType.FriendlyName );
                }
            }

            if ( channels.Any() && !ChannelId.HasValue )
            {
                ShowChannel( channels.Keys.FirstOrDefault() );
            }

            rptChannels.DataSource = channels;
            rptChannels.DataBind();
        }

        /// <summary>
        /// Shows the channel.
        /// </summary>
        private void ShowChannel( int channelId )
        {
            ChannelId = channelId;

            phContent.Controls.Clear();

            if ( ChannelId.HasValue )
            {
                var EntityType = EntityTypeCache.Read( ChannelId.Value );

                foreach ( var serviceEntry in ChannelContainer.Instance.Components )
                {
                    var channelComponent = serviceEntry.Value.Value;
                    if (channelComponent.EntityType.Id == ChannelId.Value)
                    {
                        var control = phContent.LoadControl( channelComponent.ControlPath );
                        phContent.Controls.Add( control );
                        break;
                    }
                }
            }
        }
    }
}
