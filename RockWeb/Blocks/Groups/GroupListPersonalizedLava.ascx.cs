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
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List Personalized Lava" )]
    [Category( "Groups" )]
    [Description( "Lists all group that the person is a member of using a Lava template." )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "",
        IsRequired = false,
        Order = 0 )]

    [GroupField( "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "If a group is chosen, only the groups under this group will be displayed.",
        IsRequired = false,
        Order = 1 )]

    [IntegerField( "Cache Duration",
        Key = AttributeKey.CacheDuration,
        Description = "Length of time in seconds to cache which groups are descendants of the parent group.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Order = 2 )]

    [GroupTypesField( "Include Group Types",
        Key = AttributeKey.IncludeGroupTypes,
        Description = "The group types to display in the list.  If none are selected, all group types will be included.",
        IsRequired = false,
        Order = 3 )]

    [GroupTypesField( "Exclude Group Types",
        Key = AttributeKey.ExcludeGroupTypes,
        Description = "The group types to exclude from the list (only valid if including all groups).",
        IsRequired = false,
        Order = 4 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to format the group list.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}",
        Order = 5 )]

    [BooleanField( "Display Inactive Groups",
        Key = AttributeKey.DisplayInactiveGroups,
        Description = "Include inactive groups in the lava results",
        DefaultBooleanValue = false,
        Order = 6 )]

    [CustomDropdownListField( "Initial Active Setting",
        Key = AttributeKey.InitialActiveSetting,
        Description = "Select whether to initially show all or just active groups in the lava.",
        ListSource = "0^All,1^Active",
        IsRequired = false,
        DefaultValue = "1",
        Order = 7 )]

    [TextField( "Inactive Parameter Name",
        Key = AttributeKey.InactiveParameterName,
        Description = "The page parameter name to toggle inactive groups.",
        IsRequired = false,
        DefaultValue = "showinactivegroups",
        Order = 8 )]

    [CustomCheckboxListField( "Cache Tags",
        Key = AttributeKey.CacheTags,
        Description = "Cached tags are used to link cached content so that it can be expired as a group.",
        ListSource = CACHE_TAG_LIST,
        IsRequired = false,
        Order = 9 )]

    #endregion Block Attributes

    public partial class GroupListPersonalizedLava : RockBlock
    {
        private class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ParentGroup = "ParentGroup";
            public const string CacheDuration = "CacheDuration";
            public const string IncludeGroupTypes = "IncludeGroupTypes";
            public const string ExcludeGroupTypes = "ExcludeGroupTypes";
            public const string LavaTemplate = "LavaTemplate";
            public const string DisplayInactiveGroups = "DisplayInactiveGroups";
            public const string InitialActiveSetting = "InitialActiveSetting";
            public const string InactiveParameterName = "InactiveParameterName";
            public const string CacheTags = "CacheTags";
        }

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

            if ( this.GetAttributeValue( AttributeKey.DisplayInactiveGroups ).AsBoolean() )
            {
                _hideInactive = this.GetAttributeValue( AttributeKey.InitialActiveSetting ) == "1";
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
                var inactiveGroups = PageParameter( GetAttributeValue( AttributeKey.InactiveParameterName ) ).AsBooleanOrNull();
                if ( this.GetAttributeValue( AttributeKey.DisplayInactiveGroups ).AsBoolean() && inactiveGroups.HasValue )
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

            var parentGroupGuid = GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();
            if ( parentGroupGuid != null )
            {
                var cacheLength = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();

                List<int> availableGroupIds = null;

                if ( cacheLength > 0 )
                {
                    availableGroupIds = ( List<int> ) GetCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString() );
                }

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

                    if ( cacheLength > 0 )
                    {
                        string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                        AddCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString(), availableGroupIds, cacheLength, cacheTags );
                    }
                }
                qry = qry.Where( m => availableGroupIds.Contains( m.GroupId ) );
            }

            qry = qry.Where( m => m.PersonId == CurrentPersonId );

            if ( _hideInactive )
            {
                qry = qry.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
                qry = qry.Where( m => m.Group.IsActive == true && !m.Group.IsArchived );
            }

            List<Guid> includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( excludeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            var groups = new List<GroupInvolvementSummary>();

            foreach ( var groupMember in qry.ToList() )
            {
                if ( groupMember.Group.IsAuthorized( Authorization.VIEW, CurrentPerson ) && groupMember.GroupRole.CanView )
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
            linkedPages.Add( AttributeKey.DetailPage, LinkedPageRoute( AttributeKey.DetailPage ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            if ( this.GetAttributeValue( AttributeKey.DisplayInactiveGroups ).AsBoolean() )
            {
                mergeFields.Add( "ShowInactive", this.GetAttributeValue( AttributeKey.DisplayInactiveGroups ) );
                mergeFields.Add( "InitialActive", this.GetAttributeValue( AttributeKey.InitialActiveSetting ) );
                mergeFields.Add( "InactiveParameter", this.GetAttributeValue( AttributeKey.InactiveParameterName ) );
            }

            string template = GetAttributeValue( AttributeKey.LavaTemplate );

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
        public class GroupInvolvementSummary : LavaDataObject
        {
            public Group Group { get; set; }
            public string Role { get; set; }
            public bool IsLeader { get; set; }
            public string GroupType { get; set; }
        }

        #endregion
    }
}