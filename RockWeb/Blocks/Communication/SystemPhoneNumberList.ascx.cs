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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// Lists the phone numbers currently in the system.
    /// </summary>
    [DisplayName( "System Phone Number List" )]
    [Category( "Communication" )]
    [Description( "Lists the phone numbers currently in the system." )]

    [LinkedPage( "System Phone Number Detail Page",
        Key = AttributeKey.SystemPhoneNumberDetailPage,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "CCF3C814-5D4E-4BBE-BB69-7FFBF7D1A45D" )]
    public partial class SystemPhoneNumberList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SystemPhoneNumberDetailPage = "SystemPhoneNumberDetailPage";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "SystemPhoneNumberId";
        }

        #endregion

        #region User Preference Keys

        private static class UserPreferenceKey
        {
            public const string ActiveFilter = "ShowInactive";
            public const string SmsEnabledFilter = "ShowSmsEnabled";
        }

        #endregion User Preference Keys

        #region Filter Values

        /// <summary>
        /// Keys to use for Filter Values: Active
        /// </summary>
        private static class IsActiveFilterValueSpecifier
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
        }

        /// <summary>
        /// Keys to use for Filter Values: SMS Enabled
        /// </summary>
        private static class IsSmsEnabledFilterValueSpecifier
        {
            public const string Yes = "Yes";
            public const string No = "No";
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeGrid();

            BlockUpdated += SystemPhoneNumberList_BlockUpdated;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadFilterSelectionLists();
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the SystemPhoneNumberList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SystemPhoneNumberList_BlockUpdated( object sender, EventArgs e )
        {
            InitializeGrid();
            BindGrid();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( UserPreferenceKey.ActiveFilter, ddlActiveFilter.SelectedValue );
            rFilter.SetFilterPreference( UserPreferenceKey.SmsEnabledFilter, ddlSmsEnabledFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gSystemPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSystemPhoneNumbers_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.SystemPhoneNumberDetailPage, PageParameterKey.EntityId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSystemPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSystemPhoneNumbers_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.SystemPhoneNumberDetailPage, PageParameterKey.EntityId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSystemPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSystemPhoneNumbers_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var systemPhoneNumberService = new SystemPhoneNumberService( rockContext );
                var systemPhoneNumber = systemPhoneNumberService.Get( e.RowKeyId );

                if ( systemPhoneNumber == null )
                {
                    ShowMessage( "The system phone number could not be found.", NotificationBoxType.Warning );
                    return;
                }

                if ( !systemPhoneNumberService.CanDelete( systemPhoneNumber, out string errorMessage ) )
                {
                    ShowMessage( errorMessage, NotificationBoxType.Warning );
                    return;
                }

                systemPhoneNumberService.Delete( systemPhoneNumber );

                rockContext.SaveChanges();

                SystemPhoneNumberService.DeleteLegacyPhoneNumber( systemPhoneNumber );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSystemPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSystemPhoneNumbers_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gSystemPhoneNumbers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gSystemPhoneNumbers_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var systemPhoneNumberService = new SystemPhoneNumberService( rockContext );

            var systemPhoneNumbers = systemPhoneNumberService
                .Queryable()
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            if ( systemPhoneNumbers != null )
            {
                new SystemPhoneNumberService( rockContext ).Reorder( systemPhoneNumbers, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the type of the security field entity.
        /// </summary>
        private void SetSecurityFieldEntityType()
        {
            var securityField = gSystemPhoneNumbers.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get<SystemPhoneNumber>().Id;
            }
        }

        /// <summary>
        /// Populate the selection lists for the filter.
        /// </summary>
        private void LoadFilterSelectionLists()
        {
            ddlActiveFilter.Items.Clear();
            ddlActiveFilter.Items.Add( new ListItem() );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Active ) );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Inactive ) );

            ddlSmsEnabledFilter.Items.Clear();
            ddlSmsEnabledFilter.Items.Add( new ListItem() );
            ddlSmsEnabledFilter.Items.Add( new ListItem( IsSmsEnabledFilterValueSpecifier.Yes ) );
            ddlSmsEnabledFilter.Items.Add( new ListItem( IsSmsEnabledFilterValueSpecifier.No) );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlActiveFilter.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.ActiveFilter ) );
            ddlSmsEnabledFilter.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.SmsEnabledFilter ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var systemPhoneNumberService = new SystemPhoneNumberService( rockContext );
            var systemPhoneNumbers = systemPhoneNumberService.Queryable();

            systemPhoneNumbers = ApplyFiltersAndSorting( systemPhoneNumbers );

            gSystemPhoneNumbers.EntityTypeId = EntityTypeCache.Get<SystemPhoneNumber>().Id;
            gSystemPhoneNumbers.SetLinqDataSource( systemPhoneNumbers );
            gSystemPhoneNumbers.DataBind();
        }

        /// <summary>
        /// Applies the filters and sorting.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        private IQueryable<SystemPhoneNumber> ApplyFiltersAndSorting( IQueryable<SystemPhoneNumber> query )
        {
            var activeStatus = rFilter.GetFilterPreference( UserPreferenceKey.ActiveFilter );
            switch ( activeStatus )
            {
                case IsActiveFilterValueSpecifier.Active:
                    query = query.Where( s => s.IsActive );
                    break;
                case IsActiveFilterValueSpecifier.Inactive:
                    query = query.Where( s => !s.IsActive );
                    break;
            }

            var smsEnabledStatus = rFilter.GetFilterPreference( UserPreferenceKey.SmsEnabledFilter );
            switch ( smsEnabledStatus )
            {
                case IsSmsEnabledFilterValueSpecifier.Yes:
                    query = query.Where( s => s.IsSmsEnabled );
                    break;
                case IsSmsEnabledFilterValueSpecifier.No:
                    query = query.Where( s => !s.IsSmsEnabled );
                    break;
            }

            var sortProperty = gSystemPhoneNumbers.SortProperty;
            if ( gSystemPhoneNumbers.AllowSorting && sortProperty != null )
            {
                return query.Sort( sortProperty );
            }
            else
            {
                return query.OrderBy( spn => spn.Order )
                    .ThenBy( spn => spn.Name )
                    .ThenBy( spn => spn.Id );
            }
        }

        private void InitializeGrid()
        {
            gSystemPhoneNumbers.DataKeyNames = new string[] { "Id" };
            gSystemPhoneNumbers.GridRebind += gSystemPhoneNumbers_GridRebind;
            gSystemPhoneNumbers.GridReorder += gSystemPhoneNumbers_GridReorder;

            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            var isDetailPageSet = IsDetailPageSet();

            var canDelete = isUserAuthorized;
            var canAddAndEdit = isUserAuthorized && isDetailPageSet;

            gSystemPhoneNumbers.Actions.ShowAdd = canAddAndEdit;
            gSystemPhoneNumbers.IsDeleteEnabled = canDelete;

            if ( canAddAndEdit )
            {
                gSystemPhoneNumbers.Actions.AddClick += gSystemPhoneNumbers_AddClick;
                gSystemPhoneNumbers.RowSelected += gSystemPhoneNumbers_Edit;
            }

            SetSecurityFieldEntityType();
        }

        /// <summary>
        /// Determines whether the detail page attribute has a value.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the detail page is set; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDetailPageSet()
        {
            return !GetAttributeValue( AttributeKey.SystemPhoneNumberDetailPage ).IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="notificationBoxType">The notification box type.</param>
        private void ShowMessage( string errorMessage, NotificationBoxType notificationBoxType )
        {
            nbMessage.Text = errorMessage;
            nbMessage.NotificationBoxType = notificationBoxType;
            nbMessage.Visible = true;
        }

        #endregion

        #region Support Classes

        #endregion
    }
}
