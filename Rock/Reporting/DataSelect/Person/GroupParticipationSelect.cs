﻿// <copyright>
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    ///     A Report Field that shows the list of Groups in which a Person is participating from a set of candidates defined by
    ///     a Group Data View.
    /// </summary>
    [Description( "Shows a summary of Groups in which a Person participates from a filtered subset of Groups defined by a Data View" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Group Participation" )]
    public class GroupParticipationSelect : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Groups"; }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get { return "Group Participation"; }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( IEnumerable<string> ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
        {
            return new ListDelimitedField();
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get { return "Group Participation"; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Group Participation";
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            // since this returns an Enumerable, it doesn't really make sense to sort it
            // return string.Empty to disable sorting
            return string.Empty;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var settings = new GroupParticipationSelectSettings( selection );

            //
            // Define Candidate Groups.
            //

            // Get the Group Data View that defines the set of candidates from which matching Groups can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the candidate Groups.
            var groupService = new GroupService( context );

            var groupQuery = groupService.Queryable();

            if (dataView != null)
            {
                groupQuery = DataComponentSettingsHelper.FilterByDataView( groupQuery, dataView, groupService );
            }
            else
            {
                // Apply a default Group filter to only show Groups that would be visible in a Group List.
                groupQuery = groupQuery.Where( x => x.GroupType.ShowInGroupList );
            }

            var groupKeys = groupQuery.Select( x => x.Id );

            //
            // Construct the Query to return the list of Group Members matching the filter conditions.
            //
            var groupMemberQuery = new GroupMemberService( context ).Queryable();

            // Filter By Group.
            groupMemberQuery = groupMemberQuery.Where( x => groupKeys.Contains( x.GroupId ) );

            // Filter By Group Role Type.
            switch ( settings.RoleType )
            {
                case RoleTypeSpecifier.Member:
                    groupMemberQuery = groupMemberQuery.Where( x => !x.GroupRole.IsLeader );
                    break;

                case RoleTypeSpecifier.Leader:
                    groupMemberQuery = groupMemberQuery.Where( x => x.GroupRole.IsLeader );
                    break;
            }

            // Filter by Group Member Status.
            if ( settings.MemberStatus.HasValue )
            {
                groupMemberQuery = groupMemberQuery.Where( x => x.GroupMemberStatus == settings.MemberStatus.Value );
            }

            //
            // Create a Select Expression to return the requested values.
            //

            // Set the Output Format of the field.
            Expression selectExpression;

            if (settings.ListFormat == ListFormatSpecifier.YesNo)
            {
                // Define a Query to return True/False text indicating if the Person participates in any of the filtered Groups.
                // Note that the text must be returned as an Enumerable to satisfy the expected output of this field.
                var personGroupsQuery = new PersonService( context ).Queryable()
                                                                    .Select( p => new List<string> { groupMemberQuery.Any( s => s.PersonId == p.Id ) ? "Yes" : "No" } );

                selectExpression = SelectExpressionExtractor.Extract( personGroupsQuery, entityIdProperty, "p" );
            }
            else
            {
                // Define a Query to return the collection of filtered Groups for each Person.
                Expression<Func<Rock.Model.GroupMember, string>> outputExpression;

                if (settings.ListFormat == ListFormatSpecifier.GroupOnly)
                {
                    outputExpression = ( ( m => m.Group.Name ) );
                }
                else
                {
                    outputExpression = ( ( m => m.Group.Name + " [" + m.GroupRole.Name + "]" ) );
                }

                // Define a Query to return the collection of filtered Groups for each Person.
                var personGroupsQuery = new PersonService( context ).Queryable()
                                                                    .Select( p => groupMemberQuery.Where( s => s.PersonId == p.Id )
                                                                                                  .OrderBy( x => x.Group.Name )
                                                                                                  .ThenBy( x => x.GroupRole.Name )
                                                                                                  .Select( outputExpression ).AsEnumerable() );

                selectExpression = SelectExpressionExtractor.Extract( personGroupsQuery, entityIdProperty, "p" );
            }

            return selectExpression;
        }

        private const string _CtlFormat = "ddlFormat";
        private const string _CtlDataView = "ddlDataView";
        private const string _CtlRoleType = "ddlRoleType";
        private const string _CtlGroupStatus = "ddlGroupStatus";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Control parentControl )
        {
            // Define Control: Output Format DropDown List
            var ddlFormat = new RockDropDownList();
            ddlFormat.ID = parentControl.GetChildControlInstanceName( _CtlFormat );
            ddlFormat.Label = "Output Format";
            ddlFormat.Help = "Specifies the content and format of the values in this field.";
            ddlFormat.Items.Add( new ListItem( "Group List: Name And Role", ListFormatSpecifier.GroupAndRole.ToString() ) );
            ddlFormat.Items.Add( new ListItem( "Group List: Group Name", ListFormatSpecifier.GroupOnly.ToString() ) );
            ddlFormat.Items.Add( new ListItem( "Yes/No: Yes if any participation", ListFormatSpecifier.YesNo.ToString() ) );

            parentControl.Controls.Add( ddlFormat );

            // Define Control: Group Data View Picker
            var ddlDataView = new DataViewPicker();
            ddlDataView.ID = parentControl.GetChildControlInstanceName( _CtlDataView );
            ddlDataView.Label = "Participates in Groups";
            ddlDataView.Help = "A Data View that filters the Groups included in the result. If no value is selected, any Groups that would be visible in a Group List will be included.";
            parentControl.Controls.Add( ddlDataView );

            // Define Control: Role Type DropDown List
            var ddlRoleType = new RockDropDownList();
            ddlRoleType.ID = parentControl.GetChildControlInstanceName( _CtlRoleType );
            ddlRoleType.Label = "with Group Member Type";
            ddlRoleType.Help = "Specifies the type of Group Role the Member must have to be included in the result. If no value is selected, Members in any Role will be included.";
            ddlRoleType.Items.Add( new ListItem( string.Empty, RoleTypeSpecifier.Any.ToString() ) );
            ddlRoleType.Items.Add( new ListItem( "Leader", RoleTypeSpecifier.Leader.ToString() ) );
            ddlRoleType.Items.Add( new ListItem( "Member", RoleTypeSpecifier.Member.ToString() ) );
            parentControl.Controls.Add( ddlRoleType );

            // Define Control: Group Member Status DropDown List
            var ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = parentControl.GetChildControlInstanceName( _CtlGroupStatus );
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Specifies the Status the Member must have to be included in the result. If no value is selected, Members of any Group Status will be shown.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            parentControl.Controls.Add( ddlGroupMemberStatus );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Read( typeof( Model.Group ) ).Id;
            ddlDataView.EntityTypeId = entityTypeId;

            return new Control[] { ddlDataView, ddlRoleType, ddlFormat, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Gets the selection.
        /// This is typically a string that contains the values selected with the Controls
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            var ddlDataView = controls.GetByName<DataViewPicker>( _CtlDataView );
            var ddlRoleType = controls.GetByName<RockDropDownList>( _CtlRoleType );
            var ddlFormat = controls.GetByName<RockDropDownList>( _CtlFormat );
            var ddlGroupMemberStatus = controls.GetByName<RockDropDownList>( _CtlGroupStatus );

            var settings = new GroupParticipationSelectSettings();

            settings.MemberStatus = ddlGroupMemberStatus.SelectedValue.ConvertToEnumOrNull<GroupMemberStatus>();
            settings.RoleType = ddlRoleType.SelectedValue.ConvertToEnumOrNull<RoleTypeSpecifier>();
            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( ddlDataView.SelectedValue );
            settings.ListFormat = ddlFormat.SelectedValue.ConvertToEnum<ListFormatSpecifier>( ListFormatSpecifier.GroupAndRole );

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            var ddlDataView = controls.GetByName<DataViewPicker>( _CtlDataView );
            var ddlRoleType = controls.GetByName<RockDropDownList>( _CtlRoleType );
            var ddlFormat = controls.GetByName<RockDropDownList>( _CtlFormat );
            var ddlGroupMemberStatus = controls.GetByName<RockDropDownList>( _CtlGroupStatus );

            var settings = new GroupParticipationSelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlFormat.SelectedValue = settings.ListFormat.ToString();

            if ( settings.DataViewGuid.HasValue )
            {
                var dsService = new DataViewService( new RockContext() );

                var dataView = dsService.Get( settings.DataViewGuid.Value );

                if ( dataView != null )
                {
                    ddlDataView.SelectedValue = dataView.Id.ToString();
                }
            }

            ddlRoleType.SelectedValue = settings.RoleType.ToStringSafe();
            ddlGroupMemberStatus.SelectedValue = settings.MemberStatus.ToStringSafe();
        }

        #endregion

        #region Settings

        private enum RoleTypeSpecifier
        {
            Any = 0,
            Leader = 1,
            Member = 2
        }

        private enum ListFormatSpecifier
        {
            GroupAndRole = 0,
            GroupOnly = 1,
            YesNo = 2
        }

        /// <summary>
        ///     Settings for the Data Select Component "Group Participation".
        /// </summary>
        private class GroupParticipationSelectSettings : SettingsStringBase
        {
            public Guid? DataViewGuid;
            public ListFormatSpecifier ListFormat = ListFormatSpecifier.GroupAndRole;
            public GroupMemberStatus? MemberStatus = GroupMemberStatus.Active;
            public RoleTypeSpecifier? RoleType;

            public GroupParticipationSelectSettings()
            {
                //
            }

            public GroupParticipationSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                ListFormat = DataComponentSettingsHelper.GetParameterAsEnum( parameters, 0, ListFormatSpecifier.GroupAndRole );
                DataViewGuid = DataComponentSettingsHelper.GetParameterOrDefault( parameters, 1, string.Empty ).AsGuidOrNull();
                RoleType = DataComponentSettingsHelper.GetParameterAsEnum<RoleTypeSpecifier>( parameters, 2 );
                MemberStatus = DataComponentSettingsHelper.GetParameterAsEnum<GroupMemberStatus>( parameters, 3 );
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( ( (int)ListFormat ).ToString() );
                settings.Add( DataViewGuid.ToStringSafe() );
                settings.Add( RoleType == null ? string.Empty : ( (int)RoleType ).ToString() );
                settings.Add( MemberStatus == null ? string.Empty : ( (int)MemberStatus ).ToString() );

                return settings;
            }
        }

        #endregion
    }
}