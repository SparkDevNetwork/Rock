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
    [DisplayName( "Media Account List" )]
    [Category( "CMS" )]
    [Description( "List Media Accounts" )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "7537AB61-F80B-43B1-998B-1D2B03303B36" )]
    public partial class MediaAccountList : RockBlock, ICustomGridColumns
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
        }

        #endregion Page Parameter Keys

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The Account Type
            /// </summary>
            public const string AccountType = "Account Type";

            /// <summary>
            /// The Name
            /// </summary>
            public const string Name = "Name";

            /// <summary>
            /// The Name
            /// </summary>
            public const string IncludeInactive = "Include Inactive";
        }

        #endregion UserPreferanceKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gAccountList.DataKeyNames = new string[] { "Id" };
            gAccountList.Actions.ShowAdd = canAddEditDelete;
            gAccountList.Actions.AddClick += gAccountList_AddClick;
            gAccountList.GridRebind += gAccountList_GridRebind;
            gAccountList.EntityTypeId = EntityTypeCache.Get<MediaAccount>().Id;
            gAccountList.ShowConfirmDeleteDialog = false;

            gfAccounts.ApplyFilterClick += gfAccounts_ApplyFilterClick;
            gfAccounts.DisplayFilterValue += gfAccounts_DisplayFilterValue;
            gfAccounts.ClearFilterClick += gfAccounts_ClearFilterClick;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('table.js-grid-accounts a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        var accountName = $btn.closest('tr').find('.js-name-account').text();
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you wish to delete the \''+ accountName +'\' account. This will delete all folder and media files from Rock.', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gAccountList, gAccountList.GetType(), "deleteRequestScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfAccounts_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAccounts.SetFilterPreference( UserPreferenceKey.AccountType, cpMediaAccountComponent.SelectedValue );
            gfAccounts.SetFilterPreference( UserPreferenceKey.IncludeInactive, cbShowInactive.Checked ? cbShowInactive.Checked.ToString() : string.Empty );
            gfAccounts.SetFilterPreference( UserPreferenceKey.Name, txtAccountName.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAccounts_ClearFilterClick( object sender, EventArgs e )
        {
            gfAccounts.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfAccounts_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.AccountType:
                    var entityType = EntityTypeCache.Get( cpMediaAccountComponent.SelectedValue.AsGuid() );
                    if ( entityType != null )
                    {
                        e.Value = entityType.FriendlyName;
                    }
                    break;
                case UserPreferenceKey.IncludeInactive:
                    var includeFilterValue = e.Value.AsBooleanOrNull();
                    if ( includeFilterValue.HasValue && includeFilterValue.Value )
                    {
                        e.Value = includeFilterValue.Value.ToYesNo();
                    }
                    break;
            }
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
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gAccountList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaAccountId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.MediaAccountId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountList_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var mediaAccountService = new MediaAccountService( rockContext );
            var mediaAccount = mediaAccountService.Get( e.RowKeyId );
            if ( mediaAccount != null )
            {
                string errorMessage;
                if ( !mediaAccountService.CanDelete( mediaAccount, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                mediaAccountService.Delete( mediaAccount );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtAccountName.Text = gfAccounts.GetFilterPreference( UserPreferenceKey.Name );
            cpMediaAccountComponent.SetValue( gfAccounts.GetFilterPreference( UserPreferenceKey.AccountType ) );
            cbShowInactive.Checked = gfAccounts.GetFilterPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var mediaAccountService = new MediaAccountService( rockContext );

            // Use AsNoTracking() since these records won't be modified, and therefore don't need to be tracked by the EF change tracker
            var qry = mediaAccountService.Queryable().AsNoTracking();

            // name filter
            string nameFilter = gfAccounts.GetFilterPreference( UserPreferenceKey.Name );
            if ( !string.IsNullOrEmpty( nameFilter ) )
            {
                qry = qry.Where( account => account.Name.Contains( nameFilter ) );
            }

            Guid? accountTypeGuid = gfAccounts.GetFilterPreference( UserPreferenceKey.AccountType ).AsGuidOrNull();
            if ( accountTypeGuid.HasValue )
            {
                qry = qry.Where( l => l.ComponentEntityType.Guid.Equals( accountTypeGuid.Value ) );
            }

            bool showInactiveAccounts = gfAccounts.GetFilterPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();

            if ( !showInactiveAccounts )
            {
                qry = qry.Where( s => s.IsActive == true );
            }

            var selectQry = qry
                .Select( a => new
                {
                    a.Id,
                    a.Name,
                    Type = a.ComponentEntityType,
                    a.LastRefreshDateTime,
                    Folders = a.MediaFolders.Count,
                    Videos = a.MediaFolders.SelectMany( b => b.MediaElements ).Count()
                } );

            var sortProperty = gAccountList.SortProperty;
            if ( gAccountList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.Name );
            }

            gAccountList.EntityTypeId = EntityTypeCache.GetId<MediaAccount>();
            gAccountList.DataSource = selectQry.ToList();
            gAccountList.DataBind();
        }

        #endregion
    }
}