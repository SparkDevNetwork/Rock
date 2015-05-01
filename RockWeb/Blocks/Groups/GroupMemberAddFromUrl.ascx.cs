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
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Member Add From URL" )]
    [Category( "Groups" )]
    [Description( "Adds a person to a group based on inputs from the URL query string." )]
    [GroupField("Default Group", "The default group to use if one is not passed through the query string (optional).", false)]
    [GroupRoleField("", "Default Group Member Role", "The default role to use if one is not passed through the quert string (optional).", false)]
    [CodeEditorField("Success Message", "Lava template to display when person has been added to the group.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 300, true, @"<div class='alert alert-success'>
    {{ Person.NickName }} has been added to the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>")]
    [CodeEditorField( "Already In Group Message", "Lava template to display when person is already in the group with that role.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 300, true, @"<div class='alert alert-warning'>
    {{ Person.NickName }} is already in the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>" )]
    [BooleanField("Enable Debug", "Shows the Lava variables availabled for this block")]
    [EnumField("Group Member Status", "The status to use when adding a person to the group.", typeof(GroupMemberStatus), true, "Active")]
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                RockContext rockContext = new RockContext();
                
                Group group = null;
                Guid personGuid = Guid.Empty;
                GroupTypeRole groupMemberRole = null;
                
                // get group id from url
                if ( Request["GroupId"] != null )
                {
                    int groupId = 0;
                    if ( Int32.TryParse( Request["GroupId"], out groupId ) )
                    {
                        group = new GroupService( rockContext ).Queryable("GroupType,GroupType.Roles").Where(g => g.Id == groupId ).FirstOrDefault();
                    }
                }
                else
                {
                    Guid groupGuid = Guid.Empty;
                    if ( Guid.TryParse( GetAttributeValue( "DefaultGroup" ), out groupGuid ) ) {
                        group = new GroupService( rockContext ).Queryable( "GroupType,GroupType.Roles" ).Where( g => g.Guid == groupGuid ).FirstOrDefault(); ;
                    }
                }

                if ( group == null )
                {
                    lAlerts.Text = "Could not determine the group to add to.";
                    return;
                }

                // get group role id from url
                if ( Request["GroupMemberRoleId"] != null )
                {
                    int groupMemberRoleId = 0;
                    if ( Int32.TryParse( Request["GroupMemberRoleId"], out groupMemberRoleId ) )
                    {
                        groupMemberRole = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleId );
                    }
                }
                else
                {
                    Guid groupMemberRoleGuid = Guid.Empty;
                    if ( Guid.TryParse( GetAttributeValue( "DefaultGroupMemberRole" ), out groupMemberRoleGuid ) )
                    {
                        groupMemberRole = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleGuid );
                    }
                }

                if ( groupMemberRole == null )
                {
                    lAlerts.Text += "Could not determine the group role to use for the add.";
                    return;
                }

                // get person
                if ( Request["PersonGuid"] != null )
                {
                    Guid.TryParse( Request["PersonGuid"], out personGuid );
                }

                if ( personGuid == Guid.Empty )
                {
                    lAlerts.Text += "A valid person identifier was not found in the page address.";
                    return;
                }

                // ensure that the group type has this role
                if ( ! group.GroupType.Roles.Contains( groupMemberRole ) )
                {
                    lAlerts.Text += "The group you have provided does not have the group member role configured.";
                    return;
                }

                // get person
                Person person = new PersonService( rockContext ).Get( personGuid );

                if ( person == null )
                {
                    lAlerts.Text += "A person could not be found for the identifier provided.";
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

                // show debug info?
                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }

                // ensure that the person is not already in the group
                if ( group.Members.Where( m => m.PersonId == person.Id && m.GroupRoleId == groupMemberRole.Id ).Count() != 0 )
                {
                    string templateInGroup = GetAttributeValue( "AlreadyInGroupMessage" );
                    lContent.Text = templateInGroup.ResolveMergeFields( mergeFields );
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