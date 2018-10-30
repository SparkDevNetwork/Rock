﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request Detail" )]
    [Category( "Prayer" )]
    [Description( "Displays the details of a given Prayer Request for viewing or editing." )]

    [IntegerField( "Expires After (Days)", "Default number of days until the request will expire.", false, 14, "", 0, "ExpireDays" )]
    [CategoryField( "Default Category", "If a category is not selected, choose a default category to use for all new prayer requests.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "", 1, "DefaultCategory" )]
    [BooleanField( "Set Current Person To Requester", "Will set the current person as the requester. This is useful in self-entry situations.", false, order: 2 )]
    [BooleanField( "Require Last Name", "Require that a last name be entered", true, "", 3 )]
    [BooleanField( "Default To Public", "If enabled, all prayers will be set to public by default", false, "", 4)]
    [BooleanField( "Default Allow Comments Checked", "If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", true, order: 5 )]
    [BooleanField("Require Campus", "Require that a campus be selected", false, "", 6 )]
    public partial class PrayerRequestDetail : RockBlock, IDetailBlock
    {
        #region Properties

        /// <summary>
        /// Gets the pending CSS.
        /// </summary>
        /// <value>
        /// The pending CSS.
        /// </value>
        protected string PendingCss
        {
            get
            {
                return ( ViewState["PendingCss"] as string ) ?? "btn-default";
            }

            private set
            {
                ViewState["PendingCss"] = value;
            }
        }

        /// <summary>
        /// Gets the approved CSS.
        /// </summary>
        /// <value>
        /// The approved CSS.
        /// </value>
        protected string ApprovedCss
        {
            get
            {
                return ( ViewState["ApprovedCss"] as string ) ?? "btn-default";
            }

            private set
            {
                ViewState["ApprovedCss"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbDelete.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'prayer request');";

            string scriptFormat = @"
    $('#{0} .btn-toggle').click(function (e) {{

        e.stopImmediatePropagation();

        $(this).find('.btn').removeClass('active');
        $(e.target).addClass('active');

        $(this).find('a').each(function() {{
            if ($(this).hasClass('active')) {{
                $('#{1}').val($(this).attr('data-status'));
                $(this).removeClass('btn-default');
                $(this).addClass( $(this).attr('data-active-css') );
            }} else {{
                $(this).removeClass( $(this).attr('data-active-css') );
                $(this).addClass('btn-default');
            }}
        }});

    }});
";

            string script = string.Format( scriptFormat, pnlStatus.ClientID, hfApprovedStatus.ClientID );
            ScriptManager.RegisterStartupScript( pnlStatus, pnlStatus.GetType(), "status-script-" + this.BlockId.ToString(), script, true );

            tbLastName.Required = GetAttributeValue( "RequireLastName" ).AsBooleanOrNull() ?? true;
            cpCampus.Required = GetAttributeValue("RequireCampus").AsBooleanOrNull() ?? false;
            cpCampus.Campuses = CampusCache.All( false );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "PrayerRequestId" ).AsInteger() );
            }
            else
            {
                if ( pnlEditDetails.Visible )
                {
                    var rockContext = new RockContext();
                    PrayerRequest prayerRequest;
                    int? prayerRequestId = PageParameter( "PrayerRequestId" ).AsIntegerOrNull();
                    if ( prayerRequestId.HasValue && prayerRequestId.Value > 0 )
                    {
                        prayerRequest = new PrayerRequestService( rockContext ).Get( prayerRequestId.Value );
                    }
                    else
                    {
                        prayerRequest = new PrayerRequest { Id = 0 };
                    }

                    prayerRequest.LoadAttributes();
                    phAttributes.Controls.Clear();
                    var excludeForEdit = prayerRequest.Attributes.Where( a => !a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
                    Rock.Attribute.Helper.AddEditControls( prayerRequest, phAttributes, false, BlockValidationGroup, excludeForEdit );
                }
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
            var queryParms = new Dictionary<string, string>();
            if ( !string.IsNullOrWhiteSpace( PageParameter( "PersonId" ) ) )
            {
                queryParms.Add( "PersonId", PageParameter( "PersonId" ) );
            }

            NavigateToParentPage( queryParms );
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            int prayerRequestId = hfPrayerRequestId.ValueAsInt();

            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                maWarning.Show( "You are not authorized to delete this request.", ModalAlertType.Information );
                return;
            }

            var rockContext = new RockContext();
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            PrayerRequest prayerRequest = prayerRequestService.Get( prayerRequestId );

            if ( prayerRequest != null )
            {
                DeleteAllRelatedNotes( prayerRequest, rockContext );

                string errorMessage;
                if ( !prayerRequestService.CanDelete( prayerRequest, out errorMessage ) )
                {
                    maWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                prayerRequestService.Delete( prayerRequest );
                rockContext.SaveChanges();

                var queryParms = new Dictionary<string, string>();
                if ( !string.IsNullOrWhiteSpace( PageParameter( "PersonId" ) ) )
                {
                    queryParms.Add( "PersonId", PageParameter( "PersonId" ) );
                }
                NavigateToParentPage( queryParms );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppRequestor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppRequestor_SelectPerson( object sender, EventArgs e )
        {
            if ( ppRequestor.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var requester = new PersonService( rockContext )
                        .Queryable()
                        .Where( p => p.Id == ppRequestor.PersonId.Value )
                        .Select( p => new
                        {
                            FirstName = p.NickName,
                            LastName = p.LastName,
                            Email = p.Email
                        } ).FirstOrDefault();

                    tbFirstName.Text = requester.FirstName;
                    tbLastName.Text = requester.LastName;
                    tbEmail.Text = requester.Email;
                }
            }
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
                pdAuditDetails.SetEntity( prayerRequest, ResolveRockUrl( "~" ) );
            }

            if ( prayerRequest == null )
            {
                bool isPublic = GetAttributeValue( "DefaultToPublic" ).AsBoolean();
                prayerRequest = new PrayerRequest { Id = 0, IsPublic = isPublic, IsActive = true, IsApproved = true, AllowComments = GetAttributeValue( "DefaultAllowCommentsChecked" ).AsBooleanOrNull() ?? true };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
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

            hlCategory.Text = prayerRequest.Category != null ? prayerRequest.Category.Name : string.Empty;

            if ( readOnly )
            {
                lbEdit.Visible = false;
                lbDelete.Visible = false;
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
            string email = null;
            if ( !string.IsNullOrWhiteSpace( prayerRequest.Email ) )
            {
                email = prayerRequest.Email;
            }
            else if ( prayerRequest.RequestedByPersonAlias != null )
            {
                email = prayerRequest.RequestedByPersonAlias.Person.Email;
            }

            if ( !string.IsNullOrEmpty( email ) )
            {
                descriptionList.Add( "Email", String.Format( "<a href='mailto:{0}'>{0}</a>", email ) );
            }
            descriptionList.Add( "Campus", prayerRequest.Campus );
            descriptionList.Add( "Request", prayerRequest.Text.ScrubHtmlAndConvertCrLfToBr() );
            descriptionList.Add( "Answer", prayerRequest.Answer.ScrubHtmlAndConvertCrLfToBr() );
            lMainDetails.Text = descriptionList.Html;

            prayerRequest.LoadAttributes();
            var attributes = prayerRequest.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            var attributeCategories = Helper.GetAttributeCategories( attributes );

            // Filter to only show attribute / attribute values that the person is authorized to view.
            var excludeForView = prayerRequest.Attributes.Where( a => !a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            Rock.Attribute.Helper.AddDisplayControls( prayerRequest, attributeCategories, phDisplayAttributes, excludeForView, false );

            ShowStatus( prayerRequest, this.CurrentPerson, hlblFlaggedMessageRO );
            ShowPrayerCount( prayerRequest );

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
                if ( CurrentPersonAlias != null && CurrentPerson != null && GetAttributeValue( "SetCurrentPersonToRequester" ).AsBoolean() )
                {
                    prayerRequest.RequestedByPersonAlias = CurrentPersonAlias;
                    prayerRequest.FirstName = CurrentPerson.NickName;
                    prayerRequest.LastName = CurrentPerson.LastName;
                }
            }

            cpCampus.SelectedCampusId = prayerRequest.CampusId;

            pnlDetails.Visible = true;

            var prayRequestCategory = prayerRequest.Category;
            if ( prayRequestCategory == null )
            {
                var defaultCategoryGuid = GetAttributeValue( "DefaultCategory" ).AsGuidOrNull();
                if ( defaultCategoryGuid.HasValue )
                {
                    prayRequestCategory = new CategoryService( new RockContext() ).Get( defaultCategoryGuid.Value );

                }
            }
            catpCategory.SetValue( prayRequestCategory );

            tbFirstName.Text = prayerRequest.FirstName;
            tbLastName.Text = prayerRequest.LastName;
            tbEmail.Text = prayerRequest.Email;
            dtbText.Text = prayerRequest.Text;
            dtbAnswer.Text = prayerRequest.Answer;

            if ( prayerRequest.RequestedByPersonAlias != null )
            {
                ppRequestor.SetValue( prayerRequest.RequestedByPersonAlias.Person );
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "PersonId" ) ) )
                {
                    var requestor = new PersonService( new RockContext() ).Get( PageParameter( "PersonId" ).AsInteger() );
                    ppRequestor.SetValue( requestor );
                    tbFirstName.Text = requestor.NickName;
                    tbLastName.Text = requestor.LastName;
                    tbEmail.Text = requestor.Email;
                }
                else
                {
                    ppRequestor.SetValue( null );
                }
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

            prayerRequest.LoadAttributes();
            phAttributes.Controls.Clear();
            // Filter to only include attribute / attribute values that the person is authorized to edit.
            var excludeForEdit = prayerRequest.Attributes.Where( a => !a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            Rock.Attribute.Helper.AddEditControls( prayerRequest, phAttributes, true, BlockValidationGroup, excludeForEdit );
        }

        /// <summary>
        /// Deletes all related notes.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void DeleteAllRelatedNotes( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var noteTypeService = new NoteTypeService( rockContext );
            var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            var noteService = new NoteService( rockContext );
            var prayerComments = noteService.Get( noteType.Id, prayerRequest.Id );
            foreach ( Note prayerComment in prayerComments )
            {
                noteService.Delete( prayerComment );
            }
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

            ShowApproval( prayerRequest );
        }

        /// <summary>
        /// Shows the approval.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowApproval( PrayerRequest prayerRequest )
        {
            if ( prayerRequest != null )
            {
                pnlStatus.Visible = true;
                PendingCss = prayerRequest.IsApproved == false ? "btn-warning active" : "btn-default";
                ApprovedCss = prayerRequest.IsApproved == true ? "btn-success active" : "btn-default";
                hfApprovedStatus.Value = prayerRequest.IsApproved.ToString();
            }
            else
            {
                hfApprovedStatus.Value = true.ToString();
                pnlStatus.Visible = false;
                divStatus.Visible = false;
            }

            hlStatus.Text = ( prayerRequest.IsApproved ?? false ) ? "Approved" : "Pending";

            hlStatus.LabelType = ( prayerRequest.IsApproved ?? false ) ? LabelType.Success : LabelType.Warning;

            var statusDetail = new System.Text.StringBuilder();
            if ( prayerRequest.ApprovedByPersonAlias != null && prayerRequest.ApprovedByPersonAlias.Person != null )
            {
                statusDetail.AppendFormat( "by {0} ", prayerRequest.ApprovedByPersonAlias.Person.FullName );
            }

            if ( prayerRequest.ApprovedOnDateTime.HasValue )
            {
                statusDetail.AppendFormat(
                    "on {0} at {1}",
                    prayerRequest.ApprovedOnDateTime.Value.ToShortDateString(),
                    prayerRequest.ApprovedOnDateTime.Value.ToShortTimeString() );
            }

            hlStatus.ToolTip = statusDetail.ToString();
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
            if ( !( prayerRequest.IsApproved ?? false ) && hfApprovedStatus.Value.AsBoolean() )
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

            prayerRequest.CampusId = cpCampus.SelectedCampusId;
            prayerRequest.CategoryId = catpCategory.SelectedValueAsInt();

            // Now record all the bits...
            prayerRequest.IsApproved = hfApprovedStatus.Value.AsBoolean();
            prayerRequest.IsActive = cbIsActive.Checked;
            prayerRequest.IsUrgent = cbIsUrgent.Checked;
            prayerRequest.AllowComments = cbAllowComments.Checked;
            prayerRequest.IsPublic = cbIsPublic.Checked;
            prayerRequest.FirstName = tbFirstName.Text;
            prayerRequest.LastName = tbLastName.Text;
            prayerRequest.Email = tbEmail.Text;
            prayerRequest.Text = dtbText.Text.Trim();
            prayerRequest.Answer = dtbAnswer.Text.Trim();

            prayerRequest.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, prayerRequest );

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
            prayerRequest.SaveAttributeValues( rockContext );

            var queryParms = new Dictionary<string, string>();
            if ( !string.IsNullOrWhiteSpace( PageParameter( "PersonId" ) ) )
            {
                queryParms.Add( "PersonId", PageParameter( "PersonId" ) );
            }

            NavigateToParentPage( queryParms );
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