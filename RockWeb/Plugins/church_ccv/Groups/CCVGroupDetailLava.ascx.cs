// <copyright>
// Copyright by the Spark Development Network
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Utility.Groups;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "CCV Group Detail Lava" )]
    [Category( "CCV > Groups" )]
    [Description( "Presents the details of a CCV group using Lava" )]
    public partial class CCVGroupDetailLava : ToolboxGroupDetailLava
    {
        public override UpdatePanel MainPanel { get { return upnlContent; } }
        public override PlaceHolder AttributesPlaceholder { get { return phAttributes; } }
        public override Literal MainViewContent { get { return lContent; } }
        public override Literal DebugContent { get { return lDebug; } }
               
        public override Panel GroupEdit { get { return pnlGroupEdit; } }
        public override Panel GroupView { get { return pnlGroupView; } }
               
        public override Panel Schedule { get { return pnlSchedule; } }
        public override DayOfWeekPicker DayOfWeekPicker { get { return dowWeekly; } }
        public override TimePicker MeetingTime { get { return timeWeekly; } }
               
        public override Literal GroupName { get { return tbName; } }
        //public override TextBox GroupDesc { get { return tbDescription; } }
        //public override CheckBox IsActive { get { return cbIsActive; } }

        const int CCV_GroupRole_Attendee = 49;
        
        protected override void FinalizePresentView( Dictionary<string, object> mergeFields, bool enableDebug )
        {
            string template = GetAttributeValue( "LavaTemplate" );

            // show debug info
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                string postbackCommands = @"<h5>Available Postback Commands</h5>
                                            <ul>
                                                <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                <li><strong>AddGroupMember:</strong> Shows a panel for adding group info. Does not require input. <code>{{ '' | Postback:'AddGroupMember' }}</code></li>
                                                <li><strong>SendCommunication:</strong> Sends a communication to all group members on behalf of the Current User. This will redirect them to the communication page where they can author their email. <code>{{ '' | Postback:'SendCommunication' }}</code></li>
                                            </ul>";

                DebugContent.Visible = true;
                DebugContent.Text = mergeFields.lavaDebugInfo( null, string.Empty, postbackCommands );
            }

            MainViewContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( MainPanel.ClientID );
        }

        protected override void HandlePageAction( string action, string parameters )
        {
            switch ( action )
            {
                case "AddGroupMember":    DisplayAddGroupMember( ); break;
                case "EditGroup":         DisplayEditGroup( );      break;
                
                case "SendCommunication":
                {
                    var personAliasIds = new GroupMemberService( new RockContext( ) ).Queryable()
                                    .Where( m => m.GroupId == _groupId && m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                    .ToList()
                                    .Select( m => m.Person.PrimaryAliasId )
                                    .ToList();

                    SendCommunication( personAliasIds );

                    break;
                }
            }
        }

         protected void DisplayAddGroupMember( )
        {
            // reset any error message
            nbGroupMemberErrorMessage.Title = string.Empty;
            nbGroupMemberErrorMessage.Text = string.Empty;

            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = false;
            pnlEditGroupMember.Visible = true;
            
            // populate the member statuses, but remove InActive
            rblStatus.BindToEnum<GroupMemberStatus>();
            var inactiveItem = rblStatus.Items.FindByValue( ( (int)GroupMemberStatus.Inactive ).ToString() );
            if ( inactiveItem != null )
            {
                rblStatus.Items.Remove( inactiveItem );
            }

            // set default values
            ppGroupMemberPerson.SetValue( null );
            rblStatus.SetValue( (int)GroupMemberStatus.Active );
        }
        
        /// <summary>
        /// Handles the Click event of the btnSaveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroupMember_Click( object sender, EventArgs e )
        {
            if( Page.IsValid && ppGroupMemberPerson.SelectedValue.HasValue )
            {
                var selectedStatus = rblStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();
                
                bool result = AddGroupMember( ppGroupMemberPerson.SelectedValue.Value, _groupId, CCV_GroupRole_Attendee, selectedStatus.Value );

                if ( result )
                {
                    pnlEditGroupMember.Visible = false;
                    pnlGroupView.Visible = true;

                    DisplayViewGroup();
                }
                else
                {
                    // if so, don't add and show error message
                    var person = new PersonService( new RockContext( ) ).Get( ( int ) ppGroupMemberPerson.PersonId );

                    nbGroupMemberErrorMessage.Title = "Person Already In Group";
                    nbGroupMemberErrorMessage.Text = string.Format(
                        "{0} already belongs to this group and cannot be added again.", person.FullName );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelGroupMember_Click( object sender, EventArgs e )
        {
            pnlEditGroupMember.Visible = false;
            pnlGroupView.Visible = true;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }
    }
}