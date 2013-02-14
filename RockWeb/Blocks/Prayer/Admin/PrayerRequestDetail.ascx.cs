//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web;
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
    [IntegerField( 0, "Group Category Id", "-1", null, "Filtering", "The id of the 'prayer group category'. Only prayer requests under this category will be shown." )]
    public partial class PrayerRequestDetail : RockBlock, IDetailBlock
    {
        #region Private BlockType Attributes
        private static readonly string PrayerRequestKeyParameter = "prayerRequestId";
        int _blockInstanceGroupCategoryId = -1;
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

            Int32.TryParse( GetAttributeValue( "GroupCategoryId" ), out _blockInstanceGroupCategoryId );
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
                string itemId = PageParameter( PrayerRequestKeyParameter );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( PrayerRequestKeyParameter, int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the edit Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            PrayerRequestService service = new PrayerRequestService();
            PrayerRequest item = service.Get( hfPrayerRequestId.ValueAsInt() );
            ShowEditDetails( item );
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( PrayerRequestKeyParameter ) )
            {
                return;
            }

            PrayerRequest prayerRequest;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                prayerRequest = new PrayerRequestService().Get( itemKeyValue );
            }
            else
            {
                prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = true, AllowComments = true };
            }

            hfPrayerRequestId.Value = prayerRequest.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PrayerRequest.FriendlyTypeName );
            }

            if ( readOnly )
            {
                //btnEdit.Visible = false;
                ShowReadonlyDetails( prayerRequest );
            }
            else
            {
                if ( prayerRequest.Id > 0 )
                {
                    ShowReadonlyDetails( prayerRequest );
                }
                else
                {
                    ShowEditDetails( prayerRequest );
                }
            }
        }
        #endregion

        #region View & Edit Details

        private void ShowReadonlyDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( false );

            litFullName.Text = prayerRequest.FullName;
            litCategory.Text = prayerRequest.Category.Name;
            litRequest.Text = HttpUtility.HtmlEncode( prayerRequest.Text );

            ShowStatus( prayerRequest, this.CurrentPerson, litFlaggedMessageRO );
            ShowPrayerCount( prayerRequest, litPrayerCountRO );

            litStatus.Text = ( ! prayerRequest.IsApproved ?? false ) ? "<dt><span class='label label-important'>unapproved</span></dt>" : "";
            litUrgent.Text = ( prayerRequest.IsUrgent ?? false ) ? "<dt><span class='label label-info'><i class='icon-exclamation-sign'></i> urgent</span></dt>" : "";

        }

        private void ShowEditDetails( PrayerRequest prayerRequest )
        {
            SetEditMode(true);

            lActionTitle.Text = ( prayerRequest.Id > 0 ) ? 
                ActionTitle.Edit( PrayerRequest.FriendlyTypeName ) : ActionTitle.Add( PrayerRequest.FriendlyTypeName );

            pnlDetails.Visible = true;

            ddlCategory.Items.Clear();
            CategoryService categoryService = new CategoryService();
            foreach ( Category category in categoryService.Queryable().OrderBy( a => a.Name ) )
            {
                ListItem li = new ListItem( category.Name, category.Id.ToString() );
                li.Selected = ( prayerRequest != null && category.Id == prayerRequest.CategoryId );
                ddlCategory.Items.Add( li );
            }

            tbFirstName.Text = prayerRequest.FirstName;
            tbLastName.Text = prayerRequest.LastName;
            tbText.Text = prayerRequest.Text;
            tbAnswer.Text = prayerRequest.Answer;

            ShowStatus( prayerRequest, this.CurrentPerson, lFlaggedMessage );

            cbIsPublic.Checked = prayerRequest.IsPublic ?? false;
            cbIsUrgent.Checked = prayerRequest.IsUrgent ?? false;
            cbIsActive.Checked = prayerRequest.IsActive ?? false;
            cbAllowComments.Checked = prayerRequest.AllowComments ?? false;
        }

        private void SetEditMode(bool enableEdit)
        {
            fieldsetViewDetails.Visible = !enableEdit;
            fieldsetEditDetails.Visible = enableEdit;
        }

        private void ShowPrayerCount( PrayerRequest prayerRequest, Literal lPrayerCount )
        {
            string cssClass = "badge";

            if ( prayerRequest.PrayerCount > 10 )
            {
                cssClass += " badge-success";
            }
            else if ( prayerRequest.PrayerCount < 1 )
            {
                lPrayerCount.Visible = false;
            }

            lPrayerCount.Text = string.Format( "<span class='{0}' title='current prayer count'>{1}</span>", cssClass, prayerRequest.PrayerCount ?? 0 );
        }

        private void ShowStatus( PrayerRequest prayerRequest, Person person, Literal lFlagged )
        {
            int flagCount = prayerRequest.FlagCount ?? 0;
            lFlagged.Text = ( flagCount == 0 ) ? "" : string.Format( "<dt><span class='label label-warning'><i class='icon-flag'></i> flagged {0} times</span></dt>", flagCount );

            cbApproved.Enabled = IsUserAuthorized( "Approve" );
            cbApproved.Checked = prayerRequest.IsApproved ?? false;

            if ( person != null && ( prayerRequest.IsApproved ?? false ) && prayerRequest.ApprovedByPersonId.HasValue )
            {
                lblApprovedByPerson.Visible = true;
                lblApprovedByPerson.Text = string.Format( "approved by {0}", prayerRequest.ApprovedByPerson.FullName );
            }
            else
            {
                lblApprovedByPerson.Visible = false;
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRequest();
            pnlDetails.Visible = false;
        }

        private void SaveRequest()
        {
            PrayerRequest prayerRequest;
            PrayerRequestService prayerRequestService = new PrayerRequestService();

            int prayerRequestId = int.Parse( hfPrayerRequestId.Value );

            // Fetch the prayer request or create a new one if needed
            if ( prayerRequestId == 0 )
            {
                prayerRequest = new PrayerRequest();
                prayerRequestService.Add( prayerRequest, CurrentPersonId );
                prayerRequest.EnteredDate = DateTime.UtcNow;
            }
            else
            {
                prayerRequest = prayerRequestService.Get( prayerRequestId );
            }

            // If chaning from NOT approved to approved, record who and when
            if ( !(prayerRequest.IsApproved ?? false) && cbApproved.Checked )
            {
                prayerRequest.ApprovedByPersonId = CurrentPerson.Id;
                prayerRequest.ApprovedOnDate = DateTime.UtcNow;
                // reset the flag count only to zero ONLY if it had a value previously.
                if ( prayerRequest.FlagCount.HasValue && prayerRequest.FlagCount > 0 )
                {
                    prayerRequest.FlagCount = 0;
                }
            }
            // Now record all the bits...
            prayerRequest.IsApproved = cbApproved.Checked;
            prayerRequest.IsActive = cbIsActive.Checked;
            prayerRequest.IsUrgent = cbIsUrgent.Checked;
            prayerRequest.AllowComments = cbAllowComments.Checked;
            prayerRequest.IsPublic = cbIsPublic.Checked;

            if ( string.IsNullOrWhiteSpace( ddlCategory.SelectedValue ) )
            {
                ddlCategory.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlCategory.LabelText ) );
                return;
            }
            else
            {
                prayerRequest.CategoryId = int.Parse( ddlCategory.SelectedValue );
            }

            prayerRequest.FirstName = tbFirstName.Text;
            prayerRequest.LastName = tbLastName.Text;
            prayerRequest.Text = tbText.Text;
            prayerRequest.Answer = tbAnswer.Text;

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
            NavigateToParentPage();       
        }
        #endregion

    }
}