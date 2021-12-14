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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Presents the details of a interaction channel using Lava
    /// </summary>
    [DisplayName( "Interaction Channel Detail" )]
    [Category( "Reporting" )]
    [Description( "Presents the details of a interaction channel using Lava" )]

    [CodeEditorField( "Default Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"
<div class='row'>
    {% if InteractionChannel.Name != '' %}
        <div class='col-md-6'>
            <dl><dt>Name</dt><dd>{{ InteractionChannel.Name }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.ChannelTypeMediumValue != null and InteractionChannel.ChannelTypeMediumValue != '' %}
        <div class='col-md-6'>
            <dl><dt>Medium</dt><dd>{{ InteractionChannel.ChannelTypeMediumValue.Value }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.EngagementStrength != null and InteractionChannel.EngagementStrength != '' %}
      <div class='col-md-6'>
          <dl><dt>Engagement Strength</dt><dd>{{ InteractionChannel.EngagementStrength }}<dd/></dl>
       </div>
    {% endif %}
    {% if InteractionChannel.RetentionDuration != null %}
        <div class='col-md-6'>
            <dl><dt>Retention Duration</dt><dd>{{ InteractionChannel.RetentionDuration }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.ComponentCacheDuration != null %}
        <div class='col-md-6'>
            <dl><dt>Component Cache Duration</dt><dd>{{ InteractionChannel.ComponentCacheDuration }}<dd/></dl>
        </div>
    {% endif %}
</div>
", "", 0 )]

    public partial class InteractionChannelDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        RockContext _rockContext = null;
        InteractionChannelService _channelService = null;
        InteractionChannel _channel = null;

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
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.InteractionChannel ) ).Id;

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
                ShowDetail( 0 );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            _rockContext = new RockContext();
            _channelService = new InteractionChannelService( _rockContext );
            _channel = _channelService.Get( PageParameter( "ChannelId" ).AsInteger() );

            var breadCrumbs = new List<BreadCrumb>();
            breadCrumbs.Add( new BreadCrumb( _channel != null ? _channel.Name : "Channel", pageReference ) );
            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( 0 );
        }


        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            if ( _channel != null )
            {
                if ( !_channel.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this interaction channel.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !_channelService.CanDelete( _channel, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                _channelService.Delete( _channel );
                _rockContext.SaveChanges();

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( _channel != null )
            {
                _channel.Name = tbName.Text;
                _channel.RetentionDuration = nbRetentionDuration.Text.AsIntegerOrNull();
                _channel.ComponentCacheDuration = nbComponentCacheDuration.Text.AsIntegerOrNull();
                _channel.EngagementStrength = nbEngagementStrength.Text.AsIntegerOrNull();
                _channel.ChannelListTemplate = ceChannelList.Text;
                _channel.ChannelDetailTemplate = ceChannelDetail.Text;
                _channel.SessionListTemplate = ceSessionList.Text;
                _channel.ComponentListTemplate = ceComponentList.Text;
                _channel.ComponentDetailTemplate = ceComponentDetail.Text;
                _channel.InteractionListTemplate = ceInteractionList.Text;
                _channel.InteractionDetailTemplate = ceInteractionDetail.Text;
                _channel.IsActive = cbIsActive.Checked;
                _channel.InteractionCustom1Label = tbChannelCustom1Label.Text;
                _channel.InteractionCustom2Label = tbChannelCustom2Label.Text;
                _channel.InteractionCustomIndexed1Label = tbChannelCustomIndexed1Label.Text;
                _channel.ModifiedDateTime = RockDateTime.Now;
                _channel.ModifiedByPersonAliasId = CurrentPersonAliasId;

                _rockContext.SaveChanges();
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["ChannelId"] = hfChannelId.Value;
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams["ChannelId"] = hfChannelId.Value;
            NavigateToPage( RockPage.Guid, qryParams );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="channel">The interaction channel.</param>
        private void ShowEditDetails()
        {
            if ( _channel != null )
            {
                lTitle.Text = _channel.Name.FormatAsHtmlTitle();
                SetEditMode( true );

                tbName.Text = _channel.Name;
                cbIsActive.Checked = _channel.IsActive;
                nbRetentionDuration.Text = _channel.RetentionDuration.ToString();
                nbComponentCacheDuration.Text = _channel.ComponentCacheDuration.ToString();
                nbEngagementStrength.Text = _channel.EngagementStrength.ToStringSafe();
                ceChannelList.Text = _channel.ChannelListTemplate;
                ceChannelDetail.Text = _channel.ChannelDetailTemplate;
                ceSessionList.Text = _channel.SessionListTemplate;
                ceComponentList.Text = _channel.ComponentListTemplate;
                ceComponentDetail.Text = _channel.ComponentDetailTemplate;
                ceInteractionList.Text = _channel.InteractionListTemplate;
                ceInteractionDetail.Text = _channel.InteractionDetailTemplate;
                tbChannelCustom1Label.Text = _channel.InteractionCustom1Label;
                tbChannelCustom2Label.Text = _channel.InteractionCustom2Label;
                tbChannelCustomIndexed1Label.Text = _channel.InteractionCustomIndexed1Label;
            }
        }

        /// <summary>
        /// Shows the detail with lava.
        /// </summary>
        public void ShowDetail( int channelId )
        {
            bool viewAllowed = false;
            bool editAllowed = UserCanEdit;
            bool adminAllowed = UserCanAdministrate;

            if ( _channel != null )
            {
                viewAllowed = editAllowed || _channel.IsAuthorized( Authorization.VIEW, CurrentPerson );
                editAllowed = editAllowed || _channel.IsAuthorized( Authorization.EDIT, CurrentPerson );

                pnlDetails.Visible = viewAllowed;
                if ( !editAllowed )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                }

                btnSecurity.Visible = UserCanAdministrate || _channel.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                btnSecurity.EntityId = _channel.Id;

                SetEditMode( false );

                hfChannelId.SetValue( _channel.Id );

                lTitle.Text = _channel.Name.FormatAsHtmlTitle();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.AddOrIgnore( "CurrentPerson", CurrentPerson );
                mergeFields.Add( "InteractionChannel", _channel );

                string template = GetAttributeValue( "DefaultTemplate" );
                if ( !string.IsNullOrEmpty( _channel.ChannelDetailTemplate ) )
                {
                    template = _channel.ChannelDetailTemplate;
                }

                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
            }
            else
            {
                lTitle.Text = "Interaction Channel";

                nbWarningMessage.Title = "Missing Channel Information";
                nbWarningMessage.Text = "<p>Make sure you have navigated to this page from Channel Listing page.</p>";
                nbWarningMessage.Visible = true;

                pnlViewDetails.Visible = false;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion

    }
}