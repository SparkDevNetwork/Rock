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
                    upDetail.Visible = false;
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
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ClearErrorMessage();

            if ( Page.IsValid )
            {
                GroupMemberService groupMemberService = new GroupMemberService();
                GroupMember groupMember;

                int groupMemberId = int.Parse( hfGroupMemberId.Value );

                GroupTypeRole role = new GroupTypeRoleService().Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );

                // if adding a new group member 
                if ( groupMemberId.Equals( 0 ) )
                {

                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = hfGroupId.ValueAsInt();

                    //check to see if the person is alread a member of the gorup/role
                    var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                        hfGroupId.ValueAsInt(), ppGroupMemberPerson.SelectedValue ?? 0, ddlGroupRole.SelectedValueAsId() ?? 0 );

                    if ( existingGroupMember != null )
                    {
                        //if so, don't add and show error message
                        var person = new PersonService().Get( (int)ppGroupMemberPerson.PersonId );

                        nbErrorMessage.Title = "Person already added";
                        nbErrorMessage.Text = string.Format( "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role. <a href=\"/page/{3}?groupMemberId={4}\">Click here</a> to view existing membership.",
                            person.FullName,
                            ddlGroupRole.SelectedItem.Text,
                            role.GroupType.GroupTerm,
                            CurrentPage.Id,
                            existingGroupMember.Id
                            );
                        return;

                    }
                }
                else
                {
                    //load existing group member
                    groupMember = groupMemberService.Get( groupMemberId );
                }

                int memberCountInRole = new GroupMemberService().Queryable()
                    .Where( m =>
                        m.GroupId == groupMember.GroupId &&
                        m.GroupRoleId == role.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Count();

                bool roleMembershipAboveMax = false;
                string action = "add";

                //if adding new active group member
                if ( groupMemberId.Equals( 0 ) && rblStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
                {
                    // verify that active count has not exceeded the max
                    if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }

                //if existing group member changing role or status
                else if ( groupMemberId > 0 && ( groupMember.GroupRoleId != role.Id || groupMember.GroupMemberStatus != rblStatus.SelectedValueAsEnum<GroupMemberStatus>() )
                        && rblStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
                {
                    action = "change";

                    // verify that active count has not exceeded the max
                    if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }

                //show error if above max.. do not proceed
                if ( roleMembershipAboveMax )
                {
                    var person = new PersonService().Get( (int)ppGroupMemberPerson.PersonId );

                    nbErrorMessage.Title = string.Format("Maximum {0} Exceeded", role.Name.Pluralize() );
                    nbErrorMessage.Text = string.Format( "<br />The number of {0} for this {1} is at or above its maximum allowed limit of {2:N0} active {3}.", 
                        role.Name.Pluralize(), role.GroupType.GroupTerm, role.MaxCount,
                        role.MaxCount == 1 ? role.GroupType.GroupMemberTerm : role.GroupType.GroupMemberTerm.Pluralize() );

                    return;
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

                Group group = new GroupService().Get( groupMember.GroupId );
                if ( group.IsSecurityRole )
                {
                    // new person added to SecurityRole, Flush
                    Rock.Security.Authorization.Flush();
                }
            }

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["groupId"] = hfGroupId.Value;
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
                qryString["groupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupMemberService groupMemberService = new GroupMemberService();
                GroupMember groupMember = groupMemberService.Get( int.Parse( hfGroupMemberId.Value ) );

                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["groupId"] = groupMember.GroupId.ToString();
                NavigateToParentPage( qryString );
            }
        }

        #endregion

        #region Internal Methods

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
                if ( groupId.HasValue )
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

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

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
            lGroupIconHtml.Text = groupIconHtml;

            if ( groupMember.Id.Equals( 0 ) )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( groupMember.Group.GroupType.GroupTerm + " " + groupMember.Group.GroupType.GroupMemberTerm ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = groupMember.Person.FullName.FormatAsHtmlTitle();
            } 
            
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

            btnSave.Visible = !readOnly;

            LoadDropDowns();

            ppGroupMemberPerson.SetValue(groupMember.Person);
            ppGroupMemberPerson.Enabled = !readOnly;

            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            ddlGroupRole.Enabled = !readOnly;

            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );
            rblStatus.Enabled = !readOnly;

            groupMember.LoadAttributes();
            phAttributes.Controls.Clear();

            Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, true );
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
            nbErrorMessage.Title = String.Empty;
            nbErrorMessage.Text = String.Empty;
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
                ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                ddlGroupRole.DataBind();
            }

            rblStatus.BindToEnum( typeof( GroupMemberStatus ) );
        }

        #endregion
    }
}