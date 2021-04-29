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
    [DisplayName( "Media Element List" )]
    [Category( "CMS" )]
    [Description( "List Media Elements" )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    public partial class MediaElementList : RockBlock, ICustomGridColumns
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
            public const string MediaElementId = "MediaElementId";
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

        private MediaFolder _mediaFolder = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _mediaFolder = GetMediaFolder();
            if ( _mediaFolder != null )
            {
                var mediaAccountComponent = GetMediaAccountComponent();

                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

                gElementList.DataKeyNames = new string[] { "Id" };
                gElementList.Actions.ShowAdd = canAddEditDelete && mediaAccountComponent != null && mediaAccountComponent.AllowsManualEntry;
                gElementList.IsDeleteEnabled = canAddEditDelete;
                gElementList.Actions.AddClick += gElementList_AddClick;
                gElementList.GridRebind += gElementList_GridRebind;
                gElementList.EntityTypeId = EntityTypeCache.Get<MediaElement>().Id;

                gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
                gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;
                gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;

                lTitle.Text = ( _mediaFolder.Name + " Elements" ).FormatAsHtmlTitle();
            }

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
                if ( _mediaFolder != null )
                {
                    BindGrid();
                }
                else
                {
                    pnlView.Visible = false;
                }
            }
        }

        #endregion Base Control Methods

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( UserPreferenceKey.Name, txtElementName.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteUserPreferences();
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
            if ( _mediaFolder != null )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gElementList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gElementList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gElementList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gElementList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaElementId, 0, PageParameterKey.MediaFolderId, _mediaFolder.Id );
        }

        /// <summary>
        /// Handles the RowSelected event of the gElementList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gElementList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaElementId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gElementList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gElementList_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            var mediaElement = mediaElementService.Get( e.RowKeyId );
            if ( mediaElement != null )
            {
                string errorMessage;
                if ( !mediaElementService.CanDelete( mediaElement, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                mediaElementService.Delete( mediaElement );
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
            txtElementName.Text = gfFilter.GetUserPreference( UserPreferenceKey.Name );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );

            // Use AsNoTracking() since these records won't be modified, and therefore don't need to be tracked by the EF change tracker
            var qry = mediaElementService.Queryable().AsNoTracking().Where( a => a.MediaFolderId == _mediaFolder.Id );

            // name filter
            string nameFilter = gfFilter.GetUserPreference( UserPreferenceKey.Name );
            if ( !string.IsNullOrEmpty( nameFilter ) )
            {
                qry = qry.Where( a => a.Name.Contains( nameFilter ) );
            }

            var sortProperty = gElementList.SortProperty;
            if ( gElementList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Name );
            }

            gElementList.EntityTypeId = EntityTypeCache.GetId<MediaElement>();
            gElementList.DataSource = qry.ToList();
            gElementList.DataBind();
        }

        /// <summary>
        /// Gets the media account component.
        /// </summary>
        /// <returns></returns>
        private MediaAccountComponent GetMediaAccountComponent()
        {
            var componentEntityTypeId = _mediaFolder != null && _mediaFolder.MediaAccount != null ? _mediaFolder.MediaAccount.ComponentEntityTypeId : ( int? ) null;

            if ( componentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );
                return componentEntityType == null ? null : MediaAccountContainer.GetComponent( componentEntityType.Name );
            }

            return null;
        }

        /// <summary>
        /// Get the actual media folder model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private MediaFolder GetMediaFolder( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var mediaFolderService = new MediaFolderService( rockContext );

            var mediaFolderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull();

            if ( !mediaFolderId.HasValue )
            {
                return null;
            }

            return mediaFolderService.Queryable( "MediaAccount" ).FirstOrDefault( a => a.Id == mediaFolderId.Value );
        }

        #endregion Methods
    }
}