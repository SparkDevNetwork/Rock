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
using Rock.Bus;
using Rock.Model;
using Rock.Web;

namespace RockWeb.Blocks.Bus
{
    /// <summary>
    /// Message Bus Status.
    /// </summary>
    [DisplayName( "Bus Status" )]
    [Category( "Bus" )]
    [Description( "Gives insight into the message bus." )]

    #region Block Attributes

    [LinkedPage(
        "Transport Select Page",
        Key = AttributeKey.TransportSelectPage,
        Description = "The page where the transport for the bus can be selected",
        DefaultValue = Rock.SystemGuid.Page.BUS_TRANSPORT,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" )]
    public partial class BusStatus : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string TransportSelectPage = "TransportSelectPage";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindUI();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindUI();
        }

        /// <summary>
        /// Handles the Click event of the btnTransport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTransport_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.TransportSelectPage );
        }

        #endregion

        #region Methods

        private void BindUI()
        {
            BindActiveLabel();
            BindDetails();
        }

        /// <summary>
        /// Binds the details.
        /// </summary>
        private void BindDetails()
        {
            var descriptionList = new DescriptionList();

            var transport = RockMessageBus.GetTransportName();
            if ( !transport.IsNullOrWhiteSpace() )
            {
                descriptionList.Add( "Transport", transport );
            }

            descriptionList.Add( "NodeName", RockMessageBus.NodeName );

            var statLog = RockMessageBus.StatLog;
            if ( statLog != null )
            {
                if ( statLog.MessagesConsumedLastMinute.HasValue )
                {
                    descriptionList.Add( "Messages Per Minute", statLog.MessagesConsumedLastMinute );
                }

                if ( statLog.MessagesConsumedLastHour.HasValue )
                {
                    descriptionList.Add( "Messages Per Hour", statLog.MessagesConsumedLastHour );
                }

                if ( statLog.MessagesConsumedLastDay.HasValue )
                {
                    descriptionList.Add( "Messages Per Day", statLog.MessagesConsumedLastDay );
                }
            }

            lDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Binds the active label.
        /// </summary>
        private void BindActiveLabel()
        {
            if ( RockMessageBus.IsReady() )
            {
                hlActive.Text = "Active";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Success;
            }
            else
            {
                hlActive.Text = "Inactive";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Danger;
            }
        }

        #endregion
    }
}