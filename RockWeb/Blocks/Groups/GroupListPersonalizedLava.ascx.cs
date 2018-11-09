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
using System.Runtime.Caching;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List Personalized Lava" )]
    [Category( "Groups" )]
    [Description( "Lists all group that the person is a member of using a Lava template." )]

    [LinkedPage( "Detail Page", "", false, "", "", 0 )]
    [GroupField( "Parent Group", "If a group is chosen, only the groups under this group will be displayed.", false, order: 1 )]
    [IntegerField( "Cache Duration", "Length of time in seconds to cache which groups are descendants of the parent group.", false, 3600, "", 2 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 3 )]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 4 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}", "", 5 )]
    [BooleanField( "Display Inactive Groups", "Include inactive groups in the lava results", false, order: 6 )]
    [CustomDropdownListField( "Initial Active Setting", "Select whether to initially show all or just active groups in the lava", "0^All,1^Active", false, "1", "", 7 )]
    [TextField( "Inactive Parameter Name", "The page parameter name to toggle inactive groups", false, "showinactivegroups", order: 8 )]
    [CustomCheckboxListField( "Cache Tags", "Cached tags are used to link cached content so that it can be expired as a group", CACHE_TAG_LIST, false, key: "CacheTags", order: 9 )]


    public partial class GroupListPersonalizedLava : RockBlock
    {

        #region Fields

        private bool _hideInactive = true;

        private const string CACHE_TAG_LIST = @"
            SELECT CAST([DefinedValue].[Value] AS VARCHAR) AS [Value], [DefinedValue].[Value] AS [Text]
            FROM[DefinedType]
            JOIN[DefinedValue] ON[DefinedType].[Id] = [DefinedValue].[DefinedTypeId]
            WHERE[DefinedType].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;

            if ( this.GetAttributeValue( "DisplayInactiveGroups" ).AsBoolean() )
            {
                var hideInactiveGroups = this.GetUserPreference( "HideInactiveGroups" ).AsBooleanOrNull();
                if ( !hideInactiveGroups.HasValue )
                {
                    hideInactiveGroups = this.GetAttributeValue( "InitialActiveSetting" ) == "1";
                }

                _hideInactive = hideInactiveGroups ?? true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var inactiveGroups = PageParameter( GetAttributeValue( "InactiveParameterName" ) ).AsBooleanOrNull();
                if ( this.GetAttributeValue( "DisplayInactiveGroups" ).AsBoolean() && inactiveGroups.HasValue )
                {
                    _hideInactive = !inactiveGroups ?? true;
                }
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

            var qry = new GroupMemberService( rockContext )
                        .Queryable( "Group" );

            var parentGroupGuid = GetAttributeValue( "ParentGroup" ).AsGuidOrNull();
            if ( parentGroupGuid != null )
            {
                var availableGroupIds = ( List<int> ) GetCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString() );

                if ( availableGroupIds == null )
                {
                    var parentGroup = new GroupService( rockContext ).Get( parentGroupGuid ?? new Guid() );
                    if ( parentGroup != null )
                    {
                        availableGroupIds = GetChildGroups( parentGroup ).Select( g => g.Id ).ToList();
                    }
                    else
                    {
                        availableGroupIds = new List<int>();
                    }
                    var cacheLength = GetAttributeValue( "CacheDuration" ).AsInteger();
                    string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                    AddCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString(), availableGroupIds, cacheLength, cacheTags );
                }
                qry = qry.Where( m => availableGroupIds.Contains( m.GroupId ) );
            }

            qry = qry.Where( m => m.PersonId == CurrentPersonId
                        && m.GroupMemberStatus == GroupMemberStatus.Active );

            if ( _hideInactive )
            {
                qry = qry.Where( m => m.Group.IsActive == true && !m.Group.IsArchived );
            }

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

            var groups = new List<GroupInvolvementSummary>();

            foreach ( var groupMember in qry.ToList() )
            {
                if ( groupMember.Group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    groups.Add( new GroupInvolvementSummary
                    {
                        Group = groupMember.Group,
                        Role = groupMember.GroupRole.Name,
                        IsLeader = groupMember.GroupRole.IsLeader,
                        GroupType = groupMember.Group.GroupType.Name
                    } );
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Groups", groups );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            if ( this.GetAttributeValue( "DisplayInactiveGroups" ).AsBoolean() )
            {
                mergeFields.Add( "ShowInactive", this.GetAttributeValue( "DisplayInactiveGroups" ) );
                mergeFields.Add( "InitialActive", this.GetAttributeValue( "InitialActiveSetting" ) );
                mergeFields.Add( "InactiveParameter", this.GetAttributeValue( "InactiveParameterName" ) );
            }

            string template = GetAttributeValue( "LavaTemplate" );

            lContent.Text = template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Recursively loads all descendants of the group
        /// </summary>
        /// <param name="group">Group to load from</param>
        /// <returns></returns>
        private List<Group> GetChildGroups( Group group )
        {
            List<Group> childGroups = group.Groups.ToList();
            List<Group> grandChildGroups = new List<Group>();
            foreach ( var childGroup in childGroups )
            {
                grandChildGroups.AddRange( GetChildGroups( childGroup ) );
            }
            childGroups.AddRange( grandChildGroups );
            return childGroups;
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