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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// List all the Interaction Channel.
    /// </summary>
    [DisplayName( "Interaction Channel Detail" )]
    [Category( "CRM" )]
    [Description( "Presents the details of a interaction channel using Lava" )]

    [CodeEditorField( "Default Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
         <div class='row'>
                        <div class='col-md-6'>
                            {% if InteractionChannel.Name != '' %}
                                <dl>
                               <dt>Name</dt>
                               <dd>{{ InteractionChannel.Name }}<dd/>
                               </dl>
                            {% endif %}
                            {% if InteractionChannel.RetentionDuration != '' %}
                                <dl>
                               <dt>Retention Duration</dt
                               <dd>{{ InteractionChannel.RetentionDuration }}<dd/>
                            </dl>
                            {% endif %}
                        </div>
                        <div class='col-md-6'>
                            {% if InteractionChannel.ChannelTypeMediumValue != null and InteractionChannel.ChannelTypeMediumValue != '' %}
                            <dl>
                               <dt>Name</dt
                               <dd>{{ InteractionChannel.ChannelTypeMediumValue.Value }}<dd/>
                            </dl>
                            {% endif %}
                        </div>
                    </div>
", "", 0 )]
    public partial class InteractionChannelDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Fields

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", InteractionChannel.FriendlyTypeName );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var channelId = Convert.ToInt32( PageParameter( "channelId" ) );
                if ( channelId == 0 )
                {
                    nbWarningMessage.Title = "Missing Channel Information";
                    nbWarningMessage.Text = "<p>Make sure you have navigated to this page from Channel Listing page.</p>";
                    nbWarningMessage.Visible = true;
                    return;
                }
                ShowDetail( channelId );
            }
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
            var channelId = hfChannelId.Value.AsInteger();
            ShowDetail( channelId );
        }


        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            InteractionChannelService interactionChannelService = new InteractionChannelService( new RockContext() );
            InteractionChannel interactionChannel = interactionChannelService.Get( hfChannelId.ValueAsInt() );
            ShowEditDetails( interactionChannel );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            InteractionChannelService interactionChannelService = new InteractionChannelService( rockContext );
            InteractionChannel interactionChannel = interactionChannelService.Get( hfChannelId.Value.AsInteger() );

            if ( interactionChannel != null )
            {
                if ( !interactionChannel.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this account.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !interactionChannelService.CanDelete( interactionChannel, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                interactionChannelService.Delete( interactionChannel );

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            InteractionChannel interactionChannel;
            var rockContext = new RockContext();

            var interactionChannelService = new Rock.Model.InteractionChannelService( rockContext );

            interactionChannel = interactionChannelService.Get( hfChannelId.Value.AsInteger() );

            interactionChannel.Name = tbName.Text;
            interactionChannel.RetentionDuration = nbRetentionDuration.Text.AsIntegerOrNull();
            interactionChannel.ChannelListTemplate = ceChannelList.Text;
            interactionChannel.ChannelDetailTemplate = ceChannelDetail.Text;
            interactionChannel.SessionListTemplate = ceSessionList.Text;
            interactionChannel.ComponentListTemplate = ceComponentList.Text;
            interactionChannel.ComponentDetailTemplate = ceComponentDetail.Text;
            interactionChannel.InteractionListTemplate = ceInteractionList.Text;
            interactionChannel.InteractionDetailTemplate = ceInteractionDetail.Text;

            interactionChannel.ModifiedDateTime = RockDateTime.Now;
            interactionChannel.ModifiedByPersonAliasId = CurrentPersonAliasId;

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["ChannelId"] = interactionChannel.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="channel">The interaction channel.</param>
        private void ShowEditDetails( InteractionChannel channel )
        {
            lTitle.Text = channel.Name.FormatAsHtmlTitle();
            SetEditMode( true );

            tbName.Text = channel.Name;
            nbRetentionDuration.Text = channel.RetentionDuration.ToString();
            ceChannelList.Text = channel.ChannelListTemplate;
            ceChannelDetail.Text = channel.ChannelDetailTemplate;
            ceSessionList.Text = channel.SessionListTemplate;
            ceComponentList.Text = channel.ComponentListTemplate;
            ceComponentDetail.Text = channel.ComponentDetailTemplate;
            ceInteractionList.Text = channel.InteractionListTemplate;
            ceInteractionDetail.Text = channel.InteractionDetailTemplate;
        }

        /// <summary>
        /// Shows the detail with lava.
        /// </summary>
        public void ShowDetail( int channelId )
        {
            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            RockContext rockContext = new RockContext();
            InteractionChannelService interactionChannelService = new InteractionChannelService( rockContext );
            var interactionChannel = interactionChannelService.Queryable().Where( a => a.Id == channelId ).FirstOrDefault();

            viewAllowed = editAllowed || interactionChannel.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = editAllowed || interactionChannel.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;
            if ( !editAllowed )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
            SetEditMode( false );

            hfChannelId.SetValue( interactionChannel.Id );
            lTitle.Text = interactionChannel.Name.FormatAsHtmlTitle();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.AddOrIgnore( "Person", CurrentPerson );
            mergeFields.Add( "InteractionChannel", interactionChannel );

            string template = GetAttributeValue( "DefaultTemplate" );
            if ( !string.IsNullOrEmpty( interactionChannel.ChannelDetailTemplate ) )
            {
                template = interactionChannel.ChannelDetailTemplate;
            }
            lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion

    }
}