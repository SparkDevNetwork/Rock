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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// A Data Filter to select Groups that have at least one opportunity (GroupLocationScheduleConfig), with no corresponding group member assignment records.
    /// </summary>
    [Description( "Lists any groups (projects) that have at least one opportunity (GroupLocationScheduleConfig), with no corresponding group member assignment records." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Sign-Up Opportunity Has No Members" )]
    [Rock.SystemGuid.EntityTypeGuid( "92E7406D-723A-4302-A36A-83374E706160" )]
    public class SignUpOpportunityHasNoMembersFilter : DataFilterComponent
    {
        #region Constants

        private class MemberTypeValue
        {
            public const string Leader = "Leader";
            public const string NotLeader = "NotLeader";
        }

        #endregion

        #region Settings

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        private class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
                // Add values to set defaults / populate upon object creation.
            }

            /// <summary>
            /// The selected <see cref="Rock.Model.GroupType"/> identifier.
            /// </summary>
            public string GroupTypeGuid { get; set; }

            /// <summary>
            /// The selected member type.
            /// </summary>
            public string MemberType { get; set; }

            /// <summary>
            /// Whether to hide past opportunities.
            /// </summary>
            public bool HidePastOpportunities { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON string.
            /// </summary>
            /// <param name="selection">The filter selection control.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.Group ).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Member Filters"; }
        }

        /// <summary>
        /// Gets the Sign-Up Group GroupType identifier.
        /// </summary>
        /// <value>
        /// The Sign-Up Group GroupType identifier.
        /// </value>
        private int SignUpGroupGroupTypeId
        {
            get { return GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Sign-Up Opportunity Has No Members";
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Define control: GroupType picker.
            var groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.GetChildControlInstanceName( "_groupTypePicker" );
            groupTypePicker.Label = "Group Type";
            groupTypePicker.AddCssClass( "js-group-type-picker" );
            groupTypePicker.UseGuidAsValue = true;
            groupTypePicker.Required = true;
            using ( var rockContext = new RockContext() )
            {
                groupTypePicker.GroupTypes = new GroupTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( gt => gt.Id == this.SignUpGroupGroupTypeId || gt.InheritedGroupTypeId == this.SignUpGroupGroupTypeId )
                    .ToList();
            }
            filterControl.Controls.Add( groupTypePicker );

            // Define control: member type drop down list.
            var ddlMemberType = new RockDropDownList();
            ddlMemberType.ID = filterControl.GetChildControlInstanceName( "_ddlMemberType" );
            ddlMemberType.Label = "Member Type";
            ddlMemberType.Help = "The member type to be considered for this filter.";
            ddlMemberType.AddCssClass( "js-ddl-member-type" );
            ddlMemberType.Items.Add( new ListItem( string.Empty, string.Empty ) );
            ddlMemberType.Items.Add( new ListItem( "Leader", MemberTypeValue.Leader ) );
            ddlMemberType.Items.Add( new ListItem( "Not Leader", MemberTypeValue.NotLeader ) );
            filterControl.Controls.Add( ddlMemberType );

            // Define control: hide past opportunities check box.
            var cbHidePastOpportunities = new RockCheckBox();
            cbHidePastOpportunities.ID = filterControl.GetChildControlInstanceName( "_cbHidePastOpportunities" );
            cbHidePastOpportunities.Label = "Hide Past Opportunities";
            cbHidePastOpportunities.AddCssClass( "js-cb-hide-past-opportunities" );
            filterControl.Controls.Add( cbHidePastOpportunities );

            return new Control[] { groupTypePicker, ddlMemberType, cbHidePastOpportunities };
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            // Get references to the controls we created in CreateChildControls.
            var groupTypePicker = controls[0] as GroupTypePicker;
            var ddlMemberType = controls[1] as RockDropDownList;
            var cbHidePastOpportunities = controls[2] as RockCheckBox;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Render GroupType picker control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            groupTypePicker.RenderControl( writer );
            writer.RenderEndTag();

            // Render member type drop down list control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlMemberType.RenderControl( writer );
            writer.RenderEndTag();

            // Render hide past opportunities check box control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbHidePastOpportunities.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }
#endif

        /// <summary>
        /// Formats the selection on the client-side. When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property. If including script, the
        /// control's parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var groupTypeName = $('.js-group-type-picker', $content).find(':selected').text();
    var memberType = $('.js-ddl-member-type', $content).find(':selected').text();
    var hidePastOpportunities = $('.js-cb-hide-past-opportunities', $content).is(':checked');

    var result = '';

    if (hidePastOpportunities) {
        result = 'Current ';
    } else {
        result = 'Current or past ';
    }

    result += groupTypeName + ' opportunity has no';

    if (memberType === 'Leader') {
        result += ' leaders';
    } else if (memberType === 'Not Leader') {
        result += ' non-leader members';
    } else {
        result += ' members';
    }

    return result;
}";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns>
        /// The formatted selection.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var timeframe = selectionConfig.HidePastOpportunities
                ? "Current"
                : "Current or past";

            string groupTypeName = null;
            using ( var rockContext = new RockContext() )
            {
                var groupTypeGuid = selectionConfig.GroupTypeGuid.AsGuidOrNull();
                if ( groupTypeGuid.HasValue )
                {
                    groupTypeName = new GroupTypeService( rockContext )
                        .GetNoTracking( selectionConfig.GroupTypeGuid.AsGuid() )
                        ?.Name;
                }
            }

            var memberType = "members";
            if ( selectionConfig.MemberType == MemberTypeValue.Leader )
            {
                memberType = "leaders";
            }
            else if ( selectionConfig.MemberType == MemberTypeValue.NotLeader )
            {
                memberType = "non-leader members";
            }

            return $"{timeframe} {groupTypeName} opportunity has no {memberType}";
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Returns a JSON representation of selected values.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns>A JSON representation of selected values.</returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var groupTypePicker = controls[0] as GroupTypePicker;
            var ddlMemberType = controls[1] as RockDropDownList;
            var cbHidePastOpportunities = controls[2] as RockCheckBox;

            var selectionConfig = new SelectionConfig
            {
                GroupTypeGuid = groupTypePicker.SelectedValue,
                MemberType = ddlMemberType.SelectedValue,
                HidePastOpportunities = cbHidePastOpportunities.Checked
            };

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection from a JSON string.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var groupTypePicker = controls[0] as GroupTypePicker;
            var ddlMemberType = controls[1] as RockDropDownList;
            var cbHidePastOpportunities = controls[2] as RockCheckBox;

            groupTypePicker.SetValue( selectionConfig.GroupTypeGuid );
            ddlMemberType.SetValue( selectionConfig.MemberType );
            cbHidePastOpportunities.Checked = selectionConfig.HidePastOpportunities;
        }
#endif

        /// <summary>
        /// Creates a LINQ Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );
            var requireLeader = selectionConfig.MemberType == MemberTypeValue.Leader;
            var requireNonLeader = selectionConfig.MemberType == MemberTypeValue.NotLeader;
            var hidePastOpportunities = selectionConfig.HidePastOpportunities;

            var groupTypeGuid = selectionConfig.GroupTypeGuid.AsGuidOrNull();
            if ( !groupTypeGuid.HasValue )
            {
                return null;
            }

            var groupTypeId = GroupTypeCache.GetId( groupTypeGuid.Value );
            if ( !groupTypeId.HasValue )
            {
                return null;
            }

            // We'll use today's date @ 12:00 AM to compare against Schedule.EffectiveEndDate below, in order to filter out past opportunities if needed.
            var startOfToday = RockDateTime.Now.StartOfDay();

            var membersByOpportunityQuery = new GroupLocationService( ( RockContext ) serviceInstance.Context )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.Group.GroupTypeId == groupTypeId.Value )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.LocationId,
                    Schedule = s
                } )
                /*
                 * We now have GroupLocationSchedule instances:
                 * One for each combination of Group, Location & Schedule, where [Group].[GroupTypeId] == selectionConfig's GroupTypeId.
                 */
                .Where( gls =>
                    !hidePastOpportunities
                    || (
                        gls.Schedule.IsActive
                        && (
                            !gls.Schedule.EffectiveEndDate.HasValue
                            || gls.Schedule.EffectiveEndDate.Value >= startOfToday
                        )
                    )
                )
                /*
                 * If indicated by selectionConfig.HidePastOpportunities, we've now filtered out any past opportunities.
                 * If not, we have all opportunities (GroupLocationSchedules): past, present and future.
                 */
                .Select( gls => new
                {
                    GroupId = gls.Group.Id,
                    gls.LocationId,
                    ScheduleId = gls.Schedule.Id,
                    Members = gls.Group.Members
                        .SelectMany( gm => gm.GroupMemberAssignments, ( gm, gma ) => new
                        {
                            GroupMember = gm,
                            Assignment = gma
                        } )
                        .Where( gmas =>
                            !gmas.GroupMember.Person.IsDeceased
                            && gmas.Assignment.LocationId == gls.LocationId
                            && gmas.Assignment.ScheduleId == gls.Schedule.Id
                        )
                        .Select( gmas => new
                        {
                            gmas.GroupMember,
                            gmas.GroupMember.GroupRole.IsLeader
                        } )
                } )
                /*
                 * We now have a collection of members (if any), as well as which members are leaders, grouped by opportunity.
                 */
                .Select( membersByOpportunity => new
                {
                    membersByOpportunity.GroupId,
                    membersByOpportunity.LocationId,
                    membersByOpportunity.ScheduleId,
                    LeaderCount = membersByOpportunity.Members.Count( m => m.IsLeader ),
                    NonLeaderCount = membersByOpportunity.Members.Count( m => !m.IsLeader ),
                    TotalMemberCount = membersByOpportunity.Members.Count()
                } );

            /*
             * And lastly, we have counts of leaders, non-leaders & total members, grouped by opportunity.
             * We'll be able to perform the final filtering against this data.
             */

            var query = new GroupService( ( RockContext ) serviceInstance.Context )
                .Queryable()
                .AsNoTracking();

            if ( requireLeader )
            {
                query = query.Where( g =>
                    membersByOpportunityQuery.Any( m =>
                        g.Id == m.GroupId
                        && m.LeaderCount == 0
                    )
                );
            }
            else if ( requireNonLeader )
            {
                query = query.Where( g =>
                    membersByOpportunityQuery.Any( m =>
                        g.Id == m.GroupId
                        && m.NonLeaderCount == 0
                    )
                );
            }
            else
            {
                query = query.Where( g =>
                    membersByOpportunityQuery.Any( m =>
                        g.Id == m.GroupId
                        && m.TotalMemberCount == 0
                    )
                );
            }

            return FilterExpressionExtractor.Extract<Rock.Model.Group>( query, parameterExpression, "g" );
        }

        #endregion
    }
}
