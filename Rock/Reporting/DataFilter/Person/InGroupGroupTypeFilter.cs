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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are in a group of a specific group type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group of Group Type Filter" )]
    public class InGroupGroupTypeFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

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
            return "In Group of Group Type";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var groupTypeName = $('.group-type-picker', $content).find(':selected').text()
  var checkedRoles = $('.rock-check-box-list', $content).find(':checked').closest('label');
  var result = 'In group of group type: ' + groupTypeName;
  if (checkedRoles.length > 0) {
     var roleCommaList = checkedRoles.map(function() { return $(this).text() }).get().join(',');
     result = result + ', with role(s): ' + roleCommaList;
  }

  var groupStatus = $('.js-group-status option:selected', $content).text();
  if (groupMemberStatus) {
     result = result + ', with group status:' + groupStatus;
  }

  var groupMemberStatus = $('.js-group-member-status option:selected', $content).text();
  if (groupMemberStatus) {
     result = result + ', with member status:' + groupMemberStatus;
  }

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Group Member";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var groupType = GroupTypeCache.Get( selectionValues[0].AsGuid() );

                var groupTypeRoleGuidList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService( new RockContext() ).Queryable().Where( a => groupTypeRoleGuidList.Contains( a.Guid ) ).ToList();

                bool? groupStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupStatus = selectionValues[3].AsBooleanOrNull();
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 3 )
                {
                    groupMemberStatus = selectionValues[2].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( groupType != null )
                {
                    result = string.Format( "In group of group type: {0}", groupType.Name );
                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => a.Name ).ToList().AsDelimited( "," ) );
                    }

                    if ( groupStatus.HasValue )
                    {
                        result += string.Format( ", with group status: {0}", groupStatus.Value ? "Active" : "Inactive" );
                    }

                    if ( groupMemberStatus.HasValue )
                    {
                        result += string.Format( ", with member status: {0}", groupMemberStatus.ConvertToString() );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.ID + "_groupTypePicker";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            groupTypePicker.CssClass = "js-grouptype-picker";
            filterControl.Controls.Add( groupTypePicker );

            var cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Role(s)";
            cblRole.CssClass = "js-group-roles";
            cblRole.ID = filterControl.ID + "_cblRole";
            filterControl.Controls.Add( cblRole );

            PopulateGroupRolesCheckList( filterControl );

            RockDropDownList ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = filterControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Select a specific group member status to only include group members with that status. Leaving this blank will return all members.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            filterControl.Controls.Add( ddlGroupMemberStatus );

            RockDropDownList ddlGroupStatus = new RockDropDownList();
            ddlGroupStatus.CssClass = "js-group-status";
            ddlGroupStatus.ID = filterControl.ID + "_ddlGroupStatus";
            ddlGroupStatus.Label = "with Group Status";
            ddlGroupStatus.Items.Insert( 0, new ListItem( "[All]", "" ) );
            ddlGroupStatus.Items.Insert( 1, new ListItem( "Active", "True" ) );
            ddlGroupStatus.Items.Insert( 2, new ListItem( "Inactive", "False" ) );
            ddlGroupStatus.SetValue( true.ToString() );
            filterControl.Controls.Add( ddlGroupStatus );

            return new Control[4] { groupTypePicker, cblRole, ddlGroupStatus, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            PopulateGroupRolesCheckList( filterField );
        }

        /// <summary>
        /// Populates the group roles.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        private void PopulateGroupRolesCheckList( FilterField filterField )
        {
            var groupTypePicker = filterField.ControlsOfTypeRecursive<GroupTypePicker>().FirstOrDefault( a => a.HasCssClass( "js-grouptype-picker" ) );
            var cblRole = filterField.ControlsOfTypeRecursive<RockCheckBoxList>().FirstOrDefault( a => a.HasCssClass( "js-group-roles" ) );
            int? groupTypeId = groupTypePicker.SelectedValueAsId();

            if ( groupTypeId.HasValue )
            {
                cblRole.Items.Clear();
                foreach ( var item in new GroupTypeRoleService( new RockContext() ).GetByGroupTypeId( groupTypeId.Value ) )
                {
                    cblRole.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                cblRole.Visible = cblRole.Items.Count > 0;
            }
            else
            {
                cblRole.Visible = false;
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var groupTypePicker = ( controls[0] as GroupTypePicker );
            var cblRoles = ( controls[1] as RockCheckBoxList );
            var ddlGroupStatus = ( controls[2] as RockDropDownList );
            var ddlMemberStatus = ( controls[3] as RockDropDownList );


            int groupTypeId = groupTypePicker.SelectedValueAsId() ?? 0;
            Guid? groupTypeGuid = null;
            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType != null )
            {
                groupTypeGuid = groupType.Guid;
            }

            var rolesGuidCommaList = cblRoles.SelectedValues.AsDelimited( "," );

            var memberStatusValue = ddlMemberStatus.SelectedValue;

            var groupStatus = ddlGroupStatus.SelectedValue;

            return groupTypeGuid.ToString() + "|" + rolesGuidCommaList + "|" + memberStatusValue + "|" + groupStatus;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Guid groupTypeGuid = selectionValues[0].AsGuid();
                var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeGuid );
                var groupTypePicker = ( controls[0] as GroupTypePicker );
                if ( groupType != null )
                {
                    groupTypePicker.SetValue( groupType.Id );
                }

                groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );

                string[] selectedRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                RockCheckBoxList cblRole = controls[1] as RockCheckBoxList;

                foreach ( var item in cblRole.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleGuids.Contains( item.Value );
                }

                RockDropDownList ddlGroupStatus = controls[2] as RockDropDownList;
                if ( selectionValues.Length >= 4 )
                {
                    ddlGroupStatus.SetValue( selectionValues[3] );
                }
                else
                {
                    ddlGroupStatus.SetValue( string.Empty );
                }

                RockDropDownList ddlGroupMemberStatus = controls[3] as RockDropDownList;
                if ( selectionValues.Length >= 3 )
                {
                    ddlGroupMemberStatus.SetValue( selectionValues[2] );
                }
                else
                {
                    ddlGroupMemberStatus.SetValue( string.Empty );
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                GroupMemberService groupMemberService = new GroupMemberService( ( RockContext ) serviceInstance.Context );
                int groupTypeId = 0;

                Guid groupTypeGuid = selectionValues[0].AsGuid();
                var groupType = GroupTypeCache.Get( groupTypeGuid );
                if ( groupType != null )
                {
                    groupTypeId = groupType.Id;
                }

                var groupMemberServiceQry = groupMemberService.Queryable( true ).Where( xx => xx.Group.GroupTypeId == groupTypeId && xx.Group.IsArchived != true );

                bool? groupStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupStatus = selectionValues[3].AsBooleanOrNull();
                }

                if ( groupStatus.HasValue )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.Group.IsActive == groupStatus.Value );
                }

                var groupRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                if ( groupRoleGuids.Count() > 0 )
                {
                    var groupRoleIds = new GroupTypeRoleService( ( RockContext ) serviceInstance.Context ).Queryable().Where( a => groupRoleGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupRoleIds.Contains( xx.GroupRoleId ) );
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 3 )
                {
                    groupMemberStatus = selectionValues[2].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( groupMemberStatus.HasValue )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.GroupMemberStatus == groupMemberStatus.Value );
                }

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}