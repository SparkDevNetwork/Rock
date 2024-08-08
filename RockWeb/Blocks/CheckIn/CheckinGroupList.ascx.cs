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
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;

namespace RockWeb.Blocks.Checkin
{
    /// <summary>
    /// Lists checkin areas and their groups based off a parent checkin configuration group type.
    /// </summary>
    [DisplayName( "Check-in Group List" )]
    [Category( "Check-in" )]
    [Description( "Lists group types and their groups based off group types from query string." )]

    [LinkedPage("Group Detail Page", "Link to the group details page", false)]
    [BooleanField( "Allow Campus Filter", "Should block add an option to allow filtering attendance counts and percentage by campus?", false, "", 2 )]
    [Rock.SystemGuid.BlockTypeGuid( "67E83A02-6D23-4B90-A861-F81FF78B56C7" )]
    public partial class CheckinGroupList : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        RockContext _rockContext = null;
        List<int> _addedGroupTypeIds = new List<int>();
        List<int> _addedGroupIds = new List<int>();
        private bool _allowCampusFilter = false;

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

            _rockContext = new RockContext();

            _allowCampusFilter = GetAttributeValue( "AllowCampusFilter" ).AsBoolean();
            bddlCampus.Visible = _allowCampusFilter;
            if ( _allowCampusFilter )
            {
                bddlCampus.DataSource = CampusCache.All();
                bddlCampus.DataBind();
                bddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
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
                if ( _allowCampusFilter )
                {
                    var preferences = GetBlockPersonPreferences();
                    var campus = CampusCache.Get( preferences.GetValue( "Campus" ).AsInteger() );
                    if ( campus != null )
                    {
                        bddlCampus.Title = campus.Name;
                        bddlCampus.SetValue( campus.Id );
                    }
                }

                ShowContent();
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
            ShowContent();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "Campus", bddlCampus.SelectedValue );
            preferences.Save();

            var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
            bddlCampus.Title = campus != null ? campus.Name : "All Campuses";

