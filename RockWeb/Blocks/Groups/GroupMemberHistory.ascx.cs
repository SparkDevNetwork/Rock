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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Member History" )]
    [Category( "Groups" )]
    [Description( "Displays a timeline of history for a group member. If only GroupId is specified, a list of group members that have been in the group will be shown first." )]

    [CodeEditorField( "Timeline Lava Template", "The Lava Template to use when rendering the timeline view of the history.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}", order: 1 )]
    [LinkedPage( "Group History Grid Page", defaultValue: Rock.SystemGuid.Page.GROUP_HISTORY_GRID, required: true, order: 2 )]
    [LinkedPage( "Group Member History Page", defaultValue: Rock.SystemGuid.Page.GROUP_MEMBER_HISTORY, required: true, order: 3 )]
    [BooleanField( "Show Members Grid", "Show Members Grid if GroupMemberId is not specified in the URL", true, order: 4 )]
    [Rock.SystemGuid.BlockTypeGuid( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226" )]
    public partial class GroupMemberHistory : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region Base Control Methods

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
            gGroupMembers.GridRebind += ( sender, ge ) => { BindMembersGrid(); };

            /// add lazyload js so that person-link-popover javascript works (see GroupMemberList.ascx)
            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();
                if ( !groupId.HasValue )
                {
                    if ( groupMemberId.HasValue )
                    {
                        groupId = new GroupMemberService( new RockContext() ).GetSelect( groupMemberId.Value, a => a.GroupId );
                    }
                }

                if ( groupId.HasValue )
                {
                    ShowDetail( groupId.Value, groupMemberId );
                }
                else
                {
                    this.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_RowSelected( object sender, RowEventArgs e )
        {
            Dictionary<string, string> additionalQueryParameters = new Dictionary<string, string> { { "GroupMemberId", e.RowKeyId.ToString() } };
            this.NavigateToCurrentPageReference( additionalQueryParameters );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfGroupMembers_ApplyFilterClick( object sender, EventArgs e )
        {
            int groupId = hfGroupId.Value.AsInteger();

            gfGroupMembers.PreferenceKeyPrefix = string.Format( "{0}-", groupId );
            gfGroupMembers.SetFilterPreference( "First Name", tbFirstName.Text );
            gfGroupMembers.SetFilterPreference( "Last Name", tbLastName.Text );
            gfGroupMembers.SetFilterPreference( "Last Role", cblRole.SelectedValues.AsIntegerList().ToJson() );
            gfGroupMembers.SetFilterPreference( "Status", "Last Status", cblGroupMemberStatus.SelectedValues.AsIntegerList().ToJson() );
            gfGroupMembers.SetFilterPreference( "Date Added", sdrDateAdded.DelimitedValues );
            gfGroupMembers.SetFilterPreference( "Date Removed", sdrDateRemoved.DelimitedValues );

            BindMembersGrid();
        }

        /// <summary>
        /// Gfs the group members display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfGroupMembers_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            int groupId = hfGroupId.Value.AsInteger();
            if ( e.Key == "Group Role" )
            {
                List<int> selectedGroupRoleIds = e.Value.FromJsonOrNull<List<int>>() ?? new List<int>();
                e.Value = cblRole.Items.OfType<ListItem>().Where( a => selectedGroupRoleIds.Contains( a.Value.AsInteger() ) ).Select( a => a.Text ).ToList().AsDelimited( ", ", " or " );
            }
            else if ( e.Key == "Group Member Status" )
            {
                List<int> selectedGroupMemberStatuses = e.Value.FromJsonOrNull<List<int>>() ?? new List<int>();
                e.Value = cblGroupMemberStatus.Items.OfType<ListItem>().Where( a => selectedGroupMemberStatuses.Contains( a.Value.AsInteger() ) ).Select( a => a.Text ).ToList().AsDelimited( ", ", " or " );
            }
            else if ( e.Key == "Date Added" || e.Key == "Date Removed" )
            {
                e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == "First Name" || e.Key == "Last Name" )
            {
                // let the value go thru unchanged
            }
            else
            {
                // unexpected key or a key for another GroupId
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfGroupMembers_ClearFilterClick( object sender, EventArgs e )
        {
            gfGroupMembers.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            const string photoFormat = "<div class=\"photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";
            GroupMember groupMember = e.Row.DataItem as GroupMember;
            Literal lPersonNameHtml = e.Row.FindControl( "lPersonNameHtml" ) as Literal;
            Literal lPersonProfileLink = e.Row.FindControl( "lPersonProfileLink" ) as Literal;
            if ( groupMember != null )
            {
                lPersonNameHtml.Text = string.Format( photoFormat, groupMember.PersonId, groupMember.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) )
                    + groupMember.ToString();

                string personUrl = this.ResolveUrl( string.Format( "~/Person/{0}", groupMember.PersonId ) );
                lPersonProfileLink.Text = string.Format( @"<a href='{0}'><div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div></a>", personUrl );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        public void ShowDetail( int groupId, int? groupMemberId )
        {
            hfGroupId.Value = groupId.ToString();
            hfGroupMemberId.Value = groupMemberId.ToString();
            pnlMembers.Visible = !groupMemberId.HasValue;
            int groupTypeId = new GroupService( new RockContext() ).GetSelect( groupId, a => a.GroupTypeId );
            bool showMembersGrid = this.GetAttributeValue( "ShowMembersGrid" ).AsBoolean();

            // only show this block if GroupHistory is enabled
            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType != null && groupType.EnableGroupHistory == false )
            {
                this.Visible = false;
                return;
            }

            if ( groupMemberId.HasValue )
            {
                // if groupMemberId is 0, we are adding a new group member, so don't show group member history (and don't show the member's grid either)
                if ( groupMemberId == 0 )
                {
                    this.Visible = false;
                    return;
                }

                ShowGroupMemberHistory( groupMemberId.Value );
            }
            else if ( showMembersGrid )
            {
                BindFilter();
                BindMembersGrid();
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        public void BindFilter()
        {
            var rockContext = new RockContext();
            int groupId = hfGroupId.Value.AsInteger();
            var group = new GroupService( rockContext ).Get( groupId );
            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );
            gfGroupMembers.PreferenceKeyPrefix = string.Format( "{0}-", groupId );

            tbFirstName.Text = gfGroupMembers.GetFilterPreference( "First Name" );
            tbLastName.Text = gfGroupMembers.GetFilterPreference( "Last Name" );

            sdrDateAdded.DelimitedValues = gfGroupMembers.GetFilterPreference( "Date Added" );
            sdrDateRemoved.DelimitedValues = gfGroupMembers.GetFilterPreference( "Date Removed" );
            cblRole.Items.Clear();

            if ( groupTypeCache != null )
            {
                foreach ( var role in groupTypeCache.Roles )
                {
                    cblRole.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
                }
            }

            List<int> selectedGroupRoleIds = gfGroupMembers.GetFilterPreference( "Group Role" ).FromJsonOrNull<List<int>>() ?? new List<int>();
            cblRole.SetValues( selectedGroupRoleIds );

            cblGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            List<GroupMemberStatus> selectedGroupMemberStatuses = gfGroupMembers.GetFilterPreference( "Group Member Status" ).FromJsonOrNull<List<GroupMemberStatus>>() ?? new List<GroupMemberStatus>();
            cblGroupMemberStatus.SetValues( selectedGroupMemberStatuses.Select( a => a.ConvertToInt() ) );
        }


        /// <summary>
        /// Binds the members grid.
        /// </summary>
        public void BindMembersGrid()
        {
            int groupId = hfGroupId.Value.AsInteger();
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( groupId );
            if ( group != null )
            {
                lGroupTitle.Text = group.Name.FormatAsHtmlTitle();
            }

            lGroupGridTitle.Visible = true;
            lGroupMemberPreHtml.Visible = false;
            lGroupMemberTitle.Visible = false;

            // get the unfiltered list of group members, which includes archived and deceased
            var qryGroupMembers = groupMemberService.AsNoFilter().Where( a => a.GroupId == groupId );

            // don't include deceased
            qryGroupMembers = qryGroupMembers.Where( a => a.Person.IsDeceased == false );

            // Filter by First Name
            string firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qryGroupMembers = qryGroupMembers.Where( m =>
                    m.Person.FirstName.StartsWith( firstName ) ||
                    m.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            string lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qryGroupMembers = qryGroupMembers.Where( m => m.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by Date Added
            var dateAddedRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrDateAdded.DelimitedValues );
            if ( dateAddedRange.Start.HasValue )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.DateTimeAdded >= dateAddedRange.Start.Value );
            }
            if ( dateAddedRange.End.HasValue )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.DateTimeAdded < dateAddedRange.End.Value );
            }

            var dateRemovedRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrDateRemoved.DelimitedValues );
            if ( dateRemovedRange.Start.HasValue )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.ArchivedDateTime >= dateRemovedRange.Start.Value );
            }
            if ( dateRemovedRange.End.HasValue )
            {
                qryGroupMembers = qryGroupMembers.Where( a => a.ArchivedDateTime < dateRemovedRange.End.Value );
            }

            // Filter by role
            var roles = cblRole.SelectedValues.AsIntegerList();

            if ( roles.Any() )
            {
                qryGroupMembers = qryGroupMembers.Where( m => roles.Contains( m.GroupRoleId ) );
            }

            // Filter by Group Member Status
            var statuses = cblGroupMemberStatus.SelectedValues.Select( a => a.ConvertToEnumOrNull<GroupMemberStatus>() ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();

            if ( statuses.Any() )
            {
                qryGroupMembers = qryGroupMembers.Where( m => statuses.Contains( m.GroupMemberStatus ) );
            }

            var sortProperty = gGroupMembers.SortProperty;
            if ( sortProperty != null )
            {
                qryGroupMembers = qryGroupMembers.Sort( sortProperty );
            }
            else
            {
                qryGroupMembers = qryGroupMembers.OrderBy( a => a.GroupRole.Order ).ThenBy( a => a.Person.LastName ).ThenBy( a => a.Person.FirstName );
            }

            qryGroupMembers = qryGroupMembers.Include( a => a.Person );

            gGroupMembers.SetLinqDataSource( qryGroupMembers );
            gGroupMembers.DataBind();
        }

        /// <summary>
        /// Shows the group member history.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void ShowGroupMemberHistory( int groupMemberId )
        {
            var groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId );
            if ( groupMember == null )
            {
                this.Visible = false;
                return;
            }

            lGroupTitle.Text = groupMember.Group.Name.FormatAsHtmlTitle();
            lGroupMemberTitle.Text = groupMember.ToString().FormatAsHtmlTitle();
            lGroupMemberPreHtml.Visible = true;
            lGroupMemberTitle.Visible = true;
            lGroupGridTitle.Visible = false;

            var rockContext = new RockContext();
            var historyService = new HistoryService( rockContext );

            var additionalMergeFields = new Dictionary<string, object>();
            additionalMergeFields.Add( "GroupHistoryGridPage", LinkedPageRoute( "GroupHistoryGridPage" ) );
            additionalMergeFields.Add( "GroupMemberHistoryPage", LinkedPageRoute( "GroupMemberHistoryPage" ) );

            string timelineLavaTemplate = this.GetAttributeValue( "TimelineLavaTemplate" );

            string timelineHtml = historyService.GetTimelineHtml( timelineLavaTemplate, EntityTypeCache.Get<Rock.Model.GroupMember>(), groupMemberId, null, additionalMergeFields, CurrentPerson );
            lTimelineHtml.Text = "<div class='panel-body'>" + timelineHtml + "</div>";
        }

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            this.Visible = visible;
        }

        #endregion ISecondaryBlock

        #endregion
    }
}