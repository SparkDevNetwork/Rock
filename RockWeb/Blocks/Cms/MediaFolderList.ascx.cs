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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Media Folder List" )]
    [Category( "CMS" )]
    [Description( "List Media Folders" )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "02A91579-9355-45E7-A67A-56E998FB332A" )]
    public partial class MediaFolderList : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string MediaAccountId = "MediaAccountId";
            public const string MediaFolderId = "MediaFolderId";
        }

        #endregion Page Parameter Keys

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The Name
            /// </summary>
            public const string Name = "Name";
        }

        #endregion UserPreferanceKeys

        #region Private Variables

        private MediaAccount _mediaAccount = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _mediaAccount = GetMediaAccount();
            if ( _mediaAccount != null )
            {
                var mediaAccountComponent = GetMediaAccountComponent();

                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

                gFolderList.DataKeyNames = new string[] { "Id" };
                gFolderList.Actions.ShowAdd = canAddEditDelete && mediaAccountComponent != null && mediaAccountComponent.AllowsManualEntry;
                gFolderList.IsDeleteEnabled = canAddEditDelete;
                gFolderList.Actions.AddClick += gFolderList_AddClick;
                gFolderList.GridRebind += gFolderList_GridRebind;
                gFolderList.EntityTypeId = EntityTypeCache.Get<MediaFolder>().Id;
                gFolderList.ShowConfirmDeleteDialog = false;

                gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
                gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;
                gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;

                lTitle.Text = ( _mediaAccount.Name + " Folders" ).FormatAsHtmlTitle();
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('table.js-grid-folders a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        var folderName = $btn.closest('tr').find('.js-name-folder').text();
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you wish to delete the '+ folderName +' folder. This will delete all related media files from Rock.', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gFolderList, gFolderList.GetType(), "deleteRequestScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _mediaAccount != null )
                {
                    BindGrid();
                }
                else
                {
                    pnlView.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( UserPreferenceKey.Name, txtFolderName.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the filter display for each saved user value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
        }

        #endregion Filter Events

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( _mediaAccount != null )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gFolderList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFolderList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            dynamic folderRow = e.Row.DataItem;
            if ( folderRow == null )
            {
                return;
            }

            var lWatchCount = e.Row.FindControl( "lWatchCount" ) as Literal;
            if ( lWatchCount != null )
            {
                var folderId = (int)folderRow.Id;
                var rockContext = new RockContext();
                var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() );
                var mediaElementIdQry = new MediaElementService( rockContext ).Queryable().Where( a => a.MediaFolderId == folderId ).Select( a => ( int? ) a.Id );
                var interactionComponentQry = new InteractionComponentService( rockContext )
                    .Queryable()
                    .Where( c => c.InteractionChannelId == interactionChannelId && mediaElementIdQry.Contains( c.EntityId ) )
                    .Select( a => a.Id );
                var watchCountQry = new InteractionService( rockContext )
                    .Queryable()
                    .Where( a => a.Operation == "Watch" );
                lWatchCount.Text = watchCountQry.Where( b => interactionComponentQry.Contains( b.InteractionComponentId ) ).Count().ToStringSafe();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gFolderList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gFolderList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gFolderList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gFolderList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaFolderId, 0, PageParameterKey.MediaAccountId, _mediaAccount.Id );
        }

        /// <summary>
        /// Handles the RowSelected event of the gFolderList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFolderList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaFolderId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gFolderList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFolderList_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var mediaFolderService = new MediaFolderService( rockContext );
            var mediaFolder = mediaFolderService.Get( e.RowKeyId );
            if ( mediaFolder != null )
            {
                string errorMessage;
                if ( !mediaFolderService.CanDelete( mediaFolder, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                mediaFolderService.Delete( mediaFolder );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtFolderName.Text = gfFilter.GetFilterPreference( UserPreferenceKey.Name );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var mediaFolderService = new MediaFolderService( rockContext );

            // Use AsNoTracking() since these records won't be modified, and therefore don't need to be tracked by the EF change tracker
            var qry = mediaFolderService.Queryable().AsNoTracking().Where( a => a.MediaAccountId == _mediaAccount.Id );

            // name filter
            string nameFilter = gfFilter.GetFilterPreference( UserPreferenceKey.Name );
            if ( !string.IsNullOrEmpty( nameFilter ) )
            {
                qry = qry.Where( a => a.Name.Contains( nameFilter ) );
            }

            var selectQry = qry
                .Select( a => new
                {
                    a.Id,
                    a.Name,
                    ContentChannel = a.ContentChannel,
                    Videos = a.MediaElements.Count
                } );

            var sortProperty = gFolderList.SortProperty;
            if ( gFolderList.AllowSorting && sortProperty != null )
            {
                selectQry = selectQry.Sort( sortProperty );
            }
            else
            {
                selectQry = selectQry.OrderBy( a => a.Name );
            }

            gFolderList.EntityTypeId = EntityTypeCache.GetId<MediaFolder>();
            gFolderList.DataSource = selectQry.ToList();
            gFolderList.DataBind();
        }

        /// <summary>
        /// Gets the media account component.
        /// </summary>
        /// <returns></returns>
        private MediaAccountComponent GetMediaAccountComponent()
        {
            var componentEntityTypeId = _mediaAccount != null ? _mediaAccount.ComponentEntityTypeId : ( int? ) null;

            if ( componentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );
                return componentEntityType == null ? null : MediaAccountContainer.GetComponent( componentEntityType.Name );
            }

            return null;
        }

        /// <summary>
        /// Get the actual media account model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private MediaAccount GetMediaAccount( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var mediaAccountService = new MediaAccountService( rockContext );

            var mediaAccountId = PageParameter( PageParameterKey.MediaAccountId ).AsIntegerOrNull();

            if ( !mediaAccountId.HasValue )
            {
                return null;
            }

            return mediaAccountService.Queryable().FirstOrDefault( a => a.Id == mediaAccountId.Value );
        }

        #endregion Methods
    }
}