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
    /// User control for managing the system emails
    /// </summary>
    [DisplayName( "Snippet List" )]
    [Category( "Communication" )]
    [Description( "Lists the snippets currently in the system." )]

    [LinkedPage( "Snippet Detail",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [CustomDropdownListField( "Snippet Type",
        Description = "Determines what type of snippet to filter on. This is required (only one type can be displayed at a time).",
        ListSource = "SELECT [Guid] as [Value], [Name] as [Text] FROM [SnippetType]",
        IsRequired = true,
        Key = AttributeKey.SnippetType,
        Order = 1 )]

    [BooleanField( "Show Personal Column",
        Key = AttributeKey.ShowPersonalColumn,
        Description = "Determines if the personal column is displayed. Not all types will support this..",
        DefaultBooleanValue = false,
        Order = 2 )]

    [Rock.SystemGuid.BlockTypeGuid( "2EDAD934-6129-480B-9812-4BA7B9978AD2" )]
    public partial class SnippetList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SnippetType = "SnippetType";
            public const string DetailPage = "SnippetDetail";
            public const string ShowPersonalColumn = "ShowPersonalColumn";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "SnippetId";
        }

        #endregion

        #region User Preference Keys

        private static class UserPreferenceKey
        {
            public const string OwnershipTypeFilter = "OwnershipType";
            public const string ActiveFilter = "ShowInactive";
            public const string CategoryFilter = "Category";
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
        /// Keys to use for Filter Values: Personal
        /// </summary>
        private static class IsPersonalFilterValueSpecifier
        {
            public const string Personal = "Personal";
            public const string Shared = "Shared";
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

            BlockUpdated += SnippetList_BlockUpdated;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue; ;
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
                SetBlockTitle();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the SnippetList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SnippetList_BlockUpdated( object sender, EventArgs e )
        {
            InitializeGrid();
            BindGrid();
            ToggleTypeFilterVisibility();
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
            rFilter.SetFilterPreference( UserPreferenceKey.OwnershipTypeFilter, ddlTypeFilter.SelectedValue );
            rFilter.SetFilterPreference( UserPreferenceKey.ActiveFilter, ddlActiveFilter.SelectedValue );
            var categoryId = cpCategory.SelectedValueAsInt();
            rFilter.SetFilterPreference( UserPreferenceKey.CategoryFilter, categoryId.HasValue ? categoryId.Value.ToString() : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {

                case UserPreferenceKey.OwnershipTypeFilter:
                    e.Name = "Ownership Type";
                    break;
                case UserPreferenceKey.ActiveFilter:
                    e.Name = "Is Active";
                    e.Value = e.Value == IsActiveFilterValueSpecifier.Active ? "True" : "False";
                    break;

                case UserPreferenceKey.CategoryFilter:
                    var categoryId = e.Value.AsIntegerOrNull();
                    if ( categoryId.HasValue )
                    {
                        e.Value = CategoryCache.Get( categoryId.Value )?.Name;
                    }
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gSnippets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSnippets_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.EntityId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSnippets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSnippets_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.EntityId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSnippets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSnippets_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var snippetService = new SnippetService( rockContext );
            var snippet = snippetService.Get( e.RowKeyId );

            if ( snippet == null )
            {
                ShowMessage( "The snippet type could not be found.", NotificationBoxType.Warning );
                return;
            }

            if ( snippetService.CanDelete( snippet, out string errorMessage ) )
            {
                ShowMessage( errorMessage, NotificationBoxType.Warning );
                return;
            }

            snippetService.Delete( snippet );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSnippets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSnippets_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the type of the security field entity.
        /// </summary>
        private void SetSecurityFieldEntityType()
        {
            var securityField = gSnippets.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get<Snippet>().Id;
            }
        }

        /// <summary>
        /// Sets the block title.
        /// </summary>
        private void SetBlockTitle()
        {
            var snippetTypeName = new SnippetTypeService( new RockContext() ).GetSelect( GetAttributeValue( AttributeKey.SnippetType ).AsGuid(), s => s.Name );
            if ( snippetTypeName.IsNotNullOrWhiteSpace() )
            {
                lTitle.Text = $"{snippetTypeName} Snippets";
            }
        }

        /// <summary>
        /// Populate the selection lists for the filter.
        /// </summary>
        private void LoadFilterSelectionLists()
        {
            ddlTypeFilter.Items.Clear();
            ddlTypeFilter.Items.Add( new ListItem() );
            ddlTypeFilter.Items.Add( new ListItem( IsPersonalFilterValueSpecifier.Personal ) );
            ddlTypeFilter.Items.Add( new ListItem( IsPersonalFilterValueSpecifier.Shared ) );

            ddlActiveFilter.Items.Clear();
            ddlActiveFilter.Items.Add( new ListItem() );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Active ) );
            ddlActiveFilter.Items.Add( new ListItem( IsActiveFilterValueSpecifier.Inactive ) );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlActiveFilter.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.ActiveFilter ) );
            ddlTypeFilter.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.OwnershipTypeFilter ) );
            cpCategory.SetValue( rFilter.GetFilterPreference( UserPreferenceKey.CategoryFilter ).AsIntegerOrNull() );
            ToggleTypeFilterVisibility();
        }

        /// <summary>
        /// Toggles the type filter visibility.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ToggleTypeFilterVisibility()
        {
            ddlTypeFilter.Visible = GetAttributeValue( AttributeKey.ShowPersonalColumn ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            TogglePersonalColumnVisibility();

            var rockContext = new RockContext();
            var snippetService = new SnippetService( rockContext );
            var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuid();

            var snippets = snippetService.Queryable().Where( s => s.SnippetType.Guid == snippetTypeGuid ).Select( s => new SnippetListViewModel()
            {
                Description = s.Description,
                Id = s.Id,
                IsActive = s.IsActive,
                Name = s.Name,
                Personal = s.OwnerPersonAliasId.HasValue,
                CategoryId = s.CategoryId,
            } );

            snippets = ApplyFiltersAndSorting( snippets );

            gSnippets.EntityTypeId = EntityTypeCache.Get<Snippet>().Id;
            gSnippets.SetLinqDataSource( snippets );
            gSnippets.DataBind();
        }

        /// <summary>
        /// Applies the filters and sorting.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        private IQueryable<SnippetListViewModel> ApplyFiltersAndSorting( IQueryable<SnippetListViewModel> query )
        {
            var ownershipType = rFilter.GetFilterPreference( UserPreferenceKey.OwnershipTypeFilter );
            switch ( ownershipType )
            {
                case IsPersonalFilterValueSpecifier.Shared:
                    query = query.Where( s => !s.Personal );
                    break;
                case IsPersonalFilterValueSpecifier.Personal:
                    query = query.Where( s => s.Personal );
                    break;
            }

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

            var categoryId = rFilter.GetFilterPreference( UserPreferenceKey.CategoryFilter ).AsIntegerOrNull();
            if ( categoryId.HasValue )
            {
                query = query.Where( s => s.CategoryId == categoryId );
            }

            var sortProperty = gSnippets.SortProperty;
            if ( gSnippets.AllowSorting && sortProperty != null )
            {
                return query.Sort( sortProperty );
            }
            else
            {
                return query.OrderBy( w => w.Name );
            }
        }

        /// <summary>
        /// Adds the dymanic columns.
        /// </summary>
        private void TogglePersonalColumnVisibility()
        {
            var personalColumn = gSnippets.Columns.OfType<BoolField>().FirstOrDefault( b => b.DataField == "Personal" );

            if ( personalColumn != null )
            {
                var showPersonalColumn = GetAttributeValue( AttributeKey.ShowPersonalColumn ).AsBoolean();
                personalColumn.Visible = showPersonalColumn;
            }
        }

        private void InitializeGrid()
        {
            gSnippets.DataKeyNames = new string[] { "Id" };
            gSnippets.GridRebind += gSnippets_GridRebind;

            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            var isDetailPageSet = IsDetailPageSet();

            var canDelete = isUserAuthorized;
            var canAddAndEdit = isUserAuthorized && isDetailPageSet;

            gSnippets.Actions.ShowAdd = canAddAndEdit;
            gSnippets.IsDeleteEnabled = canDelete;

            if ( canAddAndEdit )
            {
                gSnippets.Actions.AddClick += gSnippets_AddClick;
                gSnippets.RowSelected += gSnippets_Edit;
            }

            SetSecurityFieldEntityType();
        }

        /// <summary>
        /// Determines whether the detail page attribute has a value.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is detail page set]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDetailPageSet()
        {
            return !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
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

        /// <summary>
        /// View model for <see cref="gSnippets"/> grid data
        /// </summary>
        private sealed class SnippetListViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Personal { get; set; }
            public bool IsActive { get; set; }
            public int? CategoryId { get; set; }
        }

        #endregion
    }
}