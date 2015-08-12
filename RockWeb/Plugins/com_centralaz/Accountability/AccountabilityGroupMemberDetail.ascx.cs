using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.centralaz.Accountability.Data;
using com.centralaz.Accountability.Model;

using Rock;
using Rock.Web;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    [DisplayName( "Accountability Group Member Detail" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Shows the detail for a group Member" )]
    [BooleanField( "Able to choose leader" )]

    public partial class AccountabilityGroupMemberDetail : Rock.Web.UI.RockBlock
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
            int theThing = PageParameter( "GroupId" ).AsInteger();
            if ( IsPersonMember( PageParameter( "GroupId" ).AsInteger() ) || IsUserAuthorized( Authorization.EDIT ) )
            {
                if ( !Page.IsPostBack )
                {
                    ShowDetail( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
                }

                base.OnLoad( e );
            }
            else
            {
                RockPage.Layout.Site.RedirectToPageNotFoundPage();
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
        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }
        #endregion

        /// <summary>
        /// Handles the OnClick event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
        }

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

                //check for valid start date
                Group group = new GroupService( new RockContext() ).Get( hfGroupId.ValueAsInt() );
                group.LoadAttributes();
                DateTime startDate = DateTime.Parse( group.GetAttributeValue( "ReportStartDate" ) );
                DateTime memberDate = dpMemberStartDate.SelectedDate.Value;
                if ( startDate > memberDate )
                {
                    nbErrorMessage.Title = String.Format( "Please select a start date after {0}", startDate.ToShortDateString() );
                    return;
                }
                int daysOffOfWeek = ( memberDate - startDate ).Days % 7;
                if ( daysOffOfWeek != 0 )
                {
                    nbErrorMessage.Title = String.Format( "Please select a {0}", startDate.DayOfWeek.ToString() );
                    return;
                }


                // if adding a new group member 
                if ( groupMemberId.Equals( 0 ) )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = hfGroupId.ValueAsInt();

                    // check to see if the person is alread a member of the gorup/role
                    var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                        hfGroupId.ValueAsInt(), ppGroupMemberPerson.SelectedValue ?? 0, ddlGroupRole.SelectedValueAsId() ?? 0 );

                    if ( existingGroupMember != null )
                    {
                        // if so, don't add and show error message
                        var person = new PersonService( rockContext ).Get( (int)ppGroupMemberPerson.PersonId );

                        nbErrorMessage.Title = "Person already added";
                        nbErrorMessage.Text = string.Format(
                            "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role. <a href=\"/page/{3}?groupMemberId={4}\">Click here</a> to view existing membership.",
                            person.FullName,
                            ddlGroupRole.SelectedItem.Text,
                            role.GroupType.GroupTerm,
                            RockPage.PageId,
                            existingGroupMember.Id );
                        return;
                    }
                }
                else
                {
                    // load existing group member
                    groupMember = groupMemberService.Get( groupMemberId );
                }

                int memberCountInRole = new GroupMemberService( rockContext ).Queryable()
                    .Where( m =>
                        m.GroupId == groupMember.GroupId &&
                        m.GroupRoleId == role.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Count();

                bool roleMembershipAboveMax = false;

                // if adding new active group member..
                if ( groupMemberId.Equals( 0 ) && rblEditStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
                {
                    // verify that active count has not exceeded the max
                    if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }
                else if ( groupMemberId > 0 && ( groupMember.GroupRoleId != role.Id || groupMember.GroupMemberStatus != rblEditStatus.SelectedValueAsEnum<GroupMemberStatus>() )
                        && rblEditStatus.SelectedValueAsEnum<GroupMemberStatus>() == GroupMemberStatus.Active )
                {
                    // if existing group member changing role or status..
                    // verify that active count has not exceeded the max
                    if ( role.MaxCount != null && ( memberCountInRole + 1 ) > role.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }

                // show error if above max.. do not proceed
                if ( roleMembershipAboveMax )
                {
                    var person = new PersonService( rockContext ).Get( (int)ppGroupMemberPerson.PersonId );

                    nbErrorMessage.Title = string.Format( "Maximum {0} Exceeded", role.Name.Pluralize() );
                    nbErrorMessage.Text = string.Format(
                        "<br />The number of {0} for this {1} is at or above its maximum allowed limit of {2:N0} active {3}.",
                        role.Name.Pluralize(),
                        role.GroupType.GroupTerm,
                        role.MaxCount,
                        role.MaxCount == 1 ? role.GroupType.GroupMemberTerm : role.GroupType.GroupMemberTerm.Pluralize() );

                    return;
                }

                groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
                groupMember.GroupRoleId = role.Id;
                groupMember.GroupMemberStatus = rblEditStatus.SelectedValueAsEnum<GroupMemberStatus>();
                if ( groupMember.GroupRoleId == 0 )
                {
                    groupMember.Group.IsAuthorized( "Edit", groupMember.Person );
                }

                groupMember.LoadAttributes();

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !groupMember.IsValid )
                {
                    return;
                }


                if ( groupMember.Id.Equals( 0 ) )
                {
                    groupMemberService.Add( groupMember );
                }

                rockContext.SaveChanges();
                groupMember.SetAttributeValue( "MemberStartDate", dpMemberStartDate.SelectedDate.Value.ToShortDateString() );
                groupMember.SaveAttributeValues( rockContext );
                
                group = new GroupService( rockContext ).Get( groupMember.GroupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    Rock.Security.Role.Flush( group.Id );
                    Rock.Security.Authorization.Flush();
                }
            }
            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["GroupId"] = hfGroupId.Value;
            qryString["GroupMemberId"] = hfGroupMemberId.Value;
            if ( int.Parse( hfGroupMemberId.Value ) == 0 )
            {
                NavigateToParentPage( qryString );
            }
            else
            {
                ShowReadonlyDetail( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
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
                qryString["GroupId"] = hfGroupId.Value;
                qryString["GroupMemberId"] = hfGroupMemberId.Value;
                ShowReadonlyDetail( PageParameter( "GroupMemberId" ).AsInteger(), PageParameter( "GroupId" ).AsIntegerOrNull() );
            }
        }

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
        /// Shows the detail
        /// </summary>
        /// <param name="groupMemberId">The group Member id</param>
        /// <param name="groupId">the Group ID</param>
        public void ShowDetail( int groupMemberId, int? groupId )
        {
            GroupMember groupMember = null;

            bool editAllowed = true;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = GetGroupMember( groupMemberId );
                if ( groupMember != null )
                {
                    editAllowed = groupMember.IsAuthorized( Authorization.EDIT, CurrentPerson ) || IsPersonLeader( groupMember.GroupId );
                }
            }

            if ( groupMember == null )
            {
                groupMember = new GroupMember { Id = 0 };

            }

            pnlContent.Visible = true;

            hfGroupMemberId.Value = groupMember.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed && !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( readOnly )
            {
                pnlEditDetails.Visible = false;
                ShowReadonlyDetail( groupMemberId, groupId );
            }
            else
            {
                lbEdit.Visible = true;
                if ( groupMember.Id > 0 )
                {
                    ShowReadonlyDetail( groupMemberId, groupId );
                }
                else
                {
                    ShowEditDetails( groupMemberId, groupId );
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="groupId">The group id.</param>
        public void ShowReadonlyDetail( int groupMemberId, int? groupId )
        {
            SetEditMode( false );
            var rockContext = new RockContext();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                }
            }

            if ( groupMember == null )
            {
                nbErrorMessage.Title = "Invalid Request";
                nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid GroupMemberId or GroupId parameter is required.";
                pnlViewDetails.Visible = false;
                return;
            }

            pnlViewDetails.Visible = true;

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            //Load name and image
            Person person = groupMember.Person;
            if ( person != null && person.Id != 0 )
            {
                if ( person.NickName == person.FirstName )
                {
                    lName.Text = person.FullName.FormatAsHtmlTitle();
                }
                else
                {
                    lName.Text = String.Format( "{0} {2} <span class='full-name'>({1})</span>", person.NickName.FormatAsHtmlTitle(), person.FirstName, person.LastName );
                }

                // Setup Image
                string imgTag = Rock.Model.Person.GetPhotoImageTag( person.PhotoId, person.Age, person.Gender, 120, 120 );
                if ( person.PhotoId.HasValue )
                {
                    lImage.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag );
                }
                else
                {
                    lImage.Text = imgTag;
                }
            }

            // render UI based on Authorized and IsSystem
            var group = groupMember.Group;
            rblStatus.BindToEnum<GroupMemberStatus>();
            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );
            rblStatus.Enabled = false;
            rblStatus.Label = string.Format( "Status" );

            groupMember.LoadAttributes();
            lblStartDate.Text = groupMember.GetAttributeValue( "MemberStartDate" );

            //Determines visibility of the edit button
            bool readOnly = false;
            if ( ( !IsUserAuthorized( Authorization.EDIT ) || !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ) && !IsPersonLeader( group.Id ) )
            {
                readOnly = true;
            }

            if ( groupMember.IsSystem )
            {
                readOnly = true;
            }
            if ( readOnly )
            {
                lbEdit.Visible = false;
            }
            else
            {
                lbEdit.Visible = true;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="groupId">The group id.</param>
        public void ShowEditDetails( int groupMemberId, int? groupId )
        {
            SetEditMode( true );
            var rockContext = new RockContext();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                }
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


            // user has to have EDIT Auth to both the Block and the group
            nbEditModeMessage.Text = string.Empty;
            if ( ( !IsUserAuthorized( Authorization.EDIT ) || !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ) && !IsPersonLeader( group.Id ) )
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

            rblEditStatus.SetValue( (int)groupMember.GroupMemberStatus );
            rblEditStatus.Enabled = !readOnly;
            rblEditStatus.Label = string.Format( "{0} Status", group.GroupType.GroupMemberTerm );
            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember.LoadAttributes();
                dpMemberStartDate.SelectedDate = DateTime.Parse( groupMember.GetAttributeValue( "MemberStartDate" ) );
            }
            else
            {
                Group newMemberGroup = new GroupService( new RockContext() ).Get( groupId.Value );
                newMemberGroup.LoadAttributes();
                dpMemberStartDate.SelectedDate = DateTime.Parse( newMemberGroup.GetAttributeValue( "ReportStartDate" ) );
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

        private GroupMember GetGroupMember( int groupMemberId )
        {
            string key = string.Format( "GroupMember:{0}", groupMemberId );
            GroupMember groupMember = RockPage.GetSharedItem( key ) as GroupMember;
            if ( groupMember == null )
            {
                groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId );
                RockPage.SaveSharedItem( key, groupMember );
            }

            return groupMember;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            int groupId = hfGroupId.ValueAsInt();
            Group group = new GroupService( new RockContext() ).Get( groupId );
            bool isAbleToSelectLeader = bool.Parse( GetAttributeValue( "Abletochooseleader" ) );
            if ( group != null )
            {
                if ( isAbleToSelectLeader )
                {
                    ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                }
                else
                {
                    ddlGroupRole.DataSource = group.GroupType.Roles.Where( a => a.Name != "Leader" ).OrderBy( a => a.Order ).ToList();
                }
                ddlGroupRole.DataBind();
            }

            rblStatus.BindToEnum<GroupMemberStatus>();
            rblEditStatus.BindToEnum<GroupMemberStatus>();
        }

        /// <summary>
        /// Returns true if the current person is a group leader.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a leader, false if not.</returns>
        protected bool IsPersonLeader( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId &&
                    m.GroupRole.Name == "Leader"
                    )
                 .Count();
            if ( count == 1 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the current person is a group member.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a member, false if not.</returns>
        protected bool IsPersonMember( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId
                    )
                 .Count();
            if ( count == 1 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
}