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
using Humanizer;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Media Account Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Media Account" )]

    [Rock.SystemGuid.BlockTypeGuid( "0361FFC9-F32F-4C97-98BD-9DFE5F4A777E" )]
    public partial class MediaAccountDetail : RockBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string MediaAccountId = "MediaAccountId";
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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", MediaAccount.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.MediaAccountId ).AsInteger() );
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

            var mediaAccount = GetMediaAccount();
            if ( mediaAccount != null )
            {
                breadCrumbs.Add( new BreadCrumb( $"{mediaAccount.Name}'s Media", pageReference ) );
            }
            else
            {
                breadCrumbs.Add( new BreadCrumb( "New Media Account", pageReference ) );
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
            var mediaAccount = new MediaAccountService( new RockContext() ).Get( hfMediaAccountId.Value.AsInteger() );
            ShowEditDetails( mediaAccount );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var service = new MediaAccountService( rockContext );
            var mediaAccount = service.Get( int.Parse( hfMediaAccountId.Value ) );

            if ( mediaAccount == null )
            {
                return;
            }

            service.Delete( mediaAccount );

            rockContext.SaveChanges();

            // reload page
            var qryParams = new Dictionary<string, string>();
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpMediaAccountComponent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpMediaAccountComponent_SelectedIndexChanged( object sender, EventArgs e )
        {
            var mediaAccount = GetMediaAccount() ?? new MediaAccount();
            mediaAccount.ComponentEntityTypeId = cpMediaAccountComponent.SelectedEntityTypeId ?? 0;

            RenderComponentAttributeControls( mediaAccount, true );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfMediaAccountId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnSyncWithProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSyncWithProvider_Click( object sender, EventArgs e )
        {
            var mediaAccountId = hfMediaAccountId.ValueAsInt();

            Task.Run( async () =>
            {
                await MediaAccountService.SyncMediaInAccountAsync( mediaAccountId );
                await MediaAccountService.SyncAnalyticsInAccountAsync( mediaAccountId );
            } );

            mdSyncMessage.Show( "Synchronization with provider started and will continue in the background.", ModalAlertType.Information );
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
            var mediaAccount = GetMediaAccount( rockContext );
            var mediaAccountService = new MediaAccountService( rockContext );
            var isNew = mediaAccount == null;

            if ( isNew )
            {
                mediaAccount = new MediaAccount();
                mediaAccountService.Add( mediaAccount );
            }

            mediaAccount.Name = tbName.Text;
            mediaAccount.IsActive = cbActive.Checked;
            mediaAccount.ComponentEntityTypeId = cpMediaAccountComponent.SelectedEntityTypeId ?? 0;
            rockContext.SaveChanges();

            mediaAccount.LoadAttributes( rockContext );
            avcComponentAttributes.GetEditValues( mediaAccount );
            mediaAccount.SaveAttributeValues( rockContext );
            rockContext.SaveChanges();

            var pageReference = RockPage.PageReference;
            pageReference.Parameters.AddOrReplace( PageParameterKey.MediaAccountId, mediaAccount.Id.ToString() );
            Response.Redirect( pageReference.BuildUrl(), false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfMediaAccountId.Value.Equals( "0" ) )
            {
                // Cancelling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString[PageParameterKey.MediaAccountId] = hfMediaAccountId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit
                var mediaAccount = new MediaAccountService( new RockContext() ).Get( int.Parse( hfMediaAccountId.Value ) );
                ShowReadonlyDetails( mediaAccount );
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier.</param>
        public void ShowDetail( int mediaAccountId )
        {
            MediaAccount mediaAccount = null;

            if ( !mediaAccountId.Equals( 0 ) )
            {
                mediaAccount = GetMediaAccount();
            }

            if ( mediaAccount == null )
            {
                mediaAccount = new MediaAccount { Id = 0, IsActive = true };
            }

            hfMediaAccountId.Value = mediaAccount.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaAccount.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnSyncWithProvider.Visible = false;

                ShowReadonlyDetails( mediaAccount );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                if ( mediaAccount.Id > 0 )
                {
                    btnSyncWithProvider.Visible = !( mediaAccount.GetMediaAccountComponent()?.AllowsManualEntry ?? true );
                    ShowReadonlyDetails( mediaAccount );
                }
                else
                {
                    btnSyncWithProvider.Visible = false;
                    ShowEditDetails( mediaAccount );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        private void ShowEditDetails( MediaAccount mediaAccount )
        {
            if ( mediaAccount.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( MediaAccount.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = mediaAccount.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !mediaAccount.IsActive;

            SetEditMode( true );

            tbName.Text = mediaAccount.Name;
            cbActive.Checked = mediaAccount.IsActive;

            if ( mediaAccount.ComponentEntityType != null )
            {
                cpMediaAccountComponent.SetValue( mediaAccount.ComponentEntityType.Guid.ToString().ToUpper() );
            }
            else
            {
                cpMediaAccountComponent.SetValue( mediaAccount.ComponentEntityType != null ? mediaAccount.ComponentEntityType.Guid.ToString().ToUpper() : string.Empty );
            }

            RenderComponentAttributeControls( mediaAccount, true );
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
        /// Shows the readonly details.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        private void ShowReadonlyDetails( MediaAccount mediaAccount )
        {
            SetEditMode( false );

            lActionTitle.Text = mediaAccount.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !mediaAccount.IsActive;
            hlLastRefresh.Visible = mediaAccount.LastRefreshDateTime.HasValue;
            if ( mediaAccount.LastRefreshDateTime.HasValue )
            {
                var refreshTimeSpan = RockDateTime.Now - mediaAccount.LastRefreshDateTime.Value;

                hlLastRefresh.Text = "Last Refreshed: " + refreshTimeSpan.Humanize();
            }

            var mediaAccountComponent = mediaAccount.GetMediaAccountComponent();
            var descriptionList = new DescriptionList();

            if ( mediaAccountComponent != null )
            {
                descriptionList.Add( "Type", mediaAccountComponent.EntityType.FriendlyName );
            }

            lDescription.Text = descriptionList.Html;
            lMetricData.Text = mediaAccountComponent?.GetAccountHtmlSummary( mediaAccount );
        }

        /// <summary>
        /// Renders the component attribute controls.
        /// </summary>
        private void RenderComponentAttributeControls( MediaAccount mediaAccount, bool setValues )
        {
            mediaAccount.LoadAttributes();
            avcComponentAttributes.ExcludedAttributes = mediaAccount.Attributes.Values.Where( a => a.Key == "Order" || a.Key == "Active" ).ToArray();
            avcComponentAttributes.AddEditControls( mediaAccount, setValues );
        }

        /// <summary>
        /// Get the actual media account model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private MediaAccount GetMediaAccount( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var mediaAccountService = new MediaAccountService( rockContext );

            var mediaAccountId = hfMediaAccountId.Value.AsIntegerOrNull() ?? PageParameter( PageParameterKey.MediaAccountId ).AsIntegerOrNull();
            if ( !mediaAccountId.HasValue )
            {
                return null;
            }

            return mediaAccountService.Queryable().FirstOrDefault( a => a.Id == mediaAccountId.Value );
        }

        #endregion Internal Methods
    }
}