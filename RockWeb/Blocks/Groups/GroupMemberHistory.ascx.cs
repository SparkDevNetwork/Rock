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
using Rock.Cache;
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
    [Description( "Displays a timeline of history for a group member" )]

    [CodeEditorField( "Timeline Lava Template", "The Lava Template to use when rendering the timeline view of the history.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}", order: 1 )]
    [LinkedPage( "Group History Grid Page", required: true, order: 2 )]
    [LinkedPage( "Group Member History Page", required: true, order: 3 )]
    public partial class GroupMemberHistory : RockBlock, ICustomGridColumns
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
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );
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
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();
                if ( !groupId.HasValue )
                {
                    if ( groupMemberId.HasValue )
                    {
                        var groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId.Value );
                        if ( groupMember != null )
                        {
                            groupId = groupMember.GroupId;
                        }
                    }
                }

                if ( groupId.HasValue )
                {
                    ShowDetail( groupId.Value, groupMemberId );
                }
                else
                {
                    pnlMembers.Visible = false;
                }
            }
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

            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "First Name" ), "First Name", tbFirstName.Text );
            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "Last Name" ), "Last Name", tbLastName.Text );
            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "Group Role" ), "Last Role", cblRole.SelectedValues.AsIntegerList().ToJson() );
            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "Group Member Status" ), "Last Status", cblGroupMemberStatus.SelectedValues.AsIntegerList().ToJson() );
            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "Date Added" ), "Date Added", sdrDateAdded.DelimitedValues );
            gfGroupMembers.SaveUserPreference( MakeKeyUniqueToGroup( groupId, "Date Removed" ), "Date Removed", sdrDateRemoved.DelimitedValues );

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
            if ( e.Key == MakeKeyUniqueToGroup( groupId, "Group Role" ) )
            {
                List<int> selectedGroupRoleIds = e.Value.FromJsonOrNull<List<int>>() ?? new List<int>();
                e.Value = cblRole.Items.OfType<ListItem>().Where( a => selectedGroupRoleIds.Contains( a.Value.AsInteger() ) ).Select( a => a.Text ).ToList().AsDelimited( ", ", " or " );
            }
            else if ( e.Key == MakeKeyUniqueToGroup( groupId, "Group Member Status" ) )
            {
                List<int> selectedGroupMemberStatuses = e.Value.FromJsonOrNull<List<int>>() ?? new List<int>();
                e.Value = cblGroupMemberStatus.Items.OfType<ListItem>().Where( a => selectedGroupMemberStatuses.Contains( a.Value.AsInteger() ) ).Select( a => a.Text ).ToList().AsDelimited( ", ", " or " );
            }
            else if ( e.Key == MakeKeyUniqueToGroup( groupId, "Date Added" ) || e.Key == MakeKeyUniqueToGroup( groupId, "Date Removed" ) )
            {
                e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToGroup( groupId, "First Name" ) || e.Key == MakeKeyUniqueToGroup( groupId, "Last Name" ) )
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
            gfGroupMembers.DeleteUserPreferences();
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
                lPersonNameHtml.Text = string.Format( photoFormat, groupMember.PersonId, groupMember.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) )
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

            if ( groupMemberId.HasValue )
            {
                ShowGroupMemberHistory( groupMemberId.Value );
            }
            else
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
            var groupTypeCache = CacheGroupType.Get( group.GroupTypeId );

            tbFirstName.Text = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "First Name" ) );
            tbLastName.Text = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "Last Name" ) );

            sdrDateAdded.DelimitedValues = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "Date Added" ) );
            sdrDateRemoved.DelimitedValues = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "Date Removed" ) );
            cblRole.Items.Clear();

            if ( groupTypeCache != null )
            {
                foreach ( var role in groupTypeCache.Roles )
                {
                    cblRole.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
                }
            }

            List<int> selectedGroupRoleIds = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "Group Role" ) ).FromJsonOrNull<List<int>>() ?? new List<int>();
            cblRole.SetValues( selectedGroupRoleIds );

            cblGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            List<GroupMemberStatus> selectedGroupMemberStatuses = gfGroupMembers.GetUserPreference( MakeKeyUniqueToGroup( groupId, "Group Member Status" ) ).FromJsonOrNull<List<GroupMemberStatus>>() ?? new List<GroupMemberStatus>();
            cblGroupMemberStatus.SetValues( selectedGroupMemberStatuses.Select( a => a.ConvertToInt() ) );
        }

        /// <summary>
        /// Makes the key unique to group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToGroup( int groupId, string key )
        {
            return string.Format( "{0}-{1}", groupId, key );
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
            int entityId;
            CacheEntityType primaryEntityType;

            primaryEntityType = CacheEntityType.Get<Rock.Model.GroupMember>();
            entityId = groupMemberId;

            var groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId );
            if ( groupMember != null )
            {
                lGroupTitle.Text = groupMember.Group.Name.FormatAsHtmlTitle();
                lGroupMemberTitle.Text = groupMember.ToString().FormatAsHtmlTitle();
                lGroupMemberPreHtml.Visible = true;
                lGroupMemberTitle.Visible = true;
                lGroupGridTitle.Visible = false;
            }

            var rockContext = new RockContext();
            var historyService = new HistoryService( rockContext );

            var additionalMergeFields = new Dictionary<string, object>();
            additionalMergeFields.Add( "GroupHistoryGridPage", LinkedPageRoute( "GroupHistoryGridPage" ) );
            additionalMergeFields.Add( "GroupMemberHistoryPage", LinkedPageRoute( "GroupMemberHistoryPage" ) );

            string timelineLavaTemplate = this.GetAttributeValue( "TimelineLavaTemplate" );
            string timelineHtml = historyService.GetTimelineHtml( timelineLavaTemplate, primaryEntityType, entityId, null, additionalMergeFields );
            lTimelineHtml.Text = "<div class='panel-body'>" + timelineHtml +  "</div>";
        }

        #endregion
    }
}