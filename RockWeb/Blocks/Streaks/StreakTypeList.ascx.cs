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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak Type List" )]
    [Category( "Streaks" )]
    [Description( "Shows a list of all streak types." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "DDE31844-B024-472E-9B21-E094DFC40CAB" )]
    public partial class StreakTypeList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for the detail page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            /// <summary>
            /// Category for the linked pages
            /// </summary>
            public const string LinkedPages = "Linked Pages";
        }

        /// <summary>
        /// Keys to use for User Preferences
        /// </summary>
        private static class UserPreferenceKey
        {
            /// <summary>
            /// Key for the active user preference
            /// </summary>
            public const string Active = "Active";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// Key for the streak type id
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Initialize Filter
            if ( !Page.IsPostBack )
            {
                BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            // Initialize Grid
            gStreakTypes.DataKeyNames = new string[] { "Id" };
            gStreakTypes.Actions.AddClick += gStreakTypes_Add;
            gStreakTypes.GridRebind += gStreakTypes_GridRebind;
            gStreakTypes.RowItemText = "Streak Type";

            // Initialize Grid: Secured actions
            var canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gStreakTypes.Actions.ShowAdd = canAddEditDelete;
            gStreakTypes.IsDeleteEnabled = canAddEditDelete;

            if ( canAddEditDelete )
            {
                gStreakTypes.RowSelected += gStreakTypes_Edit;
            }

            var securityField = gStreakTypes.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( StreakType ) ).Id;

            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upStreakTypeList );
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

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Control Events

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( UserPreferenceKey.Active, ddlActiveFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.Active:
                    e.Value = ddlActiveFilter.SelectedValue;
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gStreakTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gStreakTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StreakTypeId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gStreakTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStreakTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StreakTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gStreakTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStreakTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var streakTypeService = GetStreakTypeService();

            var streakTypeId = e.RowKeyId;
            var streakType = streakTypeService.Get( streakTypeId );

            if ( streakType == null )
            {
                mdGridWarning.Show( "The streak type could not be found.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            if ( !streakTypeService.CanDelete( streakType, out errorMessage, true ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            streakTypeService.Delete( streakType );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gStreakTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gStreakTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlActiveFilter.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.Active ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Don't use the streak type cache here since the users will expect to see the instant changes to this
            // query when they add, edit, etc
            var streakTypeQuery = GetStreakTypeService().Queryable().AsNoTracking();

            // Filter by: Active
            var activeFilter = rFilter.GetFilterPreference( UserPreferenceKey.Active ).ToLower();

            switch ( activeFilter )
            {
                case "active":
                    streakTypeQuery = streakTypeQuery.Where( s => s.IsActive );
                    break;
                case "inactive":
                    streakTypeQuery = streakTypeQuery.Where( a => !a.IsActive );
                    break;
            }

            // Create view models to display in the grid
            var viewModelQuery = streakTypeQuery.Select( s => new StreakTypeViewModel
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive,
                OccurrenceFrequency = s.OccurrenceFrequency,
                EnrollmentCount = s.Streaks.Count(),
                StartDate = s.StartDate
            } );

            // Sort the view models
            var sortProperty = gStreakTypes.SortProperty;
            if ( sortProperty != null )
            {
                viewModelQuery = viewModelQuery.Sort( sortProperty );
            }
            else
            {
                viewModelQuery = viewModelQuery.OrderBy( vm => vm.Id );
            }

            gStreakTypes.SetLinqDataSource( viewModelQuery );
            gStreakTypes.DataBind();
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the streak type service
        /// </summary>
        /// <returns></returns>
        private StreakTypeService GetStreakTypeService()
        {
            if ( _streakTypeService == null )
            {
                var rockContext = GetRockContext();
                _streakTypeService = new StreakTypeService( rockContext );
            }

            return _streakTypeService;
        }
        private StreakTypeService _streakTypeService = null;

        #endregion Data Interface Methods

        #region View Models

        /// <summary>
        /// Represents an entry in the list of streak types shown on this page.
        /// </summary>
        private class StreakTypeViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public StreakOccurrenceFrequency OccurrenceFrequency { get; set; }
            public int EnrollmentCount { get; set; }
            public DateTime StartDate { get; set; }
        }

        #endregion View Models
    }
}