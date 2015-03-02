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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Lava;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List Personalized Lava" )]
    [Category( "Groups" )]
    [Description( "Lists all group that the person is a member of using a Lava template." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 4 )]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 5 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}", "", 6 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 7 )]
    public partial class GroupListPersonalizedLava : RockBlock
    {
        
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ListGroups();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ListGroups();
        }

        #endregion

        #region Internal Methods

        private void ListGroups()
        {
            RockContext rockContext = new RockContext();

            var qry = new GroupMemberService(rockContext)
                        .Queryable("Group")
                        .Where( m => m.PersonId == CurrentPersonId 
                                && m.GroupMemberStatus == GroupMemberStatus.Active 
                                && m.Group.IsActive == true);

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue( "ExcludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( excludeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            var groups = qry.Select( m => new GroupInvolvementSummary  { Group = m.Group, Role = m.GroupRole.Name, IsLeader = m.GroupRole.IsLeader, GroupType = m.Group.GroupType.Name } ).ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Groups", groups );
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            string template = GetAttributeValue( "LavaTemplate" );

            // show debug info
            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }

            lContent.Text = template.ResolveMergeFields( mergeFields );
        }

        [DotLiquid.LiquidType( "Group", "Role", "IsLeader", "GroupType" )]
        public class GroupInvolvementSummary
        {
            public Group Group { get; set; }
            public string Role { get; set; }
            public bool IsLeader { get; set; }
            public string GroupType { get; set; }
        }

        #endregion
    }
}