            ShowContent();
        }

        #endregion

        #region Methods

        private GroupType GetRootGroupType( int groupId )
        {
            var parentRecursionHistory = new List<int>();
            var groupTypeService = new GroupTypeService( _rockContext );
            var groupType = groupTypeService.Queryable().AsNoTracking()
                .Include( t => t.ParentGroupTypes )
                .Where( t => t.Groups.Select( g => g.Id ).Contains( groupId ) )
                .FirstOrDefault();

            while ( groupType != null && groupType.ParentGroupTypes.Count != 0 )
            {
                var currentGroupTypeId = groupType.Id;
                if ( parentRecursionHistory.Contains( currentGroupTypeId ) )
                {
                    // This Group Type has been previously encountered in the inheritance chain, so a circular reference exists.
                    var exception = new Exception( "Infinite Recursion detected in GetRootGroupType for groupId: " + groupId.ToString() );
                    LogException( exception );
                    return null;
                }
                else
                {
                    var parentGroupType = GetParentGroupType( groupType );
                    if ( parentGroupType != null && parentGroupType.Id == currentGroupTypeId )
                    {
                        // This Group Type is a parent of itself, so it must be the root of the inheritance chain.
                        return groupType;
                    }

                    // Process the parent Group Type next.
                    groupType = parentGroupType;
                }

                // Add the processed GroupType to the recursion tracking list.
                parentRecursionHistory.Add( currentGroupTypeId );
            }

            return groupType;
        }

        private GroupType GetParentGroupType( GroupType groupType )
        {
            GroupTypeService groupTypeService = new GroupTypeService( _rockContext );
            return groupTypeService.Queryable()
                                .Include( t => t.ParentGroupTypes )
                                .AsNoTracking()
                                .Where( t => t.ChildGroupTypes.Select(p => p.Id).Contains( groupType.Id ) ).FirstOrDefault();
        }

        private void ShowContent()
        {

            var groupTypeIds = new List<int>();
            
            if ( !string.IsNullOrWhiteSpace( Request["GroupTypeIds"] ))
            {
                groupTypeIds = Request["GroupTypeIds"].SplitDelimitedValues().AsIntegerList();
            }

            else if( !string.IsNullOrWhiteSpace( Request["GroupId"] ) )
            {
                // get the root group type of this group
                int groupId = Request["GroupId"].AsInteger();
                var rootGroupType = GetRootGroupType( groupId );
                if ( rootGroupType != null )
                {
                    groupTypeIds.Add( rootGroupType.Id );
                }
            }

            if ( !groupTypeIds.Any() )
            {
                lWarnings.Text = "<div class='alert alert-warning'>No group types or groups selected.</div>";
            }
            else
            {
                _addedGroupTypeIds = new List<int>();
                _addedGroupIds = new List<int>();
                int? campusId = bddlCampus.SelectedValueAsInt();

                lContent.Text = BuildHierarchy( groupTypeIds, campusId );
            } 
        }

        private string BuildHierarchy( List<int> groupTypeIds, int? campusId )
        {
            GroupTypeService groupTypeService = new GroupTypeService( _rockContext );

            var groupTypes = groupTypeService
                .Queryable().AsNoTracking()
                .Where( t => groupTypeIds.Contains( t.Id ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .ToList();

            var content = new StringBuilder();

            foreach ( var groupType in groupTypes )
            {
                if ( !_addedGroupTypeIds.Contains( groupType.Id ) )
                {
                    _addedGroupTypeIds.Add( groupType.Id );

                    if ( groupType.GroupTypePurposeValueId == null || groupType.Groups.Count > 0 )
                    {
                        string groupTypeContent = string.Empty;

                        if ( groupType.ChildGroupTypes.Count > 0 )
                        {
                            groupTypeContent = BuildHierarchy( groupType.ChildGroupTypes.Select( t => t.Id ).ToList(), campusId );
                        }

                        var groupContent = new StringBuilder();

                        foreach ( var group in groupType.Groups )
                        {
                            if ( !_addedGroupIds.Contains( group.Id ) &&
                                ( !campusId.HasValue || !group.CampusId.HasValue || campusId.Value == group.CampusId.Value ) )
                            {
                                _addedGroupIds.Add( group.Id );

                                var groupName = group.IsActive ? group.Name : group.Name + " (Inactive)";

                                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "GroupDetailPage" ) ) )
                                {
                                    var groupPageParams = new Dictionary<string, string>();
                                    if ( Request["GroupTypeIds"] != null )
                                    {
                                        groupPageParams.Add( "GroupTypeIds", Request["GroupTypeIds"] );
                                    }
                                    groupPageParams.Add( "GroupId", group.Id.ToString() );
                                    groupContent.Append( string.Format( "<li><a href='{0}'>{1}</a></li>", LinkedPageUrl( "GroupDetailPage", groupPageParams ), groupName ) );
                                }
                                else
                                {
                                    groupContent.Append( string.Format( "<li>{0}</li>", groupName ) );
                                }
                            }
                        }

                        if ( !string.IsNullOrWhiteSpace(groupTypeContent) || groupContent.Length > 0 )
                        {
                            content.Append( "<ul>" );
                            content.Append( string.Format( "<li><strong>{0}</strong></li>", groupType.Name ) );
                            content.Append( groupTypeContent );

                            if ( groupContent.Length > 0 )
                            {
                                content.Append( "<ul>" );
                                content.Append( groupContent.ToString() );
                                content.Append( "</ul>" );
                            }

                            content.Append( "</ul>" );
                        }
                    }
                    else
                    {
                        if ( groupType.ChildGroupTypes.Count > 0 )
                        {
                            content.Append( BuildHierarchy( groupType.ChildGroupTypes.Select( t => t.Id ).ToList(), campusId ) );
                        }
                    }
                }
            }

            return content.ToString();
        }

        #endregion
    }
}