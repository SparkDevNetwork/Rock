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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Navigation Tree for groups
    /// </summary>
    [DisplayName( "Group Tree View" )]
    [Category( "Groups" )]
    [Description( "Creates a navigation tree for groups of the configured group type(s)." )]

    [TextField( "Treeview Title", "Group Tree View", false, order: 1 )]
    [GroupTypesField( "Group Types Include", "Select any specific group types to show in this block. Leave all unchecked to show all group types where 'Show in Navigation' is enabled ( except for excluded group types )", false, key: "GroupTypes", order: 2 )]
    [GroupTypesField( "Group Types Exclude", "Select group types to exclude from this block. Note that this setting is only effective if 'Group Types Include' has no specific group types selected.", false, key: "GroupTypesExclude", order: 3 )]
    [GroupField( "Root Group", "Select the root group to use as a starting point for the tree view.", false, order: 4 )]
    [BooleanField( "Limit to Security Role Groups", order: 5 )]
    [BooleanField( "Show Settings Panel", defaultValue: true, key: "ShowFilterOption", order: 6 )]
    [BooleanField( "Display Inactive Campuses", "Include inactive campuses in the Campus Filter", true )]
    [CustomDropdownListField( "Initial Count Setting", "Select the counts that should be initially shown in the treeview.", "0^None,1^Child Groups,2^Group Members", false, "0", "", 7 )]
    [CustomDropdownListField( "Initial Active Setting", "Select whether to initially show all or just active groups in the treeview", "0^All,1^Active", false, "1", "", 8 )]
    [LinkedPage( "Detail Page", order: 9 )]
    [BooleanField( "Disable Auto-Select First Group", description: "Whether to disable the default behavior of auto-selecting the first group (ordered by name) in the tree view.", order: 10, key: AttributeKey.DisableAutoSelectFirstGroup )]

    [Rock.SystemGuid.BlockTypeGuid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" )]
    public partial class GroupTreeView : RockBlock
    {
        #region Attribute Keys
        public static class AttributeKey
        {
            public const string TreeviewTitle = "TreeviewTitle";
            public const string DisableAutoSelectFirstGroup = "DisableAutoSelectFirstGroup";
        }

        #endregion

        #region Fields

        private string _groupId = string.Empty;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _groupId = PageParameter( "GroupId" );

            var detailPageReference = new Rock.Web.PageReference( GetAttributeValue( "DetailPage" ) );

            // NOTE: if the detail page is the current page, use the current route instead of route specified in the DetailPage (to preserve old behavior)
            if ( detailPageReference == null || detailPageReference.PageId == this.RockPage.PageId )
            {
                hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
                hfDetailPageUrl.Value = new Rock.Web.PageReference( this.RockPage.PageId ).BuildUrl();
            }
            else
            {
                hfPageRouteTemplate.Value = string.Empty;
                var pageCache = PageCache.Get( detailPageReference.PageId );
                if ( pageCache != null )
                {
                    var route = pageCache.PageRoutes.FirstOrDefault( a => a.Id == detailPageReference.RouteId );
                    if ( route != null )
                    {
                        hfPageRouteTemplate.Value = route.Route;
                    }
                }

                hfDetailPageUrl.Value = detailPageReference.BuildUrl();
            }

            hfLimitToSecurityRoleGroups.Value = GetAttributeValue( "LimittoSecurityRoleGroups" );
            Guid? rootGroupGuid = GetAttributeValue( "RootGroup" ).AsGuidOrNull();
            if ( rootGroupGuid.HasValue )
            {
                var group = new GroupService( new RockContext() ).Get( rootGroupGuid.Value );
                if ( group != null )
                {
                    hfRootGroupId.Value = group.Id.ToString();
                }
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upGroupType );

            pnlConfigPanel.Visible = this.GetAttributeValue( "ShowFilterOption" ).AsBooleanOrNull() ?? false;
            pnlRolloverConfig.Visible = this.GetAttributeValue( "ShowFilterOption" ).AsBooleanOrNull() ?? false;

            if ( pnlConfigPanel.Visible )
            {
                var hideInactiveGroups = this.GetUserPreference( "HideInactiveGroups" ).AsBooleanOrNull();
                if ( !hideInactiveGroups.HasValue )
                {
                    hideInactiveGroups = this.GetAttributeValue( "InitialActiveSetting" ) == "1";
                }

                tglHideInactiveGroups.Checked = hideInactiveGroups ?? true;

                tglLimitPublicGroups.Checked = this.GetUserPreference( "LimitPublicGroups" ).AsBooleanOrNull() ?? false;
            }
            else
            {
                // if the filter is hidden, don't show inactive groups
                tglHideInactiveGroups.Checked = true;
            }

            ddlCountsType.Items.Clear();
            ddlCountsType.Items.Add( new ListItem( string.Empty, TreeViewItem.GetCountsType.None.ConvertToInt().ToString() ) );
            ddlCountsType.Items.Add( new ListItem( TreeViewItem.GetCountsType.ChildGroups.ConvertToString(), TreeViewItem.GetCountsType.ChildGroups.ConvertToInt().ToString() ) );
            ddlCountsType.Items.Add( new ListItem( TreeViewItem.GetCountsType.GroupMembers.ConvertToString(), TreeViewItem.GetCountsType.GroupMembers.ConvertToInt().ToString() ) );

            var countsType = this.GetUserPreference( "CountsType" );
            if ( string.IsNullOrEmpty( countsType ) )
            {
                countsType = this.GetAttributeValue( "InitialCountSetting" );
            }

            if ( _groupId.IsNullOrWhiteSpace() )
            {
                SetAllowedGroupTypes();
                FindFirstGroup();
            }

            if ( pnlConfigPanel.Visible )
            {
                ddlCountsType.SetValue( countsType );
            }
            else
            {
                ddlCountsType.SetValue( "" );
            }

            ddlCampuses.Campuses = CampusCache.All( GetAttributeValue( "DisplayInactiveCampuses" ).AsBoolean() );

            var campusFilter = this.GetUserPreference( "CampusFilter" );
            if ( pnlConfigPanel.Visible )
            {
                ddlCampuses.SetValue( campusFilter );
            }
            else
            {
                ddlCampuses.SetValue( "" );
            }

            if ( pnlConfigPanel.Visible )
            {
                tglIncludeNoCampus.Visible = ddlCampuses.Visible;
                tglIncludeNoCampus.Checked = this.GetUserPreference( "IncludeNoCampus" ).AsBoolean();
            }


            lPanelTitle.Text = GetAttributeValue( AttributeKey.TreeviewTitle );

            var shouldDisableAutoSelectFirstGroup = GetAttributeValue( AttributeKey.DisableAutoSelectFirstGroup )
                .AsBooleanOrNull()
                .GetValueOrDefault();

            if ( !shouldDisableAutoSelectFirstGroup && string.IsNullOrWhiteSpace( _groupId ) )
            {
                // If no group was selected, try to find the first group and redirect
                // back to current page with that group selected
                var group = FindFirstGroup();
                {
                    if ( group != null )
                    {
                        _groupId = group.Id.ToString();
                        string redirectUrl = string.Empty;

                        // redirect so that the group treeview has the first node selected right away and group detail shows the group
                        if ( hfPageRouteTemplate.Value.IndexOf( "{groupId}", StringComparison.OrdinalIgnoreCase ) >= 0 )
                        {
                            redirectUrl = "~/" + hfPageRouteTemplate.Value.ReplaceCaseInsensitive( "{groupId}", _groupId.ToString() );
                        }
                        else
                        {
                            if ( this.Request.QueryString.Count == 0 )
                            {
                                redirectUrl = this.Request.UrlProxySafe() + "?GroupId=" + _groupId.ToString();
                            }
                            else
                            {
                                redirectUrl = this.Request.UrlProxySafe() + "&GroupId=" + _groupId.ToString();
                            }
                        }

                        this.Response.Redirect( redirectUrl, false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SetAllowedGroupTypes();
            }

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            bool showAddChildGroupButton = false;

            if ( !string.IsNullOrWhiteSpace( _groupId ) )
            {
                string key = string.Format( "Group:{0}", _groupId );
                var rockContext = new RockContext();
                Group selectedGroup = RockPage.GetSharedItem( key ) as Group;
                if ( selectedGroup == null )
                {
                    int id = _groupId.AsInteger();
                    selectedGroup = new GroupService( rockContext )
                        .Queryable( "GroupType" )
                        .AsNoTracking()
                        .Where( g => g.Id == id )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, selectedGroup );
                }

                // get the parents of the selected item so we can tell the treeview to expand those
                int? rootGroupId = hfRootGroupId.Value.AsIntegerOrNull();
                List<string> parentIdList = new List<string>();
                var group = selectedGroup;
                while ( group != null )
                {
                    if ( !IsGroupTypeIncluded( group.GroupTypeId ) )
                    {
                        group = null;
                        selectedGroup = null;
                    }
                    else if ( group.Id == rootGroupId )
                    {
                        // stop if we are at the root group
                        group = null;
                    }
                    else
                    {
                        group = group.ParentGroup;
                    }

                    if ( group != null )
                    {
                        if ( !parentIdList.Contains( group.Id.ToString() ) )
                        {
                            parentIdList.Insert( 0, group.Id.ToString() );
                        }
                        else
                        {
                            // The parent list already contains this node, so we have encountered a recursive loop.
                            // Stop here and make the current node the root of the tree.
                            group = null;
                        }
                    }
                }

                // also get any additional expanded nodes that were sent in the Post
                string postedExpandedIds = this.Request.Params["ExpandedIds"];
                if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
                {
                    var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList();
                    foreach ( var id in postedExpandedIdList )
                    {
                        if ( !parentIdList.Contains( id ) )
                        {
                            parentIdList.Add( id );
                        }
                    }
                }

                if ( selectedGroup != null )
                {
                    var selectedGroupGroupType = GroupTypeCache.Get( selectedGroup.GroupTypeId );
                    hfInitialGroupId.Value = selectedGroup.Id.ToString();
                    hfSelectedGroupId.Value = selectedGroup.Id.ToString();

                    // show the Add Child Group button if the selected Group's GroupType can have children and one or more of those child group types is allowed
                    if ( selectedGroupGroupType.AllowAnyChildGroupType || selectedGroupGroupType.ChildGroupTypes.Any( c => IsGroupTypeIncluded( c.Id ) ) )
                    {
                        // if current person has Edit Auth on the block, then show the add child group button regardless of Group/GroupType security.
                        // 
                        showAddChildGroupButton = canEditBlock;

                        if ( !showAddChildGroupButton )
                        {
                            // if block doesn't grant Edit auth, see if the person is authorized for the selected group
                            showAddChildGroupButton = selectedGroup.IsAuthorized( Authorization.EDIT, CurrentPerson );
                            if ( !showAddChildGroupButton )
                            {
                                // if block doesn't grant Edit auth, and user isn't authorized for the selected group,
                                // see if they have Edit auth on any of the child groups
                                List<GroupTypeCache> allowedChildGroupTypes;
                                if ( selectedGroupGroupType.AllowAnyChildGroupType )
                                {
                                    // If AllowAnyChildGroupType is enabled, check for Edit Auth on all GroupTypes
                                    allowedChildGroupTypes = GroupTypeCache.All().ToList();
                                }
                                else
                                {
                                    // If AllowAnyChildGroupType is not enabled, check for Edit Auth on the just the selected group's ChildGroupTypes
                                    allowedChildGroupTypes = selectedGroupGroupType.ChildGroupTypes;
                                }

                                foreach ( var childGroupType in allowedChildGroupTypes )
                                {
                                    if ( childGroupType != null && childGroupType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                                    {
                                        showAddChildGroupButton = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                hfInitialGroupParentIds.Value = parentIdList.AsDelimited( "," );
            }
            else
            {
                // let the Add button be visible if there is nothing selected (if authorized)
                lbAddGroupChild.Enabled = canEditBlock;
            }

            // NOTE that showAddChildGroupButton just controls if the button is shown.
            // The group detail block will take care of enforcing auth when they attempt to save a group.
            divAddGroup.Visible = canEditBlock || showAddChildGroupButton;
            lbAddGroupRoot.Enabled = canEditBlock;
            lbAddGroupChild.Enabled = showAddChildGroupButton;

            // disable add child group if no group is selected
            if ( hfSelectedGroupId.ValueAsInt() == 0 )
            {
                lbAddGroupChild.Enabled = false;
            }

            hfIncludeInactiveGroups.Value = ( !tglHideInactiveGroups.Checked ).ToTrueFalse();
            hfLimitPublicGroups.Value = tglLimitPublicGroups.Checked.ToTrueFalse();
            hfCountsType.Value = ddlCountsType.SelectedValue;
            hfCampusFilter.Value = ddlCampuses.SelectedValue;
            hfIncludeNoCampus.Value = tglIncludeNoCampus.Checked.ToTrueFalse();
        }

        /// <summary>
        /// Determines whether [is group type included] [the specified group type identifier].
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private bool IsGroupTypeIncluded( int groupTypeId )
        {
            List<int> includeGroupTypes = hfGroupTypesInclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            List<int> excludeGroupTypes = hfGroupTypesExclude.Value.SplitDelimitedValues().AsIntegerList();
            if ( includeGroupTypes.Any() )
            {
                //// if includedGroupTypes has values, only include groupTypes from the includedGroupTypes list
                return includeGroupTypes.Contains( groupTypeId );
            }
            else if ( excludeGroupTypes.Any() )
            {
                //// if includedGroupTypes doesn't have anything, exclude groupTypes that are in the excludeGroupTypes list
                return !excludeGroupTypes.Contains( groupTypeId );
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddGroupRoot_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "GroupId", 0.ToString() );
            qryParams.Add( "ParentGroupId", hfRootGroupId.Value );
            qryParams.Add( "ExpandedIds", hfInitialGroupParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroupChild_Click( object sender, EventArgs e )
        {
            int groupId = hfSelectedGroupId.ValueAsInt();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "GroupId", 0.ToString() );
            qryParams.Add( "ParentGroupId", groupId.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialGroupParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the allowed group types.
        /// </summary>
        private void SetAllowedGroupTypes()
        {
            // limit GroupType selection to what Block Attributes allow
            hfGroupTypesInclude.Value = string.Empty;
            List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();

            if ( groupTypeIncludeGuids.Any() )
            {
                var groupTypeIdIncludeList = new List<int>();
                foreach ( Guid guid in groupTypeIncludeGuids )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        groupTypeIdIncludeList.Add( groupType.Id );
                    }
                }

                hfGroupTypesInclude.Value = groupTypeIdIncludeList.AsDelimited( "," );
            }

            hfGroupTypesExclude.Value = string.Empty;
            List<Guid> groupTypeExcludeGuids = GetAttributeValue( "GroupTypesExclude" ).SplitDelimitedValues().AsGuidList();
            if ( groupTypeExcludeGuids.Any() )
            {
                var groupTypeIdExcludeList = new List<int>();
                foreach ( Guid guid in groupTypeExcludeGuids )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        groupTypeIdExcludeList.Add( groupType.Id );
                    }
                }

                hfGroupTypesExclude.Value = groupTypeIdExcludeList.AsDelimited( "," );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglHideInactiveGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglHideInactiveGroups_CheckedChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "HideInactiveGroups", tglHideInactiveGroups.Checked.ToTrueFalse() );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglLimitPublicGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglLimitPublicGroups_CheckedChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "LimitPublicGroups", tglLimitPublicGroups.Checked.ToTrueFalse() );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCountType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCountsType_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "CountsType", ddlCountsType.SelectedValue );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampuses_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "CampusFilter", ddlCampuses.SelectedValue );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the CheckedChange event of the cbIncludeNoCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglIncludeNoCampus_CheckedChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "IncludeNoCampus", tglIncludeNoCampus.Checked.ToTrueFalse() );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the OnClick event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_OnClick( object sender, EventArgs e )
        {
            // redirect to search
            NavigateToPage( Rock.SystemGuid.Page.GROUP_SEARCH_RESULTS.AsGuid(), new Dictionary<string, string>() { { "SearchType", "name" }, { "SearchTerm", tbSearch.Text.Trim() } } );
        }

        /// <summary>
        /// Finds the first group.
        /// </summary>
        /// <returns></returns>
        private Group FindFirstGroup()
        {
            var groupService = new GroupService( new RockContext() );
            var includedGroupTypeIds = hfGroupTypesInclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            var excludedGroupTypeIds = hfGroupTypesExclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

            // if specific group types are specified, show the groups regardless of ShowInNavigation
            bool limitToShowInNavigation = !includedGroupTypeIds.Any();

            var qry = groupService.GetChildren( 0, hfRootGroupId.ValueAsInt(), hfLimitToSecurityRoleGroups.Value.AsBoolean(), includedGroupTypeIds, excludedGroupTypeIds, !tglHideInactiveGroups.Checked, limitToShowInNavigation, hfCampusFilter.ValueAsInt(), tglIncludeNoCampus.Checked, tglHideInactiveGroups.Checked );

            foreach ( var group in qry.OrderBy( g => g.Name ) )
            {
                // return first group they are authorized to view
                if ( group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    return group;
                }
            }

            return null;
        }

        #endregion
    }
}