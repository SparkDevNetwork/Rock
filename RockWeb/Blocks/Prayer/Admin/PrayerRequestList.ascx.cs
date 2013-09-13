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
    [AdditionalActions( new string[] { "Approve" } )]
    [LinkedPage( "Detail Page", Order = 0 )]
    [IntegerField( "Group Category Id", "The id of a 'top level' Category.  Only prayer requests under this category will be shown.", false, -1, "Filtering", 1, "GroupCategoryId" )]
    public partial class PrayerRequestList : Rock.Web.UI.RockBlock
    {
        #region Private BlockType Attributes
        /// <summary>
        /// The prayer request key parameter used in the QueryString for detail page.
        /// </summary>
        private static readonly string PrayerRequestKeyParameter = "prayerRequestId";
        /// <summary>
        /// The block instance configured group category id.  This causes only requests for the appropriate root/group-level category to be seen.
        /// </summary>
        protected int _blockInstanceGroupCategoryId = -1;
        /// <summary>
        /// The PrayerRequest entity type id.  This causes only categories that are appropriate to the PrayerRequest entity to be listed.
        /// </summary>
        protected int? _prayerRequestEntityTypeId = null;
        /// <summary>
        /// Holds whether or not the person can add, edit, and delete.
        /// </summary>
        bool canAddEditDelete = false;
        /// <summary>
        /// Holds whether or not the person can approve requests.
        /// </summary>
        bool canApprove = false;
        #endregion

        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string PrayerCategory = "Prayer Category";
            public static readonly string FromDate = "From Date";
            public static readonly string ToDate = "To Date";
            public static readonly string ApprovalStatus = "Approval Status";
            public static readonly string UrgentStatus = "Urgent Status";
            public static readonly string ActiveStatus = "Active Status";
            public static readonly string PublicStatus = "Public/Private";
            public static readonly string CommentingStatus = "Commenting Status";
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

            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            BindFilter();

            gPrayerRequests.DataKeyNames = new string[] { "id" };
            gPrayerRequests.Actions.AddClick += gPrayerRequests_Add;
            gPrayerRequests.GridRebind += gPrayerRequests_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            canApprove = IsUserAuthorized( "Approve" );
            canAddEditDelete = IsUserAuthorized( "Edit" );
            gPrayerRequests.Actions.ShowAdd = canAddEditDelete;
            gPrayerRequests.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
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

        #region Grid Filter
        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            // Set the Approval Status radio options.
            var item = rblApprovedFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.ApprovalStatus ) );
            if ( item != null )
            {
                item.Selected = true;
            }

            // Set the Public Status radio options.
            var itemPublic = rblPublicFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.PublicStatus ) );
            if ( itemPublic != null )
            {
                itemPublic.Selected = true;
            }

            // Set the Commenting Status radio options.
            var itemAllowComments = rblAllowCommentsFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.CommentingStatus ) );
            if ( itemAllowComments != null )
            {
                itemAllowComments.Selected = true;
            }

            // Set the Active Status radio options.
            var itemActiveStatus = rblActiveFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.ActiveStatus ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }

            // Set the Active Status radio options.
            var itemUrgentStatus = rblUrgentFilter.Items.FindByValue( rFilter.GetUserPreference( FilterSetting.UrgentStatus ) );
            if ( itemUrgentStatus != null )
            {
                itemUrgentStatus.Selected = true;
            }

            dtRequestEnteredDateRangeStartDate.Text = rFilter.GetUserPreference( FilterSetting.FromDate );
            dtRequestEnteredDateRangeEndDate.Text = rFilter.GetUserPreference( FilterSetting.ToDate );

            // Set the category picker's selected value
            int selectedPrayerCategoryId = -1;
            if ( int.TryParse( rFilter.GetUserPreference( FilterSetting.PrayerCategory ), out selectedPrayerCategoryId ) )
            {
                Category prayerCategory = new CategoryService().Get( selectedPrayerCategoryId );
                cpPrayerCategoryFilter.SetValue( prayerCategory );
            }
        }

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( FilterSetting.PrayerCategory, cpPrayerCategoryFilter.SelectedValue == Rock.Constants.None.IdValue ? string.Empty :  cpPrayerCategoryFilter.SelectedValue  );
            rFilter.SaveUserPreference( FilterSetting.FromDate, dtRequestEnteredDateRangeStartDate.Text );
            rFilter.SaveUserPreference( FilterSetting.ToDate, dtRequestEnteredDateRangeEndDate.Text );

            // only save settings that are not the default "all" preference...
            if ( rblApprovedFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.ApprovalStatus, "" );
            }
            else
            {
                rFilter.SaveUserPreference( FilterSetting.ApprovalStatus, rblApprovedFilter.SelectedValue );
            }

            if ( rblUrgentFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.UrgentStatus, "" );
            }
            else
            {
                rFilter.SaveUserPreference( FilterSetting.UrgentStatus, rblUrgentFilter.SelectedValue );
            }

            if ( rblPublicFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.PublicStatus, "" );
            }
            else
            {
                rFilter.SaveUserPreference( FilterSetting.PublicStatus, rblPublicFilter.SelectedValue );
            }

            if ( rblActiveFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.ActiveStatus, "" );
            }
            else
            {
                rFilter.SaveUserPreference( FilterSetting.ActiveStatus, rblActiveFilter.SelectedValue );
            }

            if ( rblAllowCommentsFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( FilterSetting.CommentingStatus, "" );
            }
            else
            {
                rFilter.SaveUserPreference( FilterSetting.CommentingStatus, rblAllowCommentsFilter.SelectedValue );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Prayer Category":

                    int categoryId = All.Id;
                    if ( int.TryParse( e.Value, out categoryId ) )
                    {
                        if ( categoryId == All.Id )
                        {
                            e.Value = "[All]";
                        }
                        else
                        {
                            var service = new CategoryService();
                            var category = service.Get( categoryId );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
        
        #region Prayer Request Grid

        /// <summary>
        /// Binds the grid to a list of Prayer Requests.
        /// </summary>
        private void BindGrid()
        {
            PrayerRequestService prayerRequestService = new PrayerRequestService();
            SortProperty sortProperty = gPrayerRequests.SortProperty;

            var prayerRequests = prayerRequestService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    FullName = a.FirstName + " " + a.LastName,
                    CategoryName = a.Category.Name,
                    a.EnteredDate,
                    a.Text,
                    a.FlagCount,
                    a.IsApproved,
                    a.CategoryId,
                    CategoryParentCategoryId = a.Category.ParentCategoryId,
                    a.IsUrgent,
                    a.IsPublic,
                    a.IsActive,
                    a.AllowComments
                } );

            // Filter by prayer category if one is selected...
            int selectedPrayerCategoryID = All.Id;
            int.TryParse( cpPrayerCategoryFilter.SelectedValue, out selectedPrayerCategoryID );
            if ( selectedPrayerCategoryID != All.Id && selectedPrayerCategoryID != None.Id )
            {
                prayerRequests = prayerRequests.Where( c => c.CategoryId == selectedPrayerCategoryID
                    || c.CategoryParentCategoryId == selectedPrayerCategoryID);
            }

            // Filter by approved/unapproved
            if ( rblApprovedFilter.SelectedIndex > -1 )
            {
                if ( rblApprovedFilter.SelectedValue == "unapproved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == false || !a.IsApproved.HasValue );
                }
                else if ( rblApprovedFilter.SelectedValue == "approved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == true );
                }
            }

            // Filter by urgent/non-urgent
            if ( rblUrgentFilter.SelectedIndex > -1 )
            {
                if ( rblUrgentFilter.SelectedValue == "non-urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == false || !a.IsUrgent.HasValue );
                }
                else if ( rblUrgentFilter.SelectedValue == "urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == true );
                }
            }

            // Filter by public/non-public
            if ( rblPublicFilter.SelectedIndex > -1 )
            {
                if ( rblPublicFilter.SelectedValue == "non-public" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsPublic == false || !a.IsPublic.HasValue );
                }
                else if ( rblPublicFilter.SelectedValue == "public" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsPublic == true );
                }
            }

            // Filter by active/inactive
            if ( rblActiveFilter.SelectedIndex > -1 )
            {
                if ( rblActiveFilter.SelectedValue == "inactive" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == false || !a.IsActive.HasValue );
                }
                else if ( rblActiveFilter.SelectedValue == "active" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == true );
                }
            }

            // Filter by active/inactive
            if ( rblAllowCommentsFilter.SelectedIndex > -1 )
            {
                if ( rblAllowCommentsFilter.SelectedValue == "unallow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == false || !a.AllowComments.HasValue );
                }
                else if ( rblAllowCommentsFilter.SelectedValue == "allow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == true );
                }
            }

            // Filter by EnteredDate
            if ( dtRequestEnteredDateRangeStartDate.SelectedDate != null )
            {
                prayerRequests = prayerRequests.Where( a => a.EnteredDate >= dtRequestEnteredDateRangeStartDate.SelectedDate );
            }
            if ( dtRequestEnteredDateRangeEndDate.SelectedDate != null )
            {
                prayerRequests = prayerRequests.Where( a => a.EnteredDate <= dtRequestEnteredDateRangeEndDate.SelectedDate );
            }

            // Sort by the given property otherwise sort by the EnteredDate
            if ( sortProperty != null )
            {
                gPrayerRequests.DataSource = prayerRequests.Sort( sortProperty ).ToList();
            }
            else
            {
                // TODO Figure out how to tell Grid what Direction and Property it's sorting on
                //sortProperty.Direction = SortDirection.Ascending;
                //sortProperty.Property = "EnteredDate";
                gPrayerRequests.DataSource = prayerRequests.OrderBy( p => p.EnteredDate ).ToList();
            }

            gPrayerRequests.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", PrayerRequestKeyParameter, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", PrayerRequestKeyParameter, (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the CheckChanged event of the gPrayerRequests IsApproved field.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_CheckChanged( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                PrayerRequestService prayerRequestService = new PrayerRequestService();
                PrayerRequest prayerRequest = prayerRequestService.Get( (int)e.RowKeyValue );

                if ( prayerRequest != null )
                {
                    failure = false;
                    // if it was approved, set it to unapproved... otherwise
                    if ( prayerRequest.IsApproved ?? false )
                    {
                        prayerRequest.IsApproved = false;
                    }
                    else
                    {
                        prayerRequest.IsApproved = true;
                        prayerRequest.ApprovedByPersonId = CurrentPerson.Id;
                        prayerRequest.ApprovedOnDate = DateTime.Now;
                        // reset the flag count only to zero ONLY if it had a value previously.
                        if ( prayerRequest.FlagCount.HasValue && prayerRequest.FlagCount > 0 )
                        {
                            prayerRequest.FlagCount = 0;
                        }
                    }

                    prayerRequestService.Save( prayerRequest, CurrentPersonId );
                }

                BindGrid();
            }

            if ( failure )
            {
                mdGridWarning.Show( "Unable to approve that prayer request", ModalAlertType.Warning );
            }

        }

        /// <summary>
        /// Handles the Delete event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                PrayerRequestService prayerRequestService = new PrayerRequestService();
                PrayerRequest prayerRequest = prayerRequestService.Get( (int)e.RowKeyValue );

                if ( prayerRequest != null )
                {
                    string errorMessage;
                    if ( !prayerRequestService.CanDelete( prayerRequest, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    prayerRequestService.Delete( prayerRequest, CurrentPersonId );
                    prayerRequestService.Save( prayerRequest, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPrayerRequests_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }
        
        /// <summary>
        /// Handles disabling the Toggle fields if the user does not have Approval rights.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_RowDataBound( object sender, GridViewRowEventArgs e )
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
    }
}