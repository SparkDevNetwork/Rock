//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [LinkedPage( "Detail Page", Order = 0 ), ]
    [IntegerField( "Group Category Id", "The id of a 'top level' Category.  Only prayer requests comments under this category will be shown.", false, -1, "Filtering", 1, "GroupCategoryId" )]
    public partial class PrayerCommentsList : Rock.Web.UI.RockBlock
    {
        #region Private BlockType Attributes
        /// <summary>
        /// The prayer comment key parameter seen in the QueryString
        /// </summary>
        private static readonly string PrayerCommentKeyParameter = "noteId";
        /// <summary>
        /// The prayer request key parameter seen in the QueryString
        /// </summary>
        private static readonly string PrayerrequestKeyParameter = "prayerRequestId";
        /// <summary>
        /// The block instance configured group category id.  This causes only comments for the appropriate root/group-level category to be seen.
        /// </summary>
        protected int _blockInstanceGroupCategoryId = -1;
        /// <summary>
        /// The PrayerRequest entity type id.  This causes only comments/categories that are appropriate to the PrayerRequest entity to be listed.
        /// </summary>
        protected int? _prayerRequestEntityTypeId = null;
        /// <summary>
        /// The NoteType which corresponds to "Prayer Comments".
        /// </summary>
        protected NoteType _noteType;
        /// <summary>
        /// Holds whether or not the person can add, edit, and delete.
        /// </summary>
        bool canAddEditDelete = false;
        /// <summary>
        /// Holds whether or not the person can approve.
        /// </summary>
        bool canApprove = false;
        #endregion

        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string FromDate = "From Date";
            public static readonly string ToDate = "To Date";
            public static readonly string ApprovalStatus = "Approval Status";
        }
        #endregion

        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Int32.TryParse( GetAttributeValue( "GroupCategoryId" ), out _blockInstanceGroupCategoryId );
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            BindFilter();

            // Block Security and special attributes (RockPage takes care of "View")
            canAddEditDelete = IsUserAuthorized( "Edit" );
            canApprove = IsUserAuthorized( "Approve" );

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

        #region Prayer Comment Grid Events

        /// <summary>
        /// Binds the comments grid.
        /// </summary>
        private void BindCommentsGrid()
        {
            var noteTypeService = new NoteTypeService();
            var noteType = noteTypeService.Get( (int)_prayerRequestEntityTypeId, "Prayer Comment" );
            // TODO log exception if noteType is null

            var noteService = new NoteService();
            var prayerComments = noteService.GetByNoteTypeId( noteType.Id );

            SortProperty sortProperty = gPrayerComments.SortProperty;

            // TODO: filter out comments that do not belong to the configured "category" grouping.
            //if ( _blockInstanceGroupCategoryId != All.Id )
            //{
            //    prayerComments = prayerComments.Where( c => c.CategoryId == _blockInstanceGroupCategoryId );
            //}

            // TODO: Filter by approved/unapproved
            //if ( rblApprovedFilter.SelectedValue == "unapproved" )
            //{
            //    prayerComments = prayerComments.Where( a => a.IsApproved == false || !a.IsApproved.HasValue );
            //}
            //else if ( rblApprovedFilter.SelectedValue == "approved" )
            //{
            //    prayerComments = prayerComments.Where( a => a.IsApproved == true );
            //}

            // Filter by EnteredDate
            if ( dtDateRangeStartDate.SelectedDate != null )
            {
                prayerComments = prayerComments.Where( a => a.CreationDateTime >= dtDateRangeStartDate.SelectedDate );
            }
            if ( dtDateRangeEndDate.SelectedDate != null )
            {
                prayerComments = prayerComments.Where( a => a.CreationDateTime <= dtDateRangeEndDate.SelectedDate );
            }

            // Sort by the given property otherwise sort by the EnteredDate
            if ( sortProperty != null )
            {
                gPrayerComments.DataSource = prayerComments.Sort( sortProperty ).ToList();
            }
            else
            {
                gPrayerComments.DataSource = prayerComments.OrderBy( n => n.CreationDateTime ).ToList();
            }

            gPrayerComments.DataBind();
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", PrayerCommentKeyParameter, (int)e.RowKeyValues["id"], PrayerrequestKeyParameter, (int)e.RowKeyValues["entityid"] );
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
                NoteService noteService = new NoteService();
                Note prayerComment = noteService.Get( (int)e.RowKeyValues["id"] );

                if ( prayerComment != null )
                {
                    failure = false;
                    // if it was approved, set it to unapproved... otherwise
                    //if ( prayerComment.IsApproved ?? false )
                    //{
                    //    prayerComment.IsApproved = false;
                    //}
                    //else
                    //{
                    //    prayerComment.IsApproved = true;
                    //}

                    noteService.Save( prayerComment, CurrentPersonId );
                }

                BindCommentsGrid();
            }

            if ( failure )
            {
                mdGridWarning.Show( "Unable to approve that prayer comment", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                NoteService noteService = new NoteService();
                Note note = noteService.Get( (int)e.RowKeyValues["id"] );

                if ( note != null )
                {
                    string errorMessage;
                    if ( !noteService.CanDelete( note, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    noteService.Delete( note, CurrentPersonId );
                    noteService.Save( note, CurrentPersonId );
                }
            } );

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
            if ( canApprove )
                return;

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
        #endregion
        
        #region Grid Filter
        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            dtDateRangeStartDate.Text = rFilter.GetUserPreference( FilterSetting.FromDate );
            dtDateRangeEndDate.Text = rFilter.GetUserPreference( FilterSetting.ToDate );

            // Set the Approval Status radio options.
            var item = rblApprovedFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.ApprovalStatus ) );
            if ( item != null )
            {
                item.Selected = true;
            }
        }

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( FilterSetting.FromDate, dtDateRangeStartDate.Text );
            rFilter.SaveUserPreference( FilterSetting.ToDate, dtDateRangeEndDate.Text );

            // only save settings that are not the default "all" preference...
            if ( rblApprovedFilter.SelectedValue != "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.ApprovalStatus, rblApprovedFilter.SelectedValue );
            }

            BindCommentsGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            // not necessary yet.
        }
        #endregion

    }
}