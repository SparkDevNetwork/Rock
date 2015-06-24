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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they are in the specified group or groups" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group(s) Filter" )]
    public class InGroupFilter : DataFilterComponent
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
            return "In Group(s)";
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
            return GetGroupFilterClientSelection( false );
        }

        /// <summary>
        /// Gets the group filter client selection based on if we are in "Not" mode
        /// </summary>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GetGroupFilterClientSelection( bool not )
        {
            string format = @"
function() {{
  var groupNames = $('.js-group-picker', $content).find('.selected-names').text();
  var checkedRoles = $('.js-roles', $content).find(':checked').closest('label');
  var result = '{0} ' + groupNames;
  var includeChildGroups = $('.js-include-child-groups', $content).is(':checked');
  if (includeChildGroups) {{
    var includeDescendantGroups = $('.js-include-child-groups-descendants', $content).is(':checked');    
    var includeSelectedGroups = $('.js-include-selected-groups', $content).is(':checked');
    if (includeDescendantGroups) {{
        result = result + ' or descendant groups';
    }} else {{
        result = result + ' or child groups';
    }}

    if (!includeSelectedGroups) {{
        result = result + ' not including selected groups';
    }}
  }}

  if (checkedRoles.length > 0) {{
     var roleCommaList = checkedRoles.map(function() {{ return $(this).text() }}).get().join(',');
     result = result + ', with role(s): ' + roleCommaList;
  }}

  var groupMemberStatus = $('.js-group-member-status option:selected', $content).text();
  if (groupMemberStatus) {{
     result = result + ', with member status:' + groupMemberStatus;
  }}

  return result;
}}
";
            return string.Format( format, not ? "Not in groups:" : "In groups:" );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return GroupFilterFormatSelection( selection, false );
        }

        /// <summary>
        /// Formats the selection for the InGroupFilter/NotInGroupFilter based on if we are in "Not" mode
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GroupFilterFormatSelection( string selection, bool not )
        {
            string result = "Group Member";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var rockContext = new RockContext();
                var groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( rockContext ).GetByGuids( groupGuids );

                var groupTypeRoleGuidList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable().Where( a => groupTypeRoleGuidList.Contains( a.Guid ) ).ToList();

                bool includeChildGroups = false;
                bool includeChildGroupsPlusDescendants = false;
                bool includeChildGroupsIncludeSelected = false;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                    if ( selectionValues.Length >= 6 )
                    {
                        includeChildGroupsIncludeSelected = selectionValues[4].AsBooleanOrNull() ?? false;
                        includeChildGroupsPlusDescendants = selectionValues[5].AsBooleanOrNull() ?? false;
                    }
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( groups != null )
                {
                    result = string.Format( not ? "Not in groups: {0}" : "In groups: {0}", groups.Select( a => a.Name ).ToList().AsDelimited( ", ", " or " ) );
                    if ( includeChildGroups )
                    {
                        if ( includeChildGroupsPlusDescendants )
                        {
                            result += " or descendant groups";
                        }
                        else
                        {
                            result += " or child groups";
                        }

                        if ( !includeChildGroupsIncludeSelected )
                        {
                            result += " not including selected groups";
                        }
                    }

                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => string.Format( "{0} ({1})", a.Name, a.GroupType.Name ) ).ToList().AsDelimited( "," ) );
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
        /// The GroupPicker
        /// </summary>
        private GroupPicker gp = null;

        /// <summary>
        /// The GroupTypeRole CheckBoxList
        /// </summary>
        private RockCheckBoxList cblRole = null;

        /// <summary>
        /// The "Include Child Groups" checkbox
        /// </summary>
        private RockCheckBox cbChildGroups = null;

        /// <summary>
        /// The "Include Selected Groups" checkbox
        /// </summary>
        private RockCheckBox cbIncludeSelectedGroup = null;

        /// <summary>
        /// The "Include Decendants Groups" checkbox
        /// </summary>
        private RockCheckBox cbChildGroupsPlusDescendants = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            gp = new GroupPicker();
            gp.ID = filterControl.ID + "_gp";
            gp.Label = "Group(s)";
            gp.SelectItem += gp_SelectItem;
            gp.CssClass = "js-group-picker";
            gp.AllowMultiSelect = true;
            filterControl.Controls.Add( gp );

            cbChildGroups = new RockCheckBox();
            cbChildGroups.ID = filterControl.ID + "_cbChildsGroups";
            cbChildGroups.Text = "Include Child Group(s)";
            cbChildGroups.CssClass = "js-include-child-groups";
            cbChildGroups.AutoPostBack = true;
            cbChildGroups.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbChildGroups );

            cbIncludeSelectedGroup = new RockCheckBox();
            cbIncludeSelectedGroup.ID = filterControl.ID + "_cbIncludeSelectedGroup";
            cbIncludeSelectedGroup.Text = "Include Selected Group(s)";
            cbIncludeSelectedGroup.CssClass = "js-include-selected-groups";
            cbIncludeSelectedGroup.AutoPostBack = true;
            cbIncludeSelectedGroup.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbIncludeSelectedGroup );

            cbChildGroupsPlusDescendants = new RockCheckBox();
            cbChildGroupsPlusDescendants.ID = filterControl.ID + "_cbChildGroupsPlusDescendants";
            cbChildGroupsPlusDescendants.Text = "Include All Descendants(s)";
            cbChildGroupsPlusDescendants.CssClass = "js-include-child-groups-descendants";
            cbChildGroupsPlusDescendants.AutoPostBack = true;
            cbChildGroupsPlusDescendants.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbChildGroupsPlusDescendants );

            cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Member Role(s) (optional)";
            cblRole.ID = filterControl.ID + "_cblRole";
            cblRole.CssClass = "js-roles";
            cblRole.Visible = false;
            filterControl.Controls.Add( cblRole );

            RockDropDownList ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = filterControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Select a specific group member status to only include group members with that status. Leaving this blank will return all members.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            filterControl.Controls.Add( ddlGroupMemberStatus );

            return new Control[6] { gp, cbChildGroups, cbIncludeSelectedGroup, cbChildGroupsPlusDescendants, cblRole, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Handles the SelectItem event of the gp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gp_SelectItem( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var groupIdList = gp.SelectedValues.AsIntegerList();
            var groupService = new GroupService( rockContext );

            var qryGroups = groupService.GetByIds( groupIdList );

            if ( qryGroups.Any() )
            {
                var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                var qryGroupTypeRoles = groupTypeRoleService.Queryable();
                List<int> selectedGroupTypeIds = qryGroups.Select( a => a.GroupTypeId ).Distinct().ToList();

                if ( cbChildGroups.Checked )
                {
                    List<int> childGroupTypeIds = new List<int>();
                    foreach ( var groupId in qryGroups.Select( a => a.Id ).ToList() )
                    {
                        if ( cbChildGroupsPlusDescendants.Checked )
                        {
                            // get all children and descendants of the selected group(s)
                            childGroupTypeIds.AddRange( groupService.GetAllDescendents( groupId ).Select( a => a.GroupTypeId ).ToList() );
                        }
                        else
                        {
                            // get only immediate children of the selected group(s)
                            childGroupTypeIds.AddRange( groupService.Queryable().Where( a => a.ParentGroupId == groupId ).Select( a => a.GroupTypeId ).ToList() );
                        }
                    }

                    childGroupTypeIds = childGroupTypeIds.Distinct().ToList();

                    if ( cbIncludeSelectedGroup.Checked )
                    {
                        qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue && ( selectedGroupTypeIds.Contains( a.GroupTypeId.Value ) || childGroupTypeIds.Contains( a.GroupTypeId.Value ) ) );
                    }
                    else
                    {
                        qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue && childGroupTypeIds.Contains( a.GroupTypeId.Value ) );
                    }
                }
                else
                {
                    qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue && selectedGroupTypeIds.Contains( a.GroupTypeId.Value ) );
                }

                var list = qryGroupTypeRoles.OrderBy( a => a.GroupType.Order ).ThenBy( a => a.GroupType.Name ).ThenBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                cblRole.Items.Clear();
                foreach ( var item in list )
                {
                    cblRole.Items.Add( new ListItem( string.Format( "{0} ({1})", item.Name, item.GroupType.Name ), item.Guid.ToString() ) );
                }

                cblRole.Visible = list.Count > 0;
                cbChildGroups.Visible = true;
            }
            else
            {
                cblRole.Visible = false;
                cbChildGroups.Visible = false;
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
            if ( controls.Count() < 6 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            groupPicker.RenderControl( writer );
            cbChildGroups.RenderControl( writer );
            if ( !cbChildGroups.Checked )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Disabled, "disabled" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbIncludeSelectedGroup.ContainerCssClass = "margin-l-md";
            cbIncludeSelectedGroup.RenderControl( writer );
            cbChildGroupsPlusDescendants.ContainerCssClass = "margin-l-md";
            cbChildGroupsPlusDescendants.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            cblRoles.RenderControl( writer );

            ddlGroupMemberStatus.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() < 6 )
            {
                return null;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;

            List<int> groupIdList = groupPicker.SelectedValues.AsIntegerList();
            var groupGuids = new GroupService( new RockContext() ).GetByIds( groupIdList ).Select( a => a.Guid ).Distinct().ToList();

            return string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}",
                groupGuids.AsDelimited( "," ),
                cblRoles.SelectedValues.AsDelimited( "," ),
                cbChildGroups.Checked.ToString(),
                ddlGroupMemberStatus.SelectedValue,
                cbIncludeSelectedGroup.Checked.ToString(),
                cbChildGroupsPlusDescendants.Checked.ToString() );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() < 6 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                List<Guid> groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( new RockContext() ).GetByGuids( groupGuids );
                if ( groups != null )
                {
                    groupPicker.SetValues( groups );
                }

                if ( selectionValues.Length >= 3 )
                {
                    cbChildGroups.Checked = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                if ( selectionValues.Length >= 6 )
                {
                    cbIncludeSelectedGroup.Checked = selectionValues[4].AsBooleanOrNull() ?? false;
                    cbChildGroupsPlusDescendants.Checked = selectionValues[5].AsBooleanOrNull() ?? false;
                }
                else
                {
                    cbIncludeSelectedGroup.Checked = true;
                    cbChildGroupsPlusDescendants.Checked = true;
                }

                gp_SelectItem( this, new EventArgs() );

                string[] selectedRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var item in cblRoles.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleGuids.Contains( item.Value );
                }

                if ( selectionValues.Length >= 4 )
                {
                    ddlGroupMemberStatus.SetValue( selectionValues[3] );
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
                GroupMemberService groupMemberService = new GroupMemberService( (RockContext)serviceInstance.Context );
                List<Guid> groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groupService = new GroupService( (RockContext)serviceInstance.Context );
                var groupIds = groupService.GetByGuids( groupGuids ).Select( a => a.Id ).Distinct().ToList();

                bool includeChildGroups = false;
                bool includeChildGroupsIncludeSelected = false;
                bool includeChildGroupsPlusDescendants = false;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                if ( selectionValues.Length >= 6 )
                {
                    includeChildGroupsIncludeSelected = selectionValues[4].AsBooleanOrNull() ?? false;
                    includeChildGroupsPlusDescendants = selectionValues[5].AsBooleanOrNull() ?? false;
                }
                else if ( includeChildGroups )
                {
                    // in case the selection was saved before these options where added
                    includeChildGroupsIncludeSelected = true;
                    includeChildGroupsPlusDescendants = true;
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                var groupMemberServiceQry = groupMemberService.Queryable();

                if ( includeChildGroups )
                {
                    List<int> childGroupIds = new List<int>();
                    foreach ( var groupId in groupIds )
                    {
                        if ( includeChildGroupsPlusDescendants )
                        {
                            // get all children and descendants of the selected group(s)
                            childGroupIds.AddRange( groupService.GetAllDescendents( groupId ).Select( a => a.Id ).Distinct().ToList() );
                        }
                        else
                        {
                            // get only immediate children of the selected group(s)
                            childGroupIds.AddRange( groupService.Queryable().Where( a => a.ParentGroupId == groupId ).Select( a => a.Id ) );
                        }
                    }

                    if ( includeChildGroupsIncludeSelected )
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupIds.Contains( xx.GroupId ) || childGroupIds.Contains( xx.GroupId ) );
                    }
                    else
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => childGroupIds.Contains( xx.GroupId ) );
                    }
                }
                else
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupIds.Contains( xx.GroupId ) );
                }

                if ( groupMemberStatus.HasValue )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.GroupMemberStatus == groupMemberStatus.Value );
                }

                var groupRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                if ( groupRoleGuids.Count() > 0 )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupRoleGuids.Contains( xx.GroupRole.Guid ) );
                }

                var qry = new PersonService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}