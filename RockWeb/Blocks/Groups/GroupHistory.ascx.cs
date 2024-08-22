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
using System.Web.UI;

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
    [DisplayName( "Group History" )]
    [Category( "Groups" )]
    [Description( "Displays a timeline of history for a group" )]

    [CodeEditorField( "Timeline Lava Template", "The Lava Template to use when rendering the timeline view of the history.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}", order: 1 )]
    [LinkedPage( "Group History Grid Page", defaultValue: Rock.SystemGuid.Page.GROUP_HISTORY_GRID, required: true, order: 2 )]
    [LinkedPage( "Group Member History Page", defaultValue: Rock.SystemGuid.Page.GROUP_MEMBER_HISTORY, required: true, order: 3 )]
    [Rock.SystemGuid.BlockTypeGuid( "E916D65E-5D30-4086-9A11-8E891CCD930E" )]
    public partial class GroupHistory : RockBlock, ICustomGridColumns
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
                if ( groupId.HasValue )
                {
                    ShowDetail( groupId.Value );
                }
                else
                {
                    // don't show the block if a GroupId isn't in the URL
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
            ShowGroupHistory( hfGroupId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglShowGroupMembersInHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglShowGroupMembersInHistory_CheckedChanged( object sender, EventArgs e )
        {
            ShowGroupHistory( hfGroupId.Value.AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void ShowDetail( int groupId )
        {
            hfGroupId.Value = groupId.ToString();

            ShowGroupHistory( groupId );
        }

        /// <summary>
        /// Shows the group history.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        private void ShowGroupHistory( int groupId )
        {
            int entityId;
            EntityTypeCache primaryEntityType;
            EntityTypeCache secondaryEntityType = null;

            hlMemberHistory.NavigateUrl = LinkedPageUrl( "GroupMemberHistoryPage", new Dictionary<string, string> { { "GroupId", groupId.ToString() } } );

            primaryEntityType = EntityTypeCache.Get<Rock.Model.Group>();
            entityId = groupId;
            var group = new GroupService( new RockContext() ).Get( groupId );
            if ( group != null )
            {
                lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            }

            if ( tglShowGroupMembersInHistory.Checked )
            {
                secondaryEntityType = EntityTypeCache.Get<Rock.Model.GroupMember>();
            }

            var rockContext = new RockContext();
            var historyService = new HistoryService( rockContext );
            string timelineLavaTemplate = this.GetAttributeValue( "TimelineLavaTemplate" );
            var additionalMergeFields = new Dictionary<string, object>();
            additionalMergeFields.Add( "GroupHistoryGridPage", LinkedPageRoute( "GroupHistoryGridPage" ) );
            additionalMergeFields.Add( "GroupMemberHistoryPage", LinkedPageRoute( "GroupMemberHistoryPage" ) );

            string timelineHtml = historyService.GetTimelineHtml( timelineLavaTemplate, primaryEntityType, entityId, secondaryEntityType, additionalMergeFields, CurrentPerson );
            lTimelineHtml.Text = timelineHtml;
        }

        #endregion
    }
}