//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string groupId = PageParameter( "groupId" );
                string groupMemberId = PageParameter( "groupMemberId" );
                if ( !string.IsNullOrWhiteSpace( groupMemberId ) )
                {
                    if ( string.IsNullOrWhiteSpace( groupId ) )
                    {
                        ShowDetail( "groupMemberId", int.Parse( groupMemberId ) );
                    }
                    else
                    {
                        ShowDetail( "groupMemberId", int.Parse( groupMemberId ), int.Parse( groupId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            var groupMember = new GroupMember { GroupId = hfGroupId.ValueAsInt() };
            groupMember.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, false );
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

            int? groupMemberId = PageParameter(pageReference, "groupMemberId" ).AsInteger();
            if ( groupMemberId != null )
            {
                GroupMember groupMember = new GroupMemberService().Get( groupMemberId.Value );
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
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = String.Empty;
            nbErrorMessage.Text = String.Empty;
        }

        /// <summary>
        /// Finds an exsiting member of a group by groupId, roleId and personId
        /// if a existing member is found it returns the groupMemberId
        /// </summary>
        /// <returns>nullable groupMemberId </returns>
        private int? FindExistingGroupMemberRole()
        {
            int groupId = int.Parse( hfGroupId.Value );
            int personId = 0;
            int roleId = int.Parse( ddlGroupRole.SelectedValue );

            if ( ppGroupMemberPerson.PersonId != null )
            {
                personId = (int)ppGroupMemberPerson.PersonId;
            }

            var service = new GroupMemberService();

            return service.Queryable()
                    .Where( m => m.GroupId == groupId )
                    .Where( m => m.PersonId == personId )
                    .Where( m => m.GroupRoleId == roleId )
                    .Select( m => m.Id ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the number of active group members who are in the selected role
        /// </summary>
        /// <returns>Group Member Count</returns>
        private int GetGroupMembersInRoleCount()
        {
            int groupId = hfGroupId.Value.AsInteger() ?? 0;
            int roleId = ddlGroupRole.SelectedValueAsInt() ?? 0;

            return new GroupMemberService().Queryable()
                        .Where( m => m.GroupId == groupId )
                        .Where( m => m.GroupRoleId == roleId )
                        .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Count();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            int groupId = hfGroupId.ValueAsInt();
            Group group = new GroupService().Get( groupId );
            if ( group != null )
            {
                ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.SortOrder ).ToList();
                ddlGroupRole.DataBind();
            }

            ddlGroupMemberStatus.BindToEnum( typeof( GroupMemberStatus ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="groupId">The group id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? groupId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "groupMemberId" ) )
            {
                return;
            }

            GroupMember groupMember = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                groupMember = new GroupMemberService().Get( itemKeyValue );
                groupMember.LoadAttributes();
            }
            else
            {
                // only create a new one if parent was specified
                if ( groupId != null )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = groupId.Value;
                    groupMember.Group = new GroupService().Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                }
            }

            if ( groupMember == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( groupMember.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( groupMember );
            }
            else
            {
                btnEdit.Visible = true;
                if ( groupMember.Id > 0 )
                {
                    ShowReadonlyDetails( groupMember );
                }
                else
                {
                    ShowEditDetails( groupMember );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        private void ShowEditDetails( GroupMember groupMember )
        {
            if ( groupMember.Id.Equals( 0 ) )
            {
                lActionTitle.Text = ActionTitle.Add( "Group Member to " + groupMember.Group.Name );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( "Group Member for " + groupMember.Group.Name );
            }

            SetEditMode( true );

            LoadDropDowns();

            ppGroupMemberPerson.SetValue(groupMember.Person);
            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            ddlGroupMemberStatus.SetValue( (int) groupMember.GroupMemberStatus );

            phAttributes.Controls.Clear();
            groupMember.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        private void ShowReadonlyDetails( GroupMember groupMember )
        {
            SetEditMode( false );
            
            ClearErrorMessage();

            string groupIconHtml = string.Empty;
            var group = groupMember.Group;
            if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
            {
                groupIconHtml = string.Format( "<i class='{0} icon-large' ></i>", group.GroupType.IconCssClass );
            }
            else
            {
                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                string imageUrlFormat = "<img src='" + appPath + "GetImage.ashx?id={0}&width=50&height=50' />";
                if ( group.GroupType.IconLargeFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconLargeFileId );
                }
                else if ( group.GroupType.IconSmallFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconSmallFileId );
                }
            }

            hfGroupId.SetValue( group.Id );
            hfGroupMemberId.SetValue( groupMember.Id );

            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = groupMember.Person.FullName;

            lblMainDetails.Text = new DescriptionList()
                .Add("Group Member", groupMember.Person)
                .Add("Member's Role", groupMember.GroupRole.Name)
                .Add( "Member's Status", groupMember.GroupMemberStatus.ConvertToString() )
                .Add("Group Name", group.Name)
                .Add("Group Description", group.Description)
                
                .Html;

            groupMember.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( groupMember, phGroupMemberAttributesReadOnly );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            GroupMemberService groupMemberService = new GroupMemberService();
            GroupMember groupMember = groupMemberService.Get( int.Parse( hfGroupMemberId.Value ) );
            ShowEditDetails( groupMember );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.DimOtherBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ClearErrorMessage();

            int groupMemberId = int.Parse( hfGroupMemberId.Value );
            GroupRole role = new GroupRoleService().Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );
            int memberCountInRole = GetGroupMembersInRoleCount();
            bool roleMembershipBelowMinimum = false;
            bool roleMembershipAboveMax = false;

            GroupMemberService groupMemberService = new GroupMemberService();


            GroupMember groupMember;

            // if adding a new group member 
            if ( groupMemberId.Equals( 0 ) )
            {

                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = hfGroupId.ValueAsInt();

                //check to see if the person is a member fo the gorup/role
                int? existingGroupMemberId = FindExistingGroupMemberRole();

                //if so, don't add and show error message
                if ( existingGroupMemberId != null && existingGroupMemberId > 0 )
                {
                    var person = new PersonService().Get( (int)ppGroupMemberPerson.PersonId );

                    nbErrorMessage.Title = string.Format( "Can not add {0} to {1} role.", person.FullName, role.Name );
                    nbErrorMessage.Text = string.Format("<br /> {0} is already a member of the {1} role for this group, and can not be added. <a href=\"/page/{2}?groupMemberId={3}\">Click here</a> to view existing membership.", 
                        person.FullName, 
                        ddlGroupRole.SelectedItem.Text,
                        CurrentPage.Id,
                        existingGroupMemberId
                        );
                    return;

                }
            }
            else
            {
                //load existing group member
                groupMember = groupMemberService.Get( groupMemberId );

            }

            //if adding new active group member
            if ( groupMemberId.Equals( 0 ) && ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
            {
                // verify that active count has not exceeded the max
                if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                {
                    roleMembershipAboveMax = true;
                }
                //check that min count has been reached
                if ( role.MinCount != null && ( memberCountInRole + 1 ) < role.MinCount )
                {
                    roleMembershipBelowMinimum = true;
                }
            }
            //if existing group member changing role or status
            else if ( groupMemberId > 0 && ( groupMember.GroupRoleId != role.Id || groupMember.GroupMemberStatus != ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>() )
                    && ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
            {
                // verify that active count has not exceeded the max
                if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                {
                    roleMembershipAboveMax = true;
                }
                //check that min count has been reached
                if ( role.MinCount != null && ( memberCountInRole + 1 ) < role.MinCount )
                {
                    roleMembershipBelowMinimum = true;
                }
            }
            // if existing member is going inactive
            else if ( groupMemberId > 0 && groupMember.GroupMemberStatus == GroupMemberStatus.Active && ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>() != GroupMemberStatus.Active )
            {
                //check that min count has been reached
                if ( role.MinCount != null && ( memberCountInRole - 1 ) < role.MinCount )
                {
                    roleMembershipBelowMinimum = true;
                }
            }
            else
            {
                //check that min count has been reached
                if ( role.MinCount != null && memberCountInRole < role.MinCount )
                {
                    roleMembershipBelowMinimum = true;
                }
            }

            //show error if above max.. do not proceed
            if ( roleMembershipAboveMax )
            {
                var person = new PersonService().Get( (int)ppGroupMemberPerson.PersonId );

                nbErrorMessage.Title = string.Format( "Can not add {0} to {1} role.", person.FullName, role.Name );
                nbErrorMessage.Text = string.Format( "<br /> {0} role is at it's maximum capacity of {1} active members.", role.Name, role.MaxCount.ToString() );
                return;
            }

            groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();

            groupMember.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !groupMember.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( groupMember.Id.Equals( 0 ) )
                {
                    groupMemberService.Add( groupMember, CurrentPersonId );
                }

                groupMemberService.Save( groupMember, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( groupMember, CurrentPersonId );
            } );

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["groupId"] = hfGroupId.Value;

            if ( roleMembershipBelowMinimum )
            {
                qryString["roleBelowMin"] = roleMembershipBelowMinimum.ToString();
                qryString["roleId"] = role.Id.ToString();
            }

            Group group = new GroupService().Get( groupMember.GroupId );
            if ( group.IsSecurityRole )
            {
                // new person added to SecurityRole, Flush
                Rock.Security.Authorization.Flush();
            }

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
                // Cancelling on Add.  Return to Grid
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["groupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupMemberService groupMemberService = new GroupMemberService();
                GroupMember groupMember = groupMemberService.Get( int.Parse( hfGroupMemberId.Value ) );
                ShowReadonlyDetails( groupMember );
            }
        }

        #endregion
    }
}