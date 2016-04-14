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
    [DisplayName( "NS Group Detail Lava" )]
    [Category( "CCV > Groups" )]
    [Description( "Presents the details of a NS group using Lava" )]
    public partial class NSGroupDetailLava : ToolboxGroupDetailLava
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
               
        public override TextBox GroupName { get { return tbName; } }
        public override TextBox GroupDesc { get { return tbDescription; } }
        public override CheckBox IsActive { get { return cbIsActive; } }
        
        protected override void FinalizePresentView( Dictionary<string, object> mergeFields, bool enableDebug )
        {
            string template = GetAttributeValue( "LavaTemplate" );

            // show debug info
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                string postbackCommands = @"<h5>Available Postback Commands</h5>
                                            <ul>
                                                <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                <li><strong>SendCommunication:</strong> Sends a communication to all group members on behalf of the Current User. This will redirect them to the communication page where they can author their email. <code>{{ '' | Postback:'SendCommunication' }}</code></li>
                                            </ul>";

                DebugContent.Visible = true;
                DebugContent.Text = mergeFields.lavaDebugInfo( null, string.Empty, postbackCommands );
            }

            MainViewContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( MainPanel.ClientID );
        }

        protected override void HandlePageAction( string action )
        {
            switch ( action )
            {
                case "EditGroup":         DisplayEditGroup( );  break;
                case "SendCommunication": SendCommunication( ); break;
            }
        }
    }
}