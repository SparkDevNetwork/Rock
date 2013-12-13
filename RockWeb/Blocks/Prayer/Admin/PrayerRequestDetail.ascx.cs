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
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [IntegerField( "Expires After (Days)", "Number of days until the request will expire (only applies when auto-approved is enabled).", false, 14, "", 0, "ExpireDays" )]
    [CategoryField( "Default Category", "If a category is not selected, choose a default category to use for all new prayer requests.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "", 1, "DefaultCategory" )]

    public partial class PrayerRequestDetail : RockBlock, IDetailBlock
    {
        #region Private BlockType Attributes
        private static readonly string PrayerRequestKeyParameter = "prayerRequestId";
        #endregion

        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
        /// Handles the edit Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            PrayerRequestService service = new PrayerRequestService();
            PrayerRequest item = service.Get( hfPrayerRequestId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Shows the prayer request's detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
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
                nbEditModeMessage.Title = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PrayerRequest.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lbEdit.Visible = false;
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

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowReadonlyDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( false );
            lActionTitle.Text = string.Format( "{0} Prayer Request", prayerRequest.FullName ).FormatAsHtmlTitle();

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( "Name", prayerRequest.FullName );
            descriptionList.Add( "Category", prayerRequest.Category != null ? prayerRequest.Category.Name : "" );
            descriptionList.Add( "Request", HttpUtility.HtmlEncode( prayerRequest.Text ) );
            descriptionList.Add( "Answer", HttpUtility.HtmlEncode( prayerRequest.Answer ) );
            lMainDetails.Text = descriptionList.Html;

            ShowStatus( prayerRequest, this.CurrentPerson, hlblFlaggedMessageRO );
            ShowPrayerCount( prayerRequest );

            if ( ! prayerRequest.IsApproved.HasValue )
            {
                hlblStatus.Visible = true;
                hlblStatus.Text = "Pending Approval";
            }
            else if ( prayerRequest.IsApproved.HasValue && ( !prayerRequest.IsApproved ?? false ) )
            {
                hlblStatus.Visible = true;
                hlblStatus.Text = "Unapproved";
            }

            hlblUrgent.Visible = ( prayerRequest.IsUrgent ?? false );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowEditDetails( PrayerRequest prayerRequest )
        {
            SetEditMode(true);

            lActionTitle.Text = ( prayerRequest.Id > 0 ) ?
                ActionTitle.Edit( PrayerRequest.FriendlyTypeName ).FormatAsHtmlTitle() :
                ActionTitle.Add( PrayerRequest.FriendlyTypeName ).FormatAsHtmlTitle();

            pnlDetails.Visible = true;

            catpCategory.SetValue( prayerRequest.Category );

            dtbFirstName.Text = prayerRequest.FirstName;
            dtbLastName.Text = prayerRequest.LastName;
            dtbText.Text =  HttpUtility.HtmlDecode( prayerRequest.Text );
            dtbAnswer.Text = HttpUtility.HtmlDecode( prayerRequest.Answer );

            // If no expiration date is set, then use the default setting.
            if ( !prayerRequest.ExpirationDate.HasValue )
            {
                var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                dpExpirationDate.SelectedDate = DateTime.Now.AddDays( expireDays );
            }
            else
            {
                dpExpirationDate.SelectedDate = prayerRequest.ExpirationDate;
            }

            ShowStatus( prayerRequest, this.CurrentPerson, hlblFlaggedMessage );

            cbIsPublic.Checked = prayerRequest.IsPublic ?? false;
            cbIsUrgent.Checked = prayerRequest.IsUrgent ?? false;
            cbIsActive.Checked = prayerRequest.IsActive ?? false;
            cbAllowComments.Checked = prayerRequest.AllowComments ?? false;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="enableEdit">if set to <c>true</c> [enable edit].</param>
        private void SetEditMode(bool enableEdit)
        {
            fieldsetViewDetails.Visible = !enableEdit;
            pnlEditDetails.Visible = enableEdit;
        }

        /// <summary>
        /// Shows the prayer count.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowPrayerCount( PrayerRequest prayerRequest )
        {
            if ( prayerRequest.PrayerCount > 10 )
            {
                badgePrayerCount.BadgeType="success";
            }

            if ( prayerRequest.PrayerCount > 0 )
            {
                badgePrayerCount.Text = string.Format( "{0} prayers", prayerRequest.PrayerCount ?? 0 );
            }
            badgePrayerCount.ToolTip = string.Format( "{0} prayers so far.", prayerRequest.PrayerCount ?? 0 );
        }

        /// <summary>
        /// Shows the status flags
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="person">The person.</param>
        /// <param name="lFlagged">The l flagged.</param>
        private void ShowStatus( PrayerRequest prayerRequest, Person person, HighlightLabel lFlagged )
        {
            int flagCount = prayerRequest.FlagCount ?? 0;
            if ( flagCount > 0 )
            {
                lFlagged.Visible = true;
                lFlagged.Text = string.Format( "flagged {0} times", flagCount );
            }

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

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SaveRequest();
        }

        /// <summary>
        /// Saves the prayer request.
        /// </summary>
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
                prayerRequest.EnteredDate = DateTime.Now;
            }
            else
            {
                prayerRequest = prayerRequestService.Get( prayerRequestId );
            }

            // If changing from NOT-approved to approved, record who and when
            if ( !(prayerRequest.IsApproved ?? false) && cbApproved.Checked )
            {
                prayerRequest.ApprovedByPersonId = CurrentPerson.Id;
                prayerRequest.ApprovedOnDate = DateTime.Now;
                // reset the flag count only to zero ONLY if it had a value previously.
                if ( prayerRequest.FlagCount.HasValue && prayerRequest.FlagCount > 0 )
                {
                    prayerRequest.FlagCount = 0;
                }
            }

            // If no expiration date was manually set, then use the default setting.
            if ( !dpExpirationDate.SelectedDate.HasValue )
            {
                var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                prayerRequest.ExpirationDate = DateTime.Now.AddDays( expireDays );
            }
            else
            {
                prayerRequest.ExpirationDate = dpExpirationDate.SelectedDate;
            }

            // If no category was selected, then use the default category if there is one.
            int? categoryId = catpCategory.SelectedValueAsInt();
            var defaultCategoryGuid = GetAttributeValue( "DefaultCategory" );
            if ( categoryId == null && !string.IsNullOrEmpty( defaultCategoryGuid ) )
            {
                var category = new CategoryService().Get( new Guid( defaultCategoryGuid ) );
                categoryId = category.Id;
            }
            prayerRequest.CategoryId = categoryId;

            // Now record all the bits...
            prayerRequest.IsApproved = cbApproved.Checked;
            prayerRequest.IsActive = cbIsActive.Checked;
            prayerRequest.IsUrgent = cbIsUrgent.Checked;
            prayerRequest.AllowComments = cbAllowComments.Checked;
            prayerRequest.IsPublic = cbIsPublic.Checked;
            prayerRequest.FirstName = dtbFirstName.Text;
            prayerRequest.LastName = dtbLastName.Text;
            prayerRequest.Text = HttpUtility.HtmlEncode( dtbText.Text );
            prayerRequest.Answer = HttpUtility.HtmlEncode( dtbAnswer.Text );

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

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();       
        }
        #endregion

    }
}