// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request Detail" )]
    [Category( "Prayer" )]
    [Description( "Displays the details of a given Prayer Request for viewing or editing." )]

    [IntegerField( "Expires After (Days)", "Default number of days until the request will expire.", false, 14, "", 0, "ExpireDays" )]
    [CategoryField( "Default Category", "If a category is not selected, choose a default category to use for all new prayer requests.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "", 1, "DefaultCategory" )]
    public partial class PrayerRequestDetail : RockBlock, IDetailBlock
    {
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
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "prayerRequestId" ).AsInteger() );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the edit Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            int prayerRequestId = hfPrayerRequestId.ValueAsInt();
            PrayerRequest item = new PrayerRequestService( new RockContext() )
                .Queryable( "RequestedByPersonAlias.Person,ApprovedByPersonAlias.Person" )
                .FirstOrDefault( p => p.Id == prayerRequestId );
            ShowEditDetails( item );
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
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the prayer request's detail.
        /// </summary>
        /// <param name="prayerId">The prayer identifier.</param>
        public void ShowDetail( int prayerId )
        {
            PrayerRequest prayerRequest = null;

            if ( prayerId != 0 )
            {
                prayerRequest = new PrayerRequestService( new RockContext() )
                    .Queryable( "RequestedByPersonAlias.Person,ApprovedByPersonAlias.Person" )
                    .FirstOrDefault( p => p.Id == prayerId );
            }

            if ( prayerRequest == null )
            {
                prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = true, AllowComments = true };
            }

            hfPrayerRequestId.Value = prayerRequest.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
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

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowReadonlyDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( false );
            lActionTitle.Text = string.Format( "{0} Prayer Request", prayerRequest.FullName ).FormatAsHtmlTitle();

            DescriptionList descriptionList = new DescriptionList();
            if ( prayerRequest.RequestedByPersonAlias != null )
            {
                descriptionList.Add( "Requested By", prayerRequest.RequestedByPersonAlias.Person.FullName );
            }
            descriptionList.Add( "Name", prayerRequest.FullName );
            descriptionList.Add( "Category", prayerRequest.Category != null ? prayerRequest.Category.Name : string.Empty );
            descriptionList.Add( "Request", prayerRequest.Text.ScrubHtmlAndConvertCrLfToBr() );
            descriptionList.Add( "Answer", prayerRequest.Answer.ScrubHtmlAndConvertCrLfToBr() );
            lMainDetails.Text = descriptionList.Html;

            ShowStatus( prayerRequest, this.CurrentPerson, hlblFlaggedMessageRO );
            ShowPrayerCount( prayerRequest );

            if ( !prayerRequest.IsApproved.HasValue )
            {
                hlblStatus.Visible = true;
                hlblStatus.Text = "Pending Approval";
            }
            else if ( prayerRequest.IsApproved.HasValue && ( !prayerRequest.IsApproved ?? false ) )
            {
                hlblStatus.Visible = true;
                hlblStatus.Text = "Unapproved";
            }

            hlblUrgent.Visible = prayerRequest.IsUrgent ?? false;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowEditDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( true );

            if ( prayerRequest.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( PrayerRequest.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( PrayerRequest.FriendlyTypeName ).FormatAsHtmlTitle();
                if ( CurrentPersonAlias != null && CurrentPerson != null )
                {
                    prayerRequest.RequestedByPersonAlias = CurrentPersonAlias;
                    prayerRequest.FirstName = CurrentPerson.NickName;
                    prayerRequest.LastName = CurrentPerson.LastName;
                }
            }

            pnlDetails.Visible = true;

            catpCategory.SetValue( prayerRequest.Category );

            dtbFirstName.Text = prayerRequest.FirstName;
            dtbLastName.Text = prayerRequest.LastName;
            dtbText.Text = prayerRequest.Text;
            dtbAnswer.Text = prayerRequest.Answer;

            if ( prayerRequest.RequestedByPersonAlias != null )
            {
                ppRequestor.SetValue( prayerRequest.RequestedByPersonAlias.Person );
            }
            else
            {
                ppRequestor.SetValue( null );
            }

            // If no expiration date is set, then use the default setting.
            if ( !prayerRequest.ExpirationDate.HasValue )
            {
                var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                dpExpirationDate.SelectedDate = RockDateTime.Now.AddDays( expireDays );
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
        private void SetEditMode( bool enableEdit )
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
                badgePrayerCount.BadgeType = "success";
            }

            if ( prayerRequest.PrayerCount > 0 )
            {
                badgePrayerCount.Text = string.Format( "{0} prayers", prayerRequest.PrayerCount ?? 0 );
            }

            badgePrayerCount.ToolTip = string.Format( "{0} prayers offered by the team for this request.", prayerRequest.PrayerCount ?? 0 );
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

            if ( person != null && 
                (prayerRequest.IsApproved ?? false ) && 
                prayerRequest.ApprovedByPersonAlias != null &&
                prayerRequest.ApprovedByPersonAlias.Person != null )
            {
                lblApprovedByPerson.Visible = true;
                lblApprovedByPerson.Text = string.Format( "approved by {0}", prayerRequest.ApprovedByPersonAlias.Person.FullName );
            }
            else
            {
                lblApprovedByPerson.Visible = false;
            }
        }

        /// <summary>
        /// Saves the prayer request.
        /// </summary>
        private void SaveRequest()
        {
            var rockContext = new RockContext();
            PrayerRequest prayerRequest;
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );

            int prayerRequestId = hfPrayerRequestId.Value.AsInteger();

            // Fetch the prayer request or create a new one if needed
            if ( prayerRequestId == 0 )
            {
                prayerRequest = new PrayerRequest();
                prayerRequestService.Add( prayerRequest );
                prayerRequest.EnteredDateTime = RockDateTime.Now;
            }
            else
            {
                prayerRequest = prayerRequestService.Get( prayerRequestId );
            }

            if ( ppRequestor.PersonId.HasValue )
            {
                prayerRequest.RequestedByPersonAliasId = ppRequestor.PersonAliasId;
            }

            // If changing from NOT-approved to approved, record who and when
            if ( !( prayerRequest.IsApproved ?? false ) && cbApproved.Checked )
            {
                prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

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
                prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
            }
            else
            {
                prayerRequest.ExpirationDate = dpExpirationDate.SelectedDate;
            }

            // If no category was selected, then use the default category if there is one.
            int? categoryId = catpCategory.SelectedValueAsInt();
            Guid defaultCategoryGuid = GetAttributeValue( "DefaultCategory" ).AsGuid();
            if ( categoryId == null && !defaultCategoryGuid.IsEmpty() )
            {
                var category = new CategoryService( rockContext ).Get( defaultCategoryGuid );
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
            prayerRequest.Text = dtbText.Text.Trim();
            prayerRequest.Answer = dtbAnswer.Text.Trim();

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !prayerRequest.IsValid )
            {
                // field controls render error messages
                return;
            }

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        #endregion

        #region Possible Extension Method 

        /// <summary>
        /// Scrubs any html from the string but converts carriage returns into html &lt;br/&gt; suitable for web display.
        /// </summary>
        /// <param name="str">a string that may contain unsanitized html and carriage returns</param>
        /// <returns>a string that has been scrubbed of any html with carriage returns converted to html br</returns>
        public static string ScrubHtmlAndConvertCrLfToBr( string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            // Note: \u00A7 is the section symbol

            // First we convert newlines and carriage returns to a character that can
            // pass through the Sanitizer.
            str = str.Replace( Environment.NewLine, "\u00A7" ).Replace( "\x0A", "\u00A7" );

            // Now we pass it to sanitizer and then convert those section-symbols to <br/>
            return str.SanitizeHtml().Replace( "\u00A7", "<br/>" );
        }

        #endregion
    }
}