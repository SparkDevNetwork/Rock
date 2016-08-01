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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

using Newtonsoft.Json.Linq;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Member Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group member for editing role, status, etc." )]
    [LinkedPage( "Registration Page", "Page used for viewing the registration(s) associated with a particular group member", false, "", "", 0 )]

    [BooleanField( "Show 'Move to another group' button", "Set to false to hide the 'Move to another group' button", true, "", 1, "ShowMoveToOtherGroup" )]
    public partial class GroupMemberDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += GroupMemberDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upDetail );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the GroupMemberDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void GroupMemberDetail_BlockUpdated( object sender, EventArgs e )
        {
            SetBlockOptions();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            ClearErrorMessage();

            if ( !Page.IsPostBack )
            {
                SetBlockOptions();
                ShowDetail( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
            }
            else
            {
                var groupMember = new GroupMember { GroupId = hfGroupId.ValueAsInt() };
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, false, BlockValidationGroup );
                }
            }
        }

        /// <summary>
        /// Sets the block options.
        /// </summary>
        public void SetBlockOptions()
        {
            bool showMoveToOtherGroup = this.GetAttributeValue( "ShowMoveToOtherGroup" ).AsBooleanOrNull() ?? true;
            btnShowMoveDialog.Visible = showMoveToOtherGroup;
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupMemberId = PageParameter( pageReference, "GroupMemberId" ).AsIntegerOrNull();
            if ( groupMemberId != null )
            {
                GroupMember groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId.Value );
                if ( groupMember != null )
                {
                    var parentPageReference = PageReference.GetParentPageReferences( this.RockPage, this.PageCache, pageReference ).LastOrDefault();

                    if ( parentPageReference != null )
                    {
                        var groupIdParam = parentPageReference.QueryString["GroupId"].AsIntegerOrNull();
                        if ( !groupIdParam.HasValue || groupIdParam.Value != groupMember.GroupId)
                        {
                            // if the GroupMember's Group isn't included in the breadcrumbs, make sure to add the Group to the breadcrumbs so we know which group the group member is in
                            breadCrumbs.Add( new BreadCrumb( groupMember.Group.Name, true ) );
                        }
                    }
                    
                    breadCrumbs.Add( new BreadCrumb( groupMember.Person.FullName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group Member", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveGroupMember();

            if ( cvGroupMember.IsValid )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAndAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveThenAdd_Click( object sender, EventArgs e )
        {
            SaveGroupMember();

            if ( cvGroupMember.IsValid )
            {
                ShowDetail( 0, hfGroupId.Value.AsIntegerOrNull() );
            }
        }

        private void SaveGroupMember()
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                GroupMember groupMember;

                int groupMemberId = int.Parse( hfGroupMemberId.Value );

                GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );

                // check to see if the user selected a role
                if ( role == null )
                {
                    nbErrorMessage.Title = "Please select a Role";
                    return;
                }

                // if adding a new group member 
                if ( groupMemberId.Equals( 0 ) )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = hfGroupId.ValueAsInt();
                }
                else
                {
                    // load existing group member
                    groupMember = groupMemberService.Get( groupMemberId );
                }

                groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
                groupMember.GroupRoleId = role.Id;
                groupMember.Note = tbNote.Text;
                groupMember.GroupMemberStatus = rblStatus.SelectedValueAsEnum<GroupMemberStatus>();

                if ( cbIsNotified.Visible )
                {
                    groupMember.IsNotified = cbIsNotified.Checked;
                }

                if ( pnlRequirements.Visible )
                {
                    foreach ( var checkboxItem in cblManualRequirements.Items.OfType<ListItem>() )
                    {
                        int groupRequirementId = checkboxItem.Value.AsInteger();
                        var groupMemberRequirement = groupMember.GroupMemberRequirements.FirstOrDefault( a => a.GroupRequirementId == groupRequirementId );
                        bool metRequirement = checkboxItem.Selected;
                        if ( metRequirement )
                        {
                            if ( groupMemberRequirement == null )
                            {
                                groupMemberRequirement = new GroupMemberRequirement();
                                groupMemberRequirement.GroupRequirementId = groupRequirementId;

                                groupMember.GroupMemberRequirements.Add( groupMemberRequirement );
                            }

                            // set the RequirementMetDateTime if it hasn't been set already
                            groupMemberRequirement.RequirementMetDateTime = groupMemberRequirement.RequirementMetDateTime ?? RockDateTime.Now;

                            groupMemberRequirement.LastRequirementCheckDateTime = RockDateTime.Now;
                        }
                        else
                        {
                            if ( groupMemberRequirement != null )
                            {
                                // doesn't meets the requirement
                                groupMemberRequirement.RequirementMetDateTime = null;
                                groupMemberRequirement.LastRequirementCheckDateTime = RockDateTime.Now;
                            }
                        }
                    }
                }

                groupMember.LoadAttributes();

                Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

                if ( !Page.IsValid )
                {
                    return;
                }

                // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
                // So, make sure a message is displayed in the validation summary
                cvGroupMember.IsValid = groupMember.IsValid;

                if ( !cvGroupMember.IsValid )
                {
                    cvGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                // using WrapTransaction because there are three Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( groupMember.Id.Equals( 0 ) )
                    {
                        groupMemberService.Add( groupMember );
                    }

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                } );

                groupMember.CalculateRequirements( rockContext, true );

                Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    Rock.Security.Role.Flush( group.Id );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupMemberId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
                GroupMember groupMember = groupMemberService.Get( int.Parse( hfGroupMemberId.Value ) );

                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = groupMember.GroupId.ToString();
                NavigateToParentPage( qryString );
            }
        }

        protected void lbResendDocumentRequest_Click( object sender, EventArgs e )
        {
            int groupMemberId = PageParameter( "GroupMemberId" ).AsInteger();
            if ( groupMemberId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                    if ( groupMember != null && groupMember.Group != null )
                    {
                        var sendErrorMessages = new List<string>();

                        string documentName = string.Format( "{0}_{1}", groupMember.Group.Name.RemoveSpecialCharacters(), groupMember.Person.FullName.RemoveSpecialCharacters() );
                        if ( new SignatureDocumentTemplateService( rockContext ).SendDocument(
                            groupMember.Group.RequiredSignatureDocumentTemplate, groupMember.Person, groupMember.Person, documentName, groupMember.Person.Email, out sendErrorMessages ) )
                        {
                            rockContext.SaveChanges();
                            maSignatureRequestSent.Show( "A Signature Request Has Been Sent!", Rock.Web.UI.Controls.ModalAlertType.Information );
                            ShowRequiredDocumentStatus( rockContext, groupMember, groupMember.Group );
                        }
                        else
                        {
                            string errorMessage = string.Format( "Unable to send a signature request: <ul><li>{0}</li></ul>", sendErrorMessages.AsDelimited( "</li><li>" ) );
                            maSignatureRequestSent.Show( errorMessage, Rock.Web.UI.Controls.ModalAlertType.Alert );
                        }
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        public void ShowDetail( int groupMemberId )
        {
            ShowDetail( groupMemberId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="groupId">The group id.</param>
        public void ShowDetail( int groupMemberId, int? groupId )
        {
            // autoexpand the person picker if this is an add
            var personPickerStartupScript = @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });";

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "StartupScript", personPickerStartupScript, true );

            var rockContext = new RockContext();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
            }
            else
            {
                // only create a new one if parent was specified
                if ( groupId.HasValue )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = groupId.Value;
                    groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.DateTimeAdded = RockDateTime.Now;
                }
            }

            if ( groupMember == null )
            {
                if ( groupMemberId > 0 )
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                    nbErrorMessage.Title = "Warning";
                    nbErrorMessage.Text = "Group Member not found. Group Member may have been moved to another group or deleted.";
                }
                else
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbErrorMessage.Title = "Invalid Request";
                    nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid GroupMemberId or GroupId parameter is required.";
                }

                pnlEditDetails.Visible = false;
                return;
            }

            pnlEditDetails.Visible = true;

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                cbIsNotified.Checked = groupMember.IsNotified;
                cbIsNotified.Visible = true;
                cbIsNotified.Help = "If this box is unchecked and a <a href=\"http://www.rockrms.com/Rock/BookContent/7/#servicejobsrelatingtogroups\">group leader notification job</a> is enabled then a notification will be sent to the group's leaders when this group member is saved.";
            }
            else
            {
                cbIsNotified.Visible = false;
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            var group = groupMember.Group;
            if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
            {
                lGroupIconHtml.Text = string.Format( "<i class='{0}' ></i>", group.GroupType.IconCssClass );
            }
            else
            {
                lGroupIconHtml.Text = "<i class='fa fa-user' ></i>";
            }

            if ( groupMember.Id.Equals( 0 ) )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( groupMember.Group.GroupType.GroupTerm + " " + groupMember.Group.GroupType.GroupMemberTerm ).FormatAsHtmlTitle();
                btnSaveThenAdd.Visible = true;
            }
            else
            {
                lReadOnlyTitle.Text = groupMember.Person.FullName.FormatAsHtmlTitle();
                btnSaveThenAdd.Visible = false;
            }

            if ( groupMember.DateTimeAdded.HasValue )
            {
                hfDateAdded.Text = string.Format( "Added: {0}", groupMember.DateTimeAdded.Value.ToShortDateString() );
                hfDateAdded.Visible = true;
            }
            else
            {
                hfDateAdded.Text = string.Empty;
                hfDateAdded.Visible = false;
            }

            // user has to have EDIT Auth to the Block OR the group
            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) && !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( groupMember.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            btnSave.Visible = !readOnly;

            if ( readOnly || groupMember.Id == 0) 
            {
                // hide the ShowMoveDialog if this is readOnly or if this is a new group member (can't move a group member that doesn't exist yet)
                btnShowMoveDialog.Visible = false;
            }
            
            LoadDropDowns();

            ShowRequiredDocumentStatus( rockContext, groupMember, group );

            ppGroupMemberPerson.SetValue( groupMember.Person );
            ppGroupMemberPerson.Enabled = !readOnly;

            if ( groupMember.Id != 0 )
            {
                // once a group member record is saved, don't let them change the person
                ppGroupMemberPerson.Enabled = false;
            }

            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            ddlGroupRole.Enabled = !readOnly;

            tbNote.Text = groupMember.Note;
            tbNote.ReadOnly = readOnly;

            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );
            rblStatus.Enabled = !readOnly;
            rblStatus.Label = string.Format( "{0} Status", group.GroupType.GroupMemberTerm );

            var registrations = new RegistrationRegistrantService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.Registration != null &&
                    r.Registration.RegistrationInstance != null &&
                    r.GroupMemberId.HasValue &&
                    r.GroupMemberId.Value == groupMember.Id )
                .Select( r => new
                {
                    Id = r.Registration.Id,
                    Name = r.Registration.RegistrationInstance.Name
                } )
                .ToList();
            if ( registrations.Any() )
            {
                rcwLinkedRegistrations.Visible = true;
                rptLinkedRegistrations.DataSource = registrations;
                rptLinkedRegistrations.DataBind();
            }
            else
            {
                rcwLinkedRegistrations.Visible = false;
            }

            groupMember.LoadAttributes();
            phAttributes.Controls.Clear();

            Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, true, string.Empty, true );
            if ( readOnly )
            {
                Rock.Attribute.Helper.AddDisplayControls( groupMember, phAttributesReadOnly );
                phAttributesReadOnly.Visible = true;
                phAttributes.Visible = false;
            }
            else
            {
                phAttributesReadOnly.Visible = false;
                phAttributes.Visible = true;
            }

            var groupHasRequirements = group.GroupRequirements.Any();
            pnlRequirements.Visible = groupHasRequirements;
            btnReCheckRequirements.Visible = groupHasRequirements;

            ShowGroupRequirementsStatuses();
        }

        private void ShowRequiredDocumentStatus( RockContext rockContext, GroupMember groupMember, Group group )
        {
            if ( groupMember.Person != null && group.RequiredSignatureDocumentTemplate != null )
            {
                var documents = new SignatureDocumentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == group.RequiredSignatureDocumentTemplate.Id &&
                        d.AppliesToPersonAlias.PersonId == groupMember.Person.Id )
                    .ToList();
                if ( !documents.Any( d => d.Status == SignatureDocumentStatus.Signed ) )
                {
                    var lastSent = documents.Any( d => d.Status == SignatureDocumentStatus.Sent ) ?
                        documents.Where( d => d.Status == SignatureDocumentStatus.Sent ).Max( d => d.LastInviteDate ) : (DateTime?)null;
                    pnlRequiredSignatureDocument.Visible = true;

                    if ( lastSent.HasValue )
                    {
                        lbResendDocumentRequest.Text = "Resend Signature Request";
                        lRequiredSignatureDocumentMessage.Text =string.Format("A signed {0} document has not yet been received for {1}. The last request was sent {2}.", group.RequiredSignatureDocumentTemplate.Name, groupMember.Person.NickName, lastSent.Value.ToElapsedString() );
                    }
                    else
                    {
                        lbResendDocumentRequest.Text = "Send Signature Request";
                        lRequiredSignatureDocumentMessage.Text = string.Format("The required {0} document has not yet been sent to {1} for signing.", group.RequiredSignatureDocumentTemplate.Name, groupMember.Person.NickName );
                    }
                }
                else
                {
                    pnlRequiredSignatureDocument.Visible = false;
                }
            }
            else
            {
                pnlRequiredSignatureDocument.Visible = false;
            }
        }

        /// <summary>
        /// Shows the group requirements statuses.
        /// </summary>
        private void ShowGroupRequirementsStatuses()
        {
            if ( !pnlRequirements.Visible )
            {
                // group doesn't have any requirements
                return;
            }

            var rockContext = new RockContext();
            int groupMemberId = hfGroupMemberId.Value.AsInteger();
            var groupId = hfGroupId.Value.AsInteger();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
            }
            else
            {
                // only create a new one if person is selected
                if ( ppGroupMemberPerson.PersonId.HasValue )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = groupId;
                    groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
                }
            }

            cblManualRequirements.Items.Clear();
            lRequirementsLabels.Text = string.Empty;

            if ( groupMember == null )
            {
                // no person selected yet, so don't show anything
                rcwRequirements.Visible = false;
                return;
            }

            rcwRequirements.Visible = true;

            IEnumerable<GroupRequirementStatus> requirementsResults;

            if ( groupMember.IsNewOrChangedGroupMember( rockContext ) )
            {
                requirementsResults = groupMember.Group.PersonMeetsGroupRequirements( ppGroupMemberPerson.PersonId ?? 0, ddlGroupRole.SelectedValue.AsIntegerOrNull() );
            }
            else
            {
                requirementsResults = groupMember.GetGroupRequirementsStatuses().ToList();
            }

            // only show the requirements that apply to the GroupRole (or all Roles)
            foreach ( var requirementResult in requirementsResults.Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable ) )
            {
                if ( requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                {
                    var checkboxItem = new ListItem( requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel, requirementResult.GroupRequirement.Id.ToString() );
                    if ( string.IsNullOrEmpty( checkboxItem.Text ) )
                    {
                        checkboxItem.Text = requirementResult.GroupRequirement.GroupRequirementType.Name;
                    }

                    checkboxItem.Selected = requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets;
                    cblManualRequirements.Items.Add( checkboxItem );
                }
                else
                {
                    string labelText;
                    string labelType;
                    string labelTooltip;
                    if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.PositiveLabel;
                        labelType = "success";
                    }
                    else if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.WarningLabel;
                        labelType = "warning";
                    }
                    else
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                        labelType = "danger";
                    }

                    if ( string.IsNullOrEmpty( labelText ) )
                    {
                        labelText = requirementResult.GroupRequirement.GroupRequirementType.Name;
                    }

                    if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                    {
                        labelTooltip = requirementResult.RequirementWarningDateTime.HasValue
                            ? "Last Checked: " + requirementResult.RequirementWarningDateTime.Value.ToString( "g" )
                            : "Not calculated yet";
                    }
                    else
                    {
                        labelTooltip = requirementResult.LastRequirementCheckDateTime.HasValue
                            ? "Last Checked: " + requirementResult.LastRequirementCheckDateTime.Value.ToString( "g" )
                            : "Not calculated yet";
                    }


                    lRequirementsLabels.Text += string.Format(
                        @"<span class='label label-{1}' title='{2}'>{0}</span>
                        ",
                        labelText,
                        labelType,
                        labelTooltip);
                }
            }

            var requirementsWithErrors = requirementsResults.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error ).ToList();
            if ( requirementsWithErrors.Any() )
            {
                nbRequirementsErrors.Visible = true;
                nbRequirementsErrors.Text = string.Format(
                    "An error occurred in one or more of the requirement calculations: <br /> {0}",
                    requirementsWithErrors.AsDelimited( "<br />" ) );
            }
            else
            {
                nbRequirementsErrors.Visible = false;
            }
        }

        /// <summary>
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            int groupId = hfGroupId.ValueAsInt();
            Group group = new GroupService( new RockContext() ).Get( groupId );
            if ( group != null )
            {
                ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                ddlGroupRole.DataBind();
            }

            rblStatus.BindToEnum<GroupMemberStatus>();
        }

        /// <summary>
        /// Registrations the URL.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        protected string RegistrationUrl( int registrationId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "RegistrationId", registrationId.ToString() );
            return LinkedPageUrl( "RegistrationPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnReCheckRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReCheckRequirements_Click( object sender, EventArgs e )
        {
            CalculateRequirements();
            nbRecheckedNotification.Text = "Successfully re-checked requirements.";
            nbRecheckedNotification.Visible = true;
        }

        /// <summary>
        /// Calculates (or re-calculates) the requirements, then updates the results on the UI
        /// </summary>
        private void CalculateRequirements()
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );

            if ( groupMember != null && !groupMember.IsNewOrChangedGroupMember( rockContext ) )
            {
                groupMember.CalculateRequirements( rockContext, true );
            }

            ShowGroupRequirementsStatuses();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            CalculateRequirements();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppGroupMemberPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppGroupMemberPerson_SelectPerson( object sender, EventArgs e )
        {
            CalculateRequirements();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnShowMoveDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowMoveDialog_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                lCurrentGroup.Text = groupMember.Group.Name;
                gpMoveGroupMember.SetValue( null );
                grpMoveGroupMember.Visible = false;
                nbMoveGroupMemberWarning.Visible = false;
                mdMoveGroupMember.Visible = true;
                mdMoveGroupMember.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMoveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMoveGroupMember_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMember = groupMemberService.Get( hfGroupMemberId.Value.AsInteger() );
            groupMember.LoadAttributes();
            int destGroupId = gpMoveGroupMember.SelectedValue.AsInteger();
            var destGroup = new GroupService( rockContext ).Get( destGroupId );

            var destGroupMember = groupMemberService.Queryable().Where( a =>
                a.GroupId == destGroupId
                && a.PersonId == groupMember.PersonId
                && a.GroupRoleId == grpMoveGroupMember.GroupRoleId ).FirstOrDefault();

            if ( destGroupMember != null )
            {
                nbMoveGroupMemberWarning.Visible = true;
                nbMoveGroupMemberWarning.Text = string.Format( "{0} is already in {1}", groupMember.Person, destGroupMember.Group );
                return;
            }

            if ( !grpMoveGroupMember.GroupRoleId.HasValue )
            {
                nbMoveGroupMemberWarning.Visible = true;
                nbMoveGroupMemberWarning.Text = string.Format( "Please select a Group Role" );
                return;
            }

            string canDeleteWarning;
            if ( !groupMemberService.CanDelete( groupMember, out canDeleteWarning ) )
            {
                nbMoveGroupMemberWarning.Visible = true;
                nbMoveGroupMemberWarning.Text = string.Format( "Unable to remove {0} from {1}: {2}", groupMember.Person, groupMember.Group, canDeleteWarning );
                return;
            }

            destGroupMember = new GroupMember();
            destGroupMember.GroupId = destGroupId;
            destGroupMember.GroupRoleId = grpMoveGroupMember.GroupRoleId.Value;
            destGroupMember.PersonId = groupMember.PersonId;
            destGroupMember.LoadAttributes();

            foreach ( var attribute in groupMember.Attributes )
            {
                if ( destGroupMember.Attributes.Any( a => a.Key == attribute.Key && a.Value.FieldTypeId == attribute.Value.FieldTypeId ) )
                {
                    destGroupMember.SetAttributeValue( attribute.Key, groupMember.GetAttributeValue( attribute.Key ) );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                groupMemberService.Add( destGroupMember );
                rockContext.SaveChanges();
                destGroupMember.SaveAttributeValues( rockContext );

                // move any Note records that were associated with the old groupMember to the new groupMember record
                if ( cbMoveGroupMemberMoveNotes.Checked )
                {
                    destGroupMember.Note = groupMember.Note;
                    int groupMemberEntityTypeId = EntityTypeCache.GetId<Rock.Model.GroupMember>().Value;
                    var noteService = new NoteService( rockContext );
                    var groupMemberNotes = noteService.Queryable().Where( a => a.NoteType.EntityTypeId == groupMemberEntityTypeId && a.EntityId == groupMember.Id );
                    foreach ( var note in groupMemberNotes )
                    {
                        note.EntityId = destGroupMember.Id;
                    }
                    
                    rockContext.SaveChanges();
                }

                groupMemberService.Delete( groupMember );
                rockContext.SaveChanges();

                destGroupMember.CalculateRequirements( rockContext, true );
            } );

            var queryString = new Dictionary<string, string>();
            queryString.Add( "GroupMemberId", destGroupMember.Id.ToString() );
            this.NavigateToPage( this.RockPage.Guid, queryString );
        }

        /// <summary>
        /// Handles the SelectItem event of the gpMoveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpMoveGroupMember_SelectItem( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var destGroup = new GroupService( rockContext ).Get( gpMoveGroupMember.SelectedValue.AsInteger() );
            if ( destGroup != null )
            {
                var destTempGroupMember = new GroupMember { Group = destGroup, GroupId = destGroup.Id };
                destTempGroupMember.LoadAttributes( rockContext );
                var destGroupMemberAttributes = destTempGroupMember.Attributes;
                var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
                groupMember.LoadAttributes();
                var currentGroupMemberAttributes = groupMember.Attributes;

                var lostAttributes = currentGroupMemberAttributes.Where( a => !destGroupMemberAttributes.Any( d => d.Key == a.Key && d.Value.FieldTypeId == a.Value.FieldTypeId ) );
                nbMoveGroupMemberWarning.Visible = lostAttributes.Any();
                nbMoveGroupMemberWarning.Text = "The destination group does not have the same group member attributes as the source. Some loss of data may occur";

                if ( destGroup.Id == groupMember.GroupId )
                {
                    grpMoveGroupMember.Visible = false;
                    nbMoveGroupMemberWarning.Visible = true;
                    nbMoveGroupMemberWarning.Text = "The destination group is the same as the current group";
                }
                else
                {
                    grpMoveGroupMember.Visible = true;
                    grpMoveGroupMember.GroupTypeId = destGroup.GroupTypeId;
                    grpMoveGroupMember.GroupRoleId = destGroup.GroupType.DefaultGroupRoleId;
                }
            }
            else
            {
                nbMoveGroupMemberWarning.Visible = false;
                grpMoveGroupMember.Visible = false;
            }
        }

    }
}