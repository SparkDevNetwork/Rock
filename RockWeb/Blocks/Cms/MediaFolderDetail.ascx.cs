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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Media Folder Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Media Folder" )]

    [Rock.SystemGuid.BlockTypeGuid( "3C9D442B-D066-43FA-9380-98C60936992E" )]
    public partial class MediaFolderDetail : RockBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string MediaAccountId = "MediaAccountId";
            public const string MediaFolderId = "MediaFolderId";
        }

        #endregion PageParameterKey

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.MediaFolderId ).AsInteger(), PageParameter( PageParameterKey.MediaAccountId ).AsIntegerOrNull() );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? mediaFolderId = PageParameter( pageReference, PageParameterKey.MediaFolderId ).AsIntegerOrNull();
            if ( mediaFolderId != null )
            {
                var rockContext = new RockContext();

                var mediaFolder = new MediaFolderService( rockContext ).Get( mediaFolderId.Value );

                if ( mediaFolder != null )
                {
                    breadCrumbs.Add( new BreadCrumb( mediaFolder.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Media Folder", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var mediaFolder = new MediaFolderService( new RockContext() ).Get( hfId.Value.AsInteger() );
            ShowEditDetails( mediaFolder );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.MediaFolderId ).AsInteger(), PageParameter( PageParameterKey.MediaAccountId ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            var mediaFolderService = new MediaFolderService( rockContext );
            var mediaFolderId = hfId.Value.AsIntegerOrNull();
            MediaFolder mediaFolder = null;
            if ( mediaFolderId.HasValue )
            {
                mediaFolder = mediaFolderService.Get( mediaFolderId.Value );
            }

            var isNew = mediaFolder == null;

            if ( isNew )
            {
                mediaFolder = new MediaFolder()
                {
                    Id = 0,
                    MediaAccountId = hfMediaAccountId.ValueAsInt()
                };
                mediaFolderService.Add( mediaFolder );
            }

            mediaFolder.Name = tbName.Text;
            mediaFolder.Description = tbDescription.Text;
            mediaFolder.IsContentChannelSyncEnabled = swEnableContentChannelSync.Checked;
            mediaFolder.WorkflowTypeId = wtpWorkflowType.SelectedValueAsId();

            if ( swEnableContentChannelSync.Checked )
            {
                var contentChannelId = ContentChannelCache.GetId( ddlContentChannel.SelectedValue.AsGuid() );
                if ( contentChannelId.HasValue )
                {
                    mediaFolder.ContentChannelId = contentChannelId.Value;
                }

                mediaFolder.ContentChannelAttributeId = ddlChannelAttribute.SelectedValueAsInt();
                mediaFolder.ContentChannelItemStatus = rrbContentChannelItemStatus.SelectedValueAsEnum<ContentChannelItemStatus>();
            }
            else
            {
                mediaFolder.ContentChannelId = null;
                mediaFolder.ContentChannelAttributeId = null;
                mediaFolder.ContentChannelItemStatus = null;
            }

            rockContext.SaveChanges();

            var pageReference = RockPage.PageReference;
            pageReference.Parameters.AddOrReplace( PageParameterKey.MediaFolderId, mediaFolder.Id.ToString() );
            Response.Redirect( pageReference.BuildUrl(), false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.Equals( "0" ) )
            {
                // Canceling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString[PageParameterKey.MediaAccountId] = hfMediaAccountId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Canceling on Edit
                var mediaFolder = new MediaFolderService( new RockContext() ).Get( int.Parse( hfId.Value ) );
                ShowReadonlyDetails( mediaFolder );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSyncContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSyncContentChannelItems_Click( object sender, EventArgs e )
        {
            Task.Run( () => MediaFolderService.AddMissingSyncedContentChannelItems( hfId.ValueAsInt() ) );

            mdSyncMessage.Show( "Content channel item creation has started and will continue in the background.", ModalAlertType.Information );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the swEnableContentChannelSync control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void swEnableContentChannelSync_CheckedChanged( object sender, EventArgs e )
        {
            pnlContentChannel.Visible = swEnableContentChannelSync.Checked;
            rrbContentChannelItemStatus.BindToEnum<ContentChannelItemStatus>();
            BindContentChannelDropdown();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            var channelGuid = ddlContentChannel.SelectedValue.AsGuidOrNull();
            UpdateMediaFileAttributeDropdowns( channelGuid );
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Bind content channel dropdown
        /// </summary>
        private void BindContentChannelDropdown()
        {
            ddlContentChannel.Items.Clear();
            ddlContentChannel.Items.Add( new ListItem() );
            foreach ( var contentChannel in ContentChannelCache.All().OrderBy( a => a.Name ) )
            {
                ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Guid.ToString() ) );
            }
        }

        /// <summary>
        /// Updates the media file attribute dropdowns.
        /// </summary>
        /// <param name="channelGuid">The channel unique identifier.</param>
        private void UpdateMediaFileAttributeDropdowns( Guid? channelGuid )
        {
            List<AttributeCache> channelAttributes = new List<AttributeCache>();

            if ( channelGuid.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).GetNoTracking( channelGuid.Value );

                // Fake in-memory content channel item so we can properly
                // load all the attributes.
                var contentChannelItem = new ContentChannelItem
                {
                    ContentChannelId = channel.Id,
                    ContentChannelTypeId = channel.ContentChannelTypeId
                };

                // add content channel item attributes
                contentChannelItem.LoadAttributes( rockContext );
                channelAttributes = contentChannelItem.Attributes.Select( a => a.Value ).ToList();
            }

            ddlChannelAttribute.Items.Clear();
            ddlChannelAttribute.Items.Add( new ListItem() );
            foreach ( var attribute in channelAttributes )
            {
                if ( attribute.FieldType.Class == typeof( Rock.Field.Types.MediaElementFieldType ).FullName )
                {
                    ddlChannelAttribute.Items.Add( new ListItem( attribute.Name, attribute.Id.ToStringSafe() ) );
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        public void ShowDetail( int mediaFolderId )
        {
            ShowDetail( mediaFolderId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        /// <param name="mediaAccountId">The media account identifier.</param>
        private void ShowDetail( int mediaFolderId, int? mediaAccountId )
        {
            var rockContext = new RockContext();
            var mediaFolderService = new MediaFolderService( rockContext );
            MediaFolder mediaFolder = null;

            if ( !mediaAccountId.Equals( 0 ) )
            {
                mediaFolder = mediaFolderService.Get( mediaFolderId );
            }

            if ( mediaFolder == null )
            {
                MediaAccount mediaAccount = null;
                if ( mediaAccountId.HasValue )
                {
                    mediaAccount = new MediaAccountService( rockContext ).Get( mediaAccountId.Value );
                }

                if ( mediaAccount != null )
                {
                    mediaFolder = new MediaFolder { Id = 0, MediaAccountId = mediaAccount.Id };
                }
                else
                {
                    pnlView.Visible = false;
                    return;
                }
            }

            hfId.SetValue( mediaFolder.Id );
            hfMediaAccountId.SetValue( mediaFolder.MediaAccountId );
            pnlView.Visible = true;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnSyncContentChannelItems.Visible = false;

                ShowReadonlyDetails( mediaFolder );
            }
            else
            {
                btnEdit.Visible = true;

                if ( mediaFolder.Id > 0 )
                {
                    btnSyncContentChannelItems.Visible = !( mediaFolder.MediaAccount.GetMediaAccountComponent()?.AllowsManualEntry ?? true );
                    ShowReadonlyDetails( mediaFolder );
                }
                else
                {
                    btnSyncContentChannelItems.Visible = false;
                    ShowEditDetails( mediaFolder );
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="mediaAccount">The media folder.</param>
        private void ShowReadonlyDetails( MediaFolder mediaFolder )
        {
            SetEditMode( false );

            lActionTitle.Text = mediaFolder.Name.FormatAsHtmlTitle();

            hlSyncStatus.Visible = mediaFolder.IsContentChannelSyncEnabled;

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Account", mediaFolder.MediaAccount.Name );

            if ( mediaFolder.IsContentChannelSyncEnabled )
            {
                descriptionList.Add( "Content Channel", mediaFolder.ContentChannel?.Name );
                descriptionList.Add( "Content Channel Attribute", mediaFolder.ContentChannelAttribute?.Name );
                descriptionList.Add( "Content Channel Item Status", mediaFolder.ContentChannelItemStatus?.ConvertToString() );
            }

            if ( mediaFolder.WorkflowTypeId.HasValue )
            {
                descriptionList.Add( "Workflow Type", mediaFolder.WorkflowType?.Name );
            }

            if ( mediaFolder.Description.IsNotNullOrWhiteSpace() )
            {
                descriptionList.Add( "Description", mediaFolder.Description );
            }

            lDescription.Text = descriptionList.Html;

            lMetricData.Text = mediaFolder.MediaAccount.GetMediaAccountComponent()?.GetFolderHtmlSummary( mediaFolder );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="mediaFolder">The media folder.</param>
        private void ShowEditDetails( MediaFolder mediaFolder )
        {
            if ( mediaFolder.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( MediaFolder.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = mediaFolder.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = mediaFolder.Name;
            tbDescription.Text = mediaFolder.Description;
            wtpWorkflowType.SetValue( mediaFolder.WorkflowTypeId );

            swEnableContentChannelSync.Checked = mediaFolder.IsContentChannelSyncEnabled;
            swEnableContentChannelSync_CheckedChanged( null, null );
            if ( mediaFolder.IsContentChannelSyncEnabled )
            {
                if ( mediaFolder.ContentChannel!=null )
                {
                    ddlContentChannel.SetValue( mediaFolder.ContentChannel.Guid );
                }

                ddlContentChannel_SelectedIndexChanged( null, null );
                rrbContentChannelItemStatus.SetValue( mediaFolder.ContentChannelItemStatus.ConvertToInt().ToString() );
                if ( mediaFolder.ContentChannelAttributeId.HasValue )
                {
                    ddlChannelAttribute.SetValue( mediaFolder.ContentChannelAttributeId.Value );
                }
            }

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            var mediaAccount = new MediaAccountService( new RockContext() ).Get( mediaFolder.MediaAccountId );
            var mediaAccountComponent = GetMediaAccountComponent( mediaAccount );
            if ( mediaAccountComponent != null )
            {
                readOnly = !mediaAccountComponent.AllowsManualEntry;
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
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

        /// <summary>
        /// Gets the media account component.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        private MediaAccountComponent GetMediaAccountComponent( MediaAccount mediaAccount )
        {
            var componentEntityTypeId = mediaAccount != null ? mediaAccount.ComponentEntityTypeId : ( int? ) null;

            if ( componentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );
                return componentEntityType == null ? null : MediaAccountContainer.GetComponent( componentEntityType.Name );
            }

            return null;
        }

        #endregion Internal Methods
    }
}