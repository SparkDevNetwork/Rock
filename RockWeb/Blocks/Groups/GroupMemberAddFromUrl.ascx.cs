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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Member Add From URL" )]
    [Category( "Groups" )]
    [Description( "Adds a person to a group based on inputs from the URL query string." )]

    [GroupField("Default Group", "The default group to use if one is not passed through the query string (optional).", false)]
    [GroupRoleField("", "Default Group Member Role", "The default role to use if one is not passed through the query string (optional).", false)]

    [CodeEditorField( "Success Message",
        Description = "Lava template to display when person has been added to the group.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-success'>
    {{ Person.NickName }} has been added to the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>" )]

    [CodeEditorField( "Already In Group Message",
        Description = "Lava template to display when person is already in the group with that role.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-warning'>
    {{ Person.NickName }} is already in the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>" )]

    [EnumField("Group Member Status", "The status to use when adding a person to the group.", typeof(GroupMemberStatus), true, "Active")]
    [GroupTypesField( "Limit Group Type", "The group types that people can be added to from a link. Required: when blank, the block adds no one. Security role groups are not supported.", true )]
    [BooleanField( "Enable Passing Group Id", "If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.", true, "" )]

    [Rock.SystemGuid.BlockTypeGuid( "42CF3822-A70C-4E07-9394-21607EED7018" )]
    public partial class GroupMemberAddFromUrl : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                RockContext rockContext = new RockContext();
                var groupService = new GroupService( rockContext );

                /*
                    10/19/2026 - MSE

                    Limit Group Type is required. The block adds no one until an administrator
                    selects the group types it may add people to, and it never accepts the
                    Security Role group type. These checks run before any value from the URL
                    is read so a misconfigured block makes no changes.

                    Reason: Only add people to group types an administrator has allowed.
                */
                var allowedGroupTypeGuids = GetAttributeValue( "LimitGroupType" ).SplitDelimitedValues().AsGuidList();
                if ( !allowedGroupTypeGuids.Any() )
                {
                    lAlerts.Text = "This block is not configured. An administrator must select one or more group types in the Limit Group Type setting.";
                    base.OnLoad( e );
                    return;
                }

                if ( allowedGroupTypeGuids.Contains( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    lAlerts.Text = "Security role groups are not supported by this block. An administrator must remove the Security Role group type from the Limit Group Type setting.";
                    base.OnLoad( e );
                    return;
                }

                Group group = null;
                Guid personGuid = Guid.Empty;
                GroupTypeRole groupMemberRole = null;

                // get group id from url
                Guid? groupGuid = PageParameter( "GroupGuid" ).AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    group = groupService.Queryable( "GroupType,GroupType.Roles" ).Where( g => g.Guid == groupGuid.Value ).FirstOrDefault();
                }

                if ( group == null && GetAttributeValue( "EnablePassingGroupId" ).AsBoolean( true ) )
                {
                    int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                    if ( groupId.HasValue )
                    {
                        group = groupService.Queryable( "GroupType,GroupType.Roles" ).Where( g => g.Id == groupId ).FirstOrDefault();
                    }
                }

                if ( group == null )
                {
                    groupGuid = GetAttributeValue( "DefaultGroup" ).AsGuidOrNull();
                    if ( groupGuid.HasValue )
                    {
                        group = groupService.Queryable( "GroupType,GroupType.Roles" ).Where( g => g.Guid == groupGuid.Value ).FirstOrDefault();
                    }
                }

                if ( group == null )
                {
                    lAlerts.Text = "Could not determine the group to add to.";
                    base.OnLoad( e );
                    return;
                }

                /*
                    10/19/2026 - MSE

                    The group, whether it came from the URL or the Default Group setting,
                    must be of one of the allowed group types, and it must never be a
                    security role group, whatever the block settings say.

                    Reason: Only add people to allowed groups, never to security roles.
                */
                if ( !allowedGroupTypeGuids.Contains( group.GroupType.Guid ) )
                {
                    lAlerts.Text = "Invalid group specified.";
                    base.OnLoad( e );
                    return;
                }

                if ( group.IsSecurityRoleOrSecurityGroupType() )
                {
                    lAlerts.Text = "Invalid group specified.";
                    base.OnLoad( e );
                    return;
                }

                // get group role id from url
                if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupMemberRoleId" ) ) )
                {
                    int groupMemberRoleId = 0;
                    if ( Int32.TryParse( PageParameter( "GroupMemberRoleId" ), out groupMemberRoleId ) )
                    {
                        groupMemberRole = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleId );
                    }
                }
                else if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DefaultGroupMemberRole" ) ) )
                {
                    Guid groupMemberRoleGuid = Guid.Empty;
                    if ( Guid.TryParse( GetAttributeValue( "DefaultGroupMemberRole" ), out groupMemberRoleGuid ) )
                    {
                        groupMemberRole = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleGuid );
                    }
                }
                else
                {
                    groupMemberRole = group.GroupType.DefaultGroupRole;
                }

                if ( groupMemberRole == null )
                {
                    lAlerts.Text += "Could not determine the group role to use for the add.";
                    base.OnLoad( e );
                    return;
                }

                // get person
                if ( !string.IsNullOrWhiteSpace( PageParameter( "PersonGuid" ) ) )
                {
                    Guid.TryParse( PageParameter( "PersonGuid" ), out personGuid );
                }

                if ( personGuid == Guid.Empty )
                {
                    lAlerts.Text += "A valid person identifier was not found in the page address.";
                    base.OnLoad( e );
                    return;
                }

                // ensure that the group type has this role
                if ( ! group.GroupType.Roles.Contains( groupMemberRole ) )
                {
                    lAlerts.Text += "The group you have provided does not have the group member role configured.";
                    base.OnLoad( e );
                    return;
                }

                // get person
                Person person = new PersonService( rockContext ).Get( personGuid );

                if ( person == null )
                {
                    lAlerts.Text += "A person could not be found for the identifier provided.";
                    base.OnLoad( e );
                    return;
                }

                // hide alert
                divAlert.Visible = false;

                // get status
                var groupMemberStatus = this.GetAttributeValue( "GroupMemberStatus" ).ConvertToEnum<GroupMemberStatus>( GroupMemberStatus.Active );

                // load merge fields
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "GroupMemberStatus", groupMemberStatus.ToString() );
                mergeFields.Add( "Group", group );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "Role", groupMemberRole );
                mergeFields.Add( "CurrentPerson", CurrentPerson );

                // ensure that the person is not already in the group
                if ( group.Members.Where( m => m.PersonId == person.Id && m.GroupRoleId == groupMemberRole.Id ).Count() != 0 )
                {
                    string templateInGroup = GetAttributeValue( "AlreadyInGroupMessage" );
                    lContent.Text = templateInGroup.ResolveMergeFields( mergeFields );
                    base.OnLoad( e );
                    return;

                }
                
                // add person to group
                GroupMember groupMember = new GroupMember();
                groupMember.GroupId = group.Id;
                groupMember.PersonId = person.Id;
                groupMember.GroupRoleId = groupMemberRole.Id;
                groupMember.GroupMemberStatus = groupMemberStatus;
                group.Members.Add( groupMember );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    divAlert.Visible = true;
                    lAlerts.Text = String.Format( "An error occurred adding {0} to the group {1}. Message: {2}.", person.FullName, group.Name, ex.Message );
                }
                
                string templateSuccess = GetAttributeValue( "SuccessMessage" );
                lContent.Text = templateSuccess.ResolveMergeFields( mergeFields );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}