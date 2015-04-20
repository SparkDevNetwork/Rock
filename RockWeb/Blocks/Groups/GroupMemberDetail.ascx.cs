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
using Rock.Web.UI;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Member Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group member for editing role, status, etc." )]
    public partial class GroupMemberDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

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
                ShowDetail( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
            }
            else
            {
                var groupMember = new GroupMember { GroupId = hfGroupId.ValueAsInt() };
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, false );
                }
            }
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
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
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
                groupMember.GroupMemberStatus = rblStatus.SelectedValueAsEnum<GroupMemberStatus>();

                groupMember.LoadAttributes();

                Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

                if ( !Page.IsValid )
                {
                    return;
                }

                cvGroupMember.IsValid = groupMember.IsValid;

                if ( !cvGroupMember.IsValid )
                {
                    cvGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                // using WrapTransaction because there are two Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( groupMember.Id.Equals( 0 ) )
                    {
                        groupMemberService.Add( groupMember );
                    }

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                } );

                Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    Rock.Security.Role.Flush( group.Id );
                    Rock.Security.Authorization.Flush();
                }
            }

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["GroupId"] = hfGroupId.Value;
            NavigateToParentPage( qryString );
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
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript", @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });", true );
            
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
                }
            }

            if ( groupMember == null )
            {
                nbErrorMessage.Title = "Invalid Request";
                nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid GroupMemberId or GroupId parameter is required.";
                pnlEditDetails.Visible = false;
                return;
            }

            pnlEditDetails.Visible = true;

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

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
            }
            else
            {
                lReadOnlyTitle.Text = groupMember.Person.FullName.FormatAsHtmlTitle();
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

            LoadDropDowns();

            ppGroupMemberPerson.SetValue( groupMember.Person );
            ppGroupMemberPerson.Enabled = !readOnly;

            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            ddlGroupRole.Enabled = !readOnly;

            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );
            rblStatus.Enabled = !readOnly;
            rblStatus.Label = string.Format( "{0} Status", group.GroupType.GroupMemberTerm );

            groupMember.LoadAttributes();
            phAttributes.Controls.Clear();

            Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, true, "", true );
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

        #endregion
    }
}