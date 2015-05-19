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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

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
    [BooleanField( "Show Filter Option to include Inactive Groups", defaultValue: true, key: "ShowFilterOption", order: 6 )]
    [LinkedPage( "Detail Page", order: 7 )]
    public partial class GroupTreeView : RockBlock
    {
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

            hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
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

            tglHideInactiveGroups.Visible = this.GetAttributeValue( "ShowFilterOption" ).AsBooleanOrNull() ?? true;
            if ( tglHideInactiveGroups.Visible )
            {
                tglHideInactiveGroups.Checked = this.GetUserPreference( "HideInactiveGroups" ).AsBooleanOrNull() ?? true;
            }
            else
            {
                // if the filter is hidden, don't show inactive groups
                tglHideInactiveGroups.Checked = true;
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

                if ( string.IsNullOrWhiteSpace( _groupId ) )
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
                                    redirectUrl = this.Request.Url + "?GroupId=" + _groupId.ToString();
                                }
                                else
                                {
                                    redirectUrl = this.Request.Url + "&GroupId=" + _groupId.ToString();
                                }
                            }

                            this.Response.Redirect( redirectUrl, false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            bool canAddChildGroup = false;

            if ( !string.IsNullOrWhiteSpace( _groupId ) )
            {
                string key = string.Format( "Group:{0}", _groupId );
                var rockContext = new RockContext();
                Group selectedGroup = RockPage.GetSharedItem( key ) as Group;
                if ( selectedGroup == null )
                {
                    int id = _groupId.AsInteger();
                    selectedGroup = new GroupService( rockContext ).Queryable( "GroupType" )
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
                        parentIdList.Insert( 0, group.Id.ToString() );
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
                    hfInitialGroupId.Value = selectedGroup.Id.ToString();
                    hfSelectedGroupId.Value = selectedGroup.Id.ToString();

                    // show the Add button if the selected Group's GroupType can have children and one or more of those child group types is allowed
                    if ( selectedGroup.GroupType.ChildGroupTypes.Count > 0 &&
                        ( selectedGroup.GroupType.ChildGroupTypes.Any( c => IsGroupTypeIncluded( c.Id ) ) ) )
                    {
                        canAddChildGroup = canEditBlock;

                        if ( !canAddChildGroup )
                        {
                            canAddChildGroup = selectedGroup.IsAuthorized( Authorization.EDIT, CurrentPerson );
                            if ( !canAddChildGroup )
                            {
                                var groupType = GroupTypeCache.Read( selectedGroup.GroupTypeId );
                                if ( groupType != null )
                                {
                                    foreach ( var childGroupType in groupType.ChildGroupTypes )
                                    {
                                        if ( childGroupType != null && childGroupType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                                        {
                                            canAddChildGroup = true;
                                            break;
                                        }
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

            divTreeviewActions.Visible = canEditBlock || canAddChildGroup;
            lbAddGroupRoot.Enabled = canEditBlock;
            lbAddGroupChild.Enabled = canAddChildGroup;

            // disable add child group if no group is selected
            if ( hfSelectedGroupId.ValueAsInt() == 0 )
            {
                lbAddGroupChild.Enabled = false;
            }

            hfIncludeInactiveGroups.Value = ( !tglHideInactiveGroups.Checked ).ToTrueFalse();
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
                return ( includeGroupTypes.Contains( groupTypeId ) );
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
                    var groupType = GroupTypeCache.Read( guid );
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
                    var groupType = GroupTypeCache.Read( guid );
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

            var qry = groupService.GetChildren( 0, hfRootGroupId.ValueAsInt(), hfLimitToSecurityRoleGroups.Value.AsBoolean(), includedGroupTypeIds, excludedGroupTypeIds, !tglHideInactiveGroups.Checked, limitToShowInNavigation );

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