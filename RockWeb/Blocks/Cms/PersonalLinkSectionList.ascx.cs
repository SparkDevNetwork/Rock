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
    [DisplayName( "Personal Link Section List" )]
    [Category( "CMS" )]
    [Description( "Lists personal link section in the system." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [BooleanField(
        "Shared Sections",
        Description = "When enabled, only shared sections will be displayed.",
        Key = AttributeKey.SharedSections,
        Order = 1 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "0BFD74A8-1888-4407-9102-D3FCEABF3095" )]
    public partial class PersonalLinkSectionList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string SharedSections = "SharedSection";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SectionId = "SectionId";
        }

        #endregion PageParameterKey

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

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var limitToSharedSections = GetAttributeValue( AttributeKey.SharedSections ).AsBoolean();
            var reorderField = gSectionList.ColumnsOfType<ReorderField>().FirstOrDefault();
            bool allowReordering = !limitToSharedSections;
            if ( reorderField != null )
            {
                // only show the reorder if showing both shared and non-shared (the sort is specific to each person)
                reorderField.Visible = allowReordering;
            }

            // only show the filter if we aren't allowing re-ordering
            gfFilter.Visible = !allowReordering;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            gSectionList.DataKeyNames = new string[] { "Id" };
            gSectionList.Actions.ShowAdd = true;
            gSectionList.IsDeleteEnabled = true;
            gSectionList.RowSelected += gSectionList_Edit;
            gSectionList.Actions.AddClick += gSectionList_Add;
            gSectionList.GridReorder += gSectionList_GridReorder;
            gSectionList.GridRebind += gSectionList_GridRebind;
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

        #endregion

        #region Events 

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SectionId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Edit( object sender, RowEventArgs e )
        {
            var section = new PersonalLinkSectionService( new RockContext() ).Get( e.RowKeyId );

            if ( section == null )
            {
                return;
            }

            if ( section.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SectionId, e.RowKeyId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );
            var personalLinkSection = personalLinkSectionService.Get( e.RowKeyId );

            if ( personalLinkSection != null )
            {
                string errorMessage;
                if ( !personalLinkSectionService.CanDelete( personalLinkSection, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                personalLinkSectionService.Delete( personalLinkSection );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSectionList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gSectionList_GridReorder( object sender, GridReorderEventArgs e )
        {
            if ( GetAttributeValue( AttributeKey.SharedSections ).AsBoolean() )
            {
                // The event shouldn't get called in this situation, but just in case...
                // Re-ordering allow applies when showing all shared and non-shared sections
                return;
            }

            var rockContext = new RockContext();
            var gridDataSourceList = GetGridDataSourceList( rockContext );

            var sectionOrderList = gridDataSourceList.Select( a => a.PersonalLinkSectionOrder ).ToList();

            new PersonalLinkSectionOrderService( rockContext ).Reorder( sectionOrderList, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the gfFilter control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( UserPreferenceKey.Name, txtSectionName.Text );
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
        /// Handles the RowDataBound event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gSectionList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var personalLinkSectionViewModel = e.Row.DataItem as PersonalLinkSectionViewModel;

            if ( personalLinkSectionViewModel == null )
            {
                return;
            }

            if ( personalLinkSectionViewModel.CanEdit == false )
            {
                // if this section isn't editable, remove the 'grid-select-cell' from the cells in that row (so that the hand icon doesn't show)
                foreach ( var selectableCell in e.Row.Cells.OfType<DataControlFieldCell>().Where( a => a.HasCssClass( "grid-select-cell" ) ) )
                {
                    selectableCell.RemoveCssClass( "grid-select-cell" );
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtSectionName.Text = gfFilter.GetFilterPreference( UserPreferenceKey.Name );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var gridDataSourceList = GetGridDataSourceList( new RockContext() );

            gSectionList.DataSource = gridDataSourceList;
            gSectionList.DataBind();
        }

        /// <summary>
        /// Gets the grid data source list (ordered)
        /// </summary>
        /// <returns>List&lt;PersonalLinkSectionViewModel&gt;.</returns>
        private List<PersonalLinkSectionViewModel> GetGridDataSourceList( RockContext rockContext )
        {
            var limitToSharedSections = GetAttributeValue( AttributeKey.SharedSections ).AsBoolean();
            List<PersonalLinkSection> personalLinkSectionList;
            Dictionary<int, PersonalLinkSectionOrder> currentPersonSectionOrderLookupBySectionId = null;

            if ( limitToSharedSections )
            {
                // only show shared sections in this mode
                var sharedPersonalLinkSectionsQuery = new PersonalLinkSectionService( rockContext ).Queryable().Where( a => a.IsShared );
                personalLinkSectionList = sharedPersonalLinkSectionsQuery.Include( a => a.PersonalLinks ).OrderBy( a => a.Name ).AsNoTracking().ToList();
            }
            else
            {
                // show both shared and non-shared, but don't let shared sections get deleted (even if authorized)
                var personalLinkService = new PersonalLinkService( rockContext );
                if ( personalLinkService.AddMissingPersonalLinkSectionOrders( this.CurrentPerson ) )
                {
                    rockContext.SaveChanges();
                }

                var orderedPersonalLinkSectionsQuery = new PersonalLinkService( rockContext ).GetOrderedPersonalLinkSectionsQuery( this.CurrentPerson );

                personalLinkSectionList = orderedPersonalLinkSectionsQuery
                    .Include( a => a.PersonalLinks )
                    .AsNoTracking()
                    .ToList()
                    .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                    .ToList();

                // NOTE: We might be making changes when resorting this, so don't use AsNoTracking()
                var sectionOrderQuery = personalLinkService.GetSectionOrderQuery( this.CurrentPerson );
                currentPersonSectionOrderLookupBySectionId = sectionOrderQuery.ToDictionary( k => k.SectionId, v => v );
            }

            gSectionList.EntityTypeId = EntityTypeCache.GetId<PersonalLinkSection>();

            var viewModelList = personalLinkSectionList.Select( a =>
            {
                var personalLinkSectionViewModel = new PersonalLinkSectionViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    LinkCount = a.PersonalLinks.Where( x => x.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).Count(),
                    IsShared = a.IsShared,
                    PersonalLinkSectionOrder = currentPersonSectionOrderLookupBySectionId?.GetValueOrNull( a.Id )
                };

                if ( limitToSharedSections )
                {
                    // if we are only showing shared sections, let them edit it if authorized edit
                    personalLinkSectionViewModel.CanEdit = a.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
                else
                {
                    // Don't allow shared sections to be deleted/edited if we showing both shared and non-shared sections
                    personalLinkSectionViewModel.CanEdit = !a.IsShared;
                }

                personalLinkSectionViewModel.CanDelete = personalLinkSectionViewModel.CanEdit;

                return personalLinkSectionViewModel;
            } ).ToList();

            return viewModelList.OrderBy( a => a.PersonalLinkSectionOrder?.Order ?? 0 ).ThenBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Handles the OnDataBound event of the DeleteButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteButton_OnDataBound( object sender, RowEventArgs e )
        {
            var personalLink = e.Row.DataItem as PersonalLinkSectionViewModel;

            if ( personalLink == null )
            {
                return;
            }

            var deleteButton = sender as LinkButton;
            if ( deleteButton == null )
            {
                return;
            }

            deleteButton.Visible = personalLink.CanDelete;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class PersonalLinkSectionViewModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsShared { get; set; }

            public int LinkCount { get; set; }

            public PersonalLinkSectionOrder PersonalLinkSectionOrder { get; set; }

            public bool CanDelete { get; set; }

            public bool CanEdit { get; set; }
        }

        #endregion Helper Classes
    }
}