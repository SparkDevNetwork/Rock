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
    [DisplayName( "Group Member RemoveFrom URL" )]
    [Category( "Groups" )]
    [Description( "Removes a person from a group based on inputs from the URL query string (GroupId, PersonGuid)." )]
    [GroupField("Default Group", "The default group to use if one is not passed through the query string (optional).", false)]
    [CodeEditorField("Success Message", "Lava template to display when person has been added to the group.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, true, @"<div class='alert alert-success'>
    {{ Person.NickName }} has been removed from the group '{{ Group.Name }}'.
</div>")]
    [CodeEditorField( "Not In Group Message", "Lava template to display when person is not in the group.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, true, @"<div class='alert alert-warning'>
    {{ Person.NickName }} was not in the group '{{ Group.Name }}'.
</div>" )]
    [BooleanField("Warn When Not In Group", "Determines if the 'Not In Group Message'should be shown if the person is not in the group. Otherwise the success message will be shown", true)]
    [BooleanField("Enable Debug", "Shows the Lava variables availabled for this block")]
    public partial class GroupMemberRemoveFromUrl : Rock.Web.UI.RockBlock
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

                // get group id from url
                if ( Request["GroupId"] != null )
                {
                    int groupId = 0;
                    if ( Int32.TryParse( Request["GroupId"], out groupId ) )
                    {
                        group = new GroupService( rockContext ).Queryable().Where(g => g.Id == groupId ).FirstOrDefault();
                    }
                }
                else
                {
                    Guid groupGuid = Guid.Empty;
                    if ( Guid.TryParse( GetAttributeValue( "DefaultGroup" ), out groupGuid ) ) {
                        group = new GroupService( rockContext ).Queryable().Where( g => g.Guid == groupGuid ).FirstOrDefault(); ;
                    }
                }

                if ( group == null )
                {
                    lAlerts.Text = "Could not determine the group to add to.";
                    return;
                }

                // get person
                Person person = null;

                if ( !string.IsNullOrWhiteSpace(Request["PersonGuid"]) )
                {
                    person = new PersonService( rockContext ).Get( Request["PersonGuid"].AsGuid() );
                }

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
                mergeFields.Add( "Group", group );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "CurrentPerson", CurrentPerson );

                // show debug info?
                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }

                var groupMemberService = new GroupMemberService(rockContext);

                var groupMemberList = groupMemberService.Queryable()
                                        .Where( m => m.GroupId == group.Id && m.PersonId == person.Id )
                                        .ToList();

                if (groupMemberList.Count > 0 )
                {
                    foreach(var groupMember in groupMemberList )
                    {
                        groupMemberService.Delete( groupMember );
                        rockContext.SaveChanges();
                    }

                    lContent.Text = GetAttributeValue( "SuccessMessage" ).ResolveMergeFields( mergeFields );
                }
                else
                {
                    if ( GetAttributeValue( "WarnWhenNotInGroup").AsBoolean() )
                    {
                        lContent.Text = GetAttributeValue( "NotInGroupMessage" ).ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        lContent.Text = GetAttributeValue( "SuccessMessage" ).ResolveMergeFields( mergeFields );
                    }
                }
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