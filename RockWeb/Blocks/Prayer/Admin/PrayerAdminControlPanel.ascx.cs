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
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    // This attribute field belongs elsewhere... like in the Prayer Session blocktype
    [IntegerField( 0, "Catgegory Id", "", null, "Filtering", "The id of the parent Category. Only prayer requests under this category will be shown." )]
    [IntegerField( 1, "Min Flagged Count", "1", null, "Flagged Requests", "Number of times a request has to be 'flagged' before it is considered flagged for approval/re-approval." )]
    public partial class PrayerAdminControlPanel : Rock.Web.UI.RockBlock
    {
        #region Private BlockType Attributes
        //int _minFlaggedCount = 1;
        //_minFlaggedCount = int.Parse( GetAttributeValue( "MinFlaggedCount" ) );
        
        protected int? _prayerRequestEntityTypeId = null;
        protected NoteType _noteType;
        #endregion

        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            gPrayerRequests.DataKeyNames = new string[] { "id" };
            gPrayerRequests.Actions.AddClick += gPrayerRequests_Add;
            gPrayerRequests.GridRebind += gPrayerRequests_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gPrayerRequests.Actions.IsAddEnabled = canAddEditDelete;
            gPrayerRequests.IsDeleteEnabled = canAddEditDelete;

            // Prayer Comment stuff...
            gPrayerComments.Actions.IsAddEnabled = false;
            gPrayerComments.IsDeleteEnabled = canAddEditDelete;

            gPrayerComments.DataKeyNames = new string[] { "id" };
            gPrayerComments.GridRebind += gPrayerComments_GridRebind;

            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );
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
                BindCommentsGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Prayer Request Grid Events

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            // Here we'll only save the Category preference since the other filters are
            // typically more transient in nature.
            //rFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "prayerRequestId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Edit( object sender, RowEventArgs e )
        {
            //NavigateToDetailPage( "prayerRequestId", (int)e.RowKeyValue );
            ShowEdit( (int)e.RowKeyValue );

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

        #endregion

        #region Prayer Comment Grid Events

        /// <summary>
        /// Handles the Add event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "prayerRequestId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerComments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerComments_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "prayerRequestId", (int)e.RowKeyValue );
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
                Note note = noteService.Get( (int)e.RowKeyValue );

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

        #endregion

        #region Edit Details

        private void ShowEdit( int prayerRequestId )
        {
            pnlLists.Visible = false;
            pnlDetails.Visible = true;

            hfPrayerRequestId.Value = prayerRequestId.ToString();

            lActionTitle.Text = ( prayerRequestId > 0 ) ?
                    ActionTitle.Edit( PrayerRequest.FriendlyTypeName ) : ActionTitle.Add( PrayerRequest.FriendlyTypeName );

            PrayerRequest prayerRequest = new PrayerRequestService().Get( prayerRequestId );

            ddlCategory.Items.Clear();
            ddlCategory.Items.Add( new ListItem( All.Text, All.Id.ToString() ) );
            CategoryService categoryService = new CategoryService();
            foreach ( Category category in categoryService.Queryable().OrderBy( a => a.Name ) )
            {
                ListItem li = new ListItem( category.Name, category.Id.ToString() );
                li.Selected = ( prayerRequest != null && category.Id == prayerRequest.CategoryId );
                ddlCategory.Items.Add( li );
            }

            if ( prayerRequest != null )
            {
                btnApproveRequest.Visible = btnDenyRequest.Visible = IsUserAuthorized( "Approve" );

                tbFirstName.Text = prayerRequest.FirstName;
                tbLastName.Text = prayerRequest.LastName;
                tbText.Text = prayerRequest.Text;
                hfPrayerRequestFlagCount.Value = prayerRequest.FlagCount.ToString();

                SetApprovalValues( prayerRequest.IsApproved, this.CurrentPerson );
            }
        }

        protected void btnApproveRequest_Click( object sender, EventArgs e )
        {
            hfPrayerRequestApprovalStatus.Value = "True";
            SetApprovalValues( true, this.CurrentPerson );
        }

        protected void btnDenyRequest_Click( object sender, EventArgs e )
        {
            hfPrayerRequestApprovalStatus.Value = "False";
            SetApprovalValues( false, this.CurrentPerson );
        }

        /// <summary>
        /// Sets the approval values.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="person">The person.</param>
        private void SetApprovalValues( bool? isApproved, Person person )
        {
            hfPrayerRequestApprovalStatus.Value = isApproved.ToString();
            int flagCount = 0;
            int.TryParse( hfPrayerRequestFlagCount.Value, out flagCount );

            switch ( isApproved )
            {
                case true:
                    ltPrayerRequestApprovalStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-success";
                    ltPrayerRequestApprovalStatus.Text = "Approved";
                    flagCount = 0;
                    hfPrayerRequestFlagCount.SetValue( flagCount );
                    btnApproveRequest.Visible = false;
                    break;
                case false:
                    ltPrayerRequestApprovalStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-error";
                    ltPrayerRequestApprovalStatus.Text = "Rejected";
                    btnDenyRequest.Visible = false;
                    break;
                default:
                    ltPrayerRequestApprovalStatus.TextCssClass = "alert alert-info";
                    ltPrayerRequestApprovalStatus.Text = "Pending Approval";
                    btnApproveRequest.Visible = btnDenyRequest.Visible = true;
                    break;
            }

            lFlaggedMessage.Text = ( flagCount == 0 ) ? "" : string.Format( "<div class='alert'><i class='icon-flag'></i> flagged {0} times</div>", flagCount );

            if ( person != null && isApproved.HasValue )
            {
                lblApprovedByPerson.Visible = true;
                lblApprovedByPerson.Text = string.Format( "by {0}", person.FullName );
                hfApprovedByPersonId.Value = person.Id.ToString();
            }
            else
            {
                lblApprovedByPerson.Visible = false;
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRequest();
            BindGrid();
            BindCommentsGrid();
            pnlLists.Visible = true;
            pnlDetails.Visible = false;
        }

        private void SaveRequest()
        {
            PrayerRequest prayerRequest;
            PrayerRequestService prayerRequestService = new PrayerRequestService();

            int prayerRequestId = int.Parse( hfPrayerRequestId.Value );

            if ( prayerRequestId == 0 )
            {
                prayerRequest = new PrayerRequest();
                prayerRequestService.Add( prayerRequest, CurrentPersonId );
                prayerRequest.IsApproved = true;
            }
            else
            {
                prayerRequest = prayerRequestService.Get( prayerRequestId );
                if ( !string.IsNullOrEmpty( hfPrayerRequestApprovalStatus.Value ) )
                {
                    prayerRequest.IsApproved = bool.Parse( hfPrayerRequestApprovalStatus.Value );
                }
                if ( !string.IsNullOrEmpty( hfPrayerRequestFlagCount.Value ) )
                {
                    prayerRequest.FlagCount = hfPrayerRequestFlagCount.ValueAsInt();
                }
            }

            if ( string.IsNullOrWhiteSpace( ddlCategory.SelectedValue ) )
            {
                ddlCategory.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlCategory.LabelText ) );
                return;
            }

            prayerRequest.FirstName = tbFirstName.Text;
            prayerRequest.LastName = tbLastName.Text;
            prayerRequest.Text = tbText.Text;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !prayerRequest.IsValid )
            {
                // field controls render error messages
                return;
            }

            prayerRequestService.Save( prayerRequest, CurrentPersonId );

        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlLists.Visible = true;
            pnlDetails.Visible = false;
        }
        #endregion
        #region Internal Methods

        /// <summary>
        /// Binds any needed data to the Grid Filter
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( new ListItem( All.Text, All.Id.ToString() ) );

            CategoryService categoryService = new CategoryService();
            foreach ( Category category in categoryService.Queryable().OrderBy( a => a.Name ) )
            {
                ListItem li = new ListItem( category.Name, category.Id.ToString() );
                li.Selected = category.Id.ToString() == rFilter.GetUserPreference( "Category" );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            PrayerRequestService prayerRequestService = new PrayerRequestService();
            SortProperty sortProperty = gPrayerRequests.SortProperty;

            var prayerRequests = prayerRequestService.Queryable();

            // Filter by category...
            if ( ddlCategoryFilter.SelectedValue != All.Id.ToString() )
            {
                int selectedCategoryID = int.Parse( ddlCategoryFilter.SelectedValue );
                prayerRequests = prayerRequests.Where( a => a.Category != null && a.Category.Id == selectedCategoryID );
            }

            // Filter by approved/unapproved
            if ( !cbShowApproved.Checked )
            {
                prayerRequests = prayerRequests.Where( a => a.IsApproved == false || !a.IsApproved.HasValue );
            }

            // Filter by EnteredDate
            if ( dtRequestEnteredDateRangeStartDate.SelectedDate != null )
            {
                prayerRequests = prayerRequests.Where( a => a.EnteredDate >= dtRequestEnteredDateRangeStartDate.SelectedDate );
            }
            if ( dtRequestEnteredDateRangeStartDate.SelectedDate != null )
            {
                prayerRequests = prayerRequests.Where( a => a.EnteredDate <= dtRequestEnteredDateRangeEndDate.SelectedDate );
            }

            if ( prayerRequests.Count() > 0 )
            {
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
            }
            gPrayerRequests.DataBind();
        }

        private void BindCommentsGrid()
        {
            var noteTypeService = new NoteTypeService();
            var noteType = noteTypeService.Get( (int)_prayerRequestEntityTypeId, "Prayer Comment" );
            // TODO log exception if noteType is null

            var noteService = new NoteService();
            var prayerComments = noteService.GetByNoteTypeId( noteType.Id );

            SortProperty sortProperty = gPrayerComments.SortProperty;

            // Sort by the given property otherwise sort by the EnteredDate
            if ( sortProperty != null )
            {
                gPrayerComments.DataSource = prayerComments.Sort( sortProperty ).ToList();
            }
            else
            {
                gPrayerComments.DataSource = prayerComments.OrderBy( n => n.Date ).ToList();
            }

            gPrayerComments.DataBind();
        }
        #endregion

        protected void gPrayerRequests_CheckChanged( object sender, RowEventArgs e )
        {
            mdGridWarning.Show( string.Format( "Boo: {0}", e.RowKeyValue ), ModalAlertType.Warning );
        }
    }
}