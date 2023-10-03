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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Comment List" )]
    [Category( "Prayer" )]
    [Description( "Displays a list of prayer comments for the configured top-level group category." )]

    [LinkedPage( "Detail Page", Order = 0 ),]
    [CategoryField( "Category Selection", "A top level category. Only prayer requests comments under this category will be shown.", false, "Rock.Model.PrayerRequest", "", "", false, "", "Category Selection", 1, "PrayerRequestCategory" )]
    [Rock.SystemGuid.BlockTypeGuid( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22" )]
    public partial class PrayerCommentsList : RockBlock, ICustomGridColumns
    {
        #region Fields

        /// <summary>
        /// Holds whether or not the person can approve.
        /// </summary>
        private bool _canApprove = false;

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();

            // Block Security and special attributes (RockPage takes care of View)
            var canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            _canApprove = IsUserAuthorized( "Approve" );

            // grid stuff...
            gPrayerComments.Actions.ShowAdd = false;
            gPrayerComments.IsDeleteEnabled = canAddEditDelete;

            gPrayerComments.DataKeyNames = new string[] { "id", "entityid" };
            gPrayerComments.GridRebind += gPrayerComments_GridRebind;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindCommentsGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Edit( object sender, RowEventArgs e )
        {
            // NOTE: DataKeys for Grid has two fields "id,entityId"
            NavigateToLinkedPage( "DetailPage", "noteId", (int)e.RowKeyValues["id"], "PrayerRequestId", (int)e.RowKeyValues["entityid"] );
        }

        /// <summary>
        /// Handles the CheckChanged event of the grid's IsApproved field.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_CheckChanged( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValues != null )
            {
                var rockContext = new RockContext();
                NoteService noteService = new NoteService( rockContext );

                // NOTE: DataKeys for Grid has two fields "id,entityId"
                Note prayerComment = noteService.Get( (int)e.RowKeyValues["id"] );

                if ( prayerComment != null )
                {
                    failure = false;
                    rockContext.SaveChanges();
                }

                BindCommentsGrid();
            }

            if ( failure )
            {
                maGridWarning.Show( "Unable to approve that prayer comment", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            NoteService noteService = new NoteService( rockContext );
            Note note = noteService.Get( (int)e.RowKeyValues["id"] );

            if ( note != null )
            {
                string errorMessage;
                if ( !noteService.CanDelete( note, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                noteService.Delete( note );
                rockContext.SaveChanges();
            }

            BindCommentsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPrayerComments_GridRebind( object sender, EventArgs e )
        {
            BindCommentsGrid();
        }

        /// <summary>
        /// Handles disabling the Toggle fields if the user does not have Approval rights.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( _canApprove )
            {
                return;
            }

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                foreach ( TableCell cell in e.Row.Cells )
                {
                    foreach ( Control c in cell.Controls )
                    {
                        Toggle toggle = c as Toggle;
                        if ( toggle != null )
                        {
                            toggle.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( FilterSetting.DateRange, drpDateRange.DelimitedValues );
            gfFilter.SetFilterPreference( FilterSetting.PrayerCategory, catpPrayerCategoryFilter.SelectedValue == Rock.Constants.None.IdValue ? string.Empty : catpPrayerCategoryFilter.SelectedValue );
            BindCommentsGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                // don't display dead setting
                case "From Date":
                    e.Value = string.Empty;
                    break;

                // don't display dead setting
                case "To Date":
                    e.Value = string.Empty;
                    break;

                case "Prayer Category":

                    int categoryId = e.Value.AsIntegerOrNull() ?? All.Id;
                    if ( categoryId == All.Id )
                    {
                        e.Value = "All";
                    }
                    else
                    {
                        var category = CategoryCache.Get( categoryId );
                        if ( category != null )
                        {
                            e.Value = category.Name;
                        }
                    }

                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the comments grid.
        /// </summary>
        private void BindCommentsGrid()
        {
            var rockContext = new RockContext();

            var noteTypeService = new NoteTypeService( rockContext );
            var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            var noteService = new NoteService( rockContext );
            var prayerComments = noteService.GetByNoteTypeId( noteType.Id );

            SortProperty sortProperty = gPrayerComments.SortProperty;

            // Filter by Category.  First see if there is a Block Setting, otherwise use the Grid Filter
            CategoryCache categoryFilter = null;
            var blockCategoryGuid = GetAttributeValue( "PrayerRequestCategory" ).AsGuidOrNull();
            if ( blockCategoryGuid.HasValue )
            {
                categoryFilter = CategoryCache.Get( blockCategoryGuid.Value );
            }

            if ( categoryFilter == null && catpPrayerCategoryFilter.Visible )
            {
                int? filterCategoryId = catpPrayerCategoryFilter.SelectedValue.AsIntegerOrNull();
                if ( filterCategoryId.HasValue )
                {
                    categoryFilter = CategoryCache.Get( filterCategoryId.Value );
                }
            }

            if ( categoryFilter != null )
            {
                // if filtered by category, only show comments for prayer requests in that category or any of its decendent categories
                var categoryService = new CategoryService( rockContext );
                var categories = new CategoryService( rockContext ).GetAllDescendents( categoryFilter.Guid ).Select( a => a.Id ).ToList();

                var prayerRequestQry = new PrayerRequestService( rockContext ).Queryable().Where( a => a.CategoryId.HasValue &&
                    ( a.Category.Guid == categoryFilter.Guid || categories.Contains( a.CategoryId.Value ) ) )
                    .Select( a => a.Id );

                prayerComments = prayerComments.Where( a => a.EntityId.HasValue && prayerRequestQry.Contains( a.EntityId.Value ) );
            }

            // Filter by Date Range
            if ( drpDateRange.LowerValue.HasValue )
            {
                DateTime startDate = drpDateRange.LowerValue.Value.Date;
                prayerComments = prayerComments.Where( a => a.CreatedDateTime.HasValue && a.CreatedDateTime.Value >= startDate );
            }

            if ( drpDateRange.UpperValue.HasValue )
            {
                // Add one day in order to include everything up to the end of the selected datetime.
                var endDate = drpDateRange.UpperValue.Value.AddDays( 1 );
                prayerComments = prayerComments.Where( a => a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < endDate );
            }

            // Sort by the given property otherwise sort by the EnteredDate
            if ( sortProperty != null )
            {
                gPrayerComments.DataSource = prayerComments.Sort( sortProperty ).ToList();
            }
            else
            {
                gPrayerComments.DataSource = prayerComments.OrderByDescending( n => n.CreatedDateTime ).ToList();
            }

            gPrayerComments.DataBind();
        }

        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            drpDateRange.DelimitedValues = gfFilter.GetFilterPreference( FilterSetting.DateRange );

            // Set the category picker's selected value
            int selectedPrayerCategoryId = gfFilter.GetFilterPreference( FilterSetting.PrayerCategory ).AsInteger();
            Category prayerCategory = new CategoryService( new RockContext() ).Get( selectedPrayerCategoryId );
            catpPrayerCategoryFilter.SetValue( prayerCategory );

            // only show the Filter if there isn't Category set in the Block Setting
            CategoryCache blockCategory = null;
            var blockCategoryGuid = GetAttributeValue( "PrayerRequestCategory" ).AsGuidOrNull();
            if ( blockCategoryGuid.HasValue )
            {
                blockCategory = CategoryCache.Get( blockCategoryGuid.Value );
            }

            catpPrayerCategoryFilter.Visible = blockCategory == null;
        }

        #endregion

        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string DateRange = "Date Range";
            public static readonly string PrayerCategory = "Prayer Category";
        }
    }
}