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
    [Description( "Filter people on whether they are in the specified group" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group Filter" )]
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
            return "In Group";
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
  var groupName = $('.group-picker', $content).find('.selected-names').text();
  var checkedRoles = $('.rock-check-box-list', $content).find(':checked').closest('label');
  var result = 'In group: ' + groupName;
  if (checkedRoles.length > 0) {
     var roleCommaList = checkedRoles.map(function() { return $(this).text() }).get().join(',');
     result = result + ', with role(s): ' + roleCommaList;
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
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( selectionValues[0].AsGuid() );

                var groupTypeRoleGuidList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable().Where( a => groupTypeRoleGuidList.Contains( a.Guid ) ).ToList();

                bool includeChildGroups = false;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( group != null )
                {
                    result = string.Format( "In group: {0}", group.Name );
                    if ( includeChildGroups )
                    {
                        result += " or child groups";
                    }

                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => string.Format( "{0} ({1})", a.Name, a.GroupType.Name ) ).ToList().AsDelimited( "," ) );
                    }

                    if (groupMemberStatus.HasValue)
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
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            gp = new GroupPicker();
            gp.ID = filterControl.ID + "_gp";
            gp.Label = "Group";
            gp.SelectItem += gp_SelectItem;
            filterControl.Controls.Add( gp );

            cbChildGroups = new RockCheckBox();
            cbChildGroups.ID = filterControl.ID + "_cbChildsGroups";
            cbChildGroups.Text = "Include Child Group(s)";
            cbChildGroups.AutoPostBack = true;
            cbChildGroups.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbChildGroups );

            cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Member Role(s) (optional)";
            cblRole.ID = filterControl.ID + "_cblRole";
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

            return new Control[4] { gp, cbChildGroups, cblRole, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Handles the SelectItem event of the gp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gp_SelectItem( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            int groupId = gp.SelectedValueAsId() ?? 0;
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( groupId );

            if ( group != null )
            {
                var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                var qryGroupTypeRoles = groupTypeRoleService.Queryable();
                if ( cbChildGroups.Checked )
                {
                    var childGroupTypeIds = groupService.GetAllDescendents( group.Id ).Select( a => a.GroupTypeId ).Distinct().ToList();
                    qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue && ( a.GroupTypeId == group.GroupTypeId || childGroupTypeIds.Contains( a.GroupTypeId.Value ) ) );
                }
                else
                {
                    qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue && a.GroupTypeId == group.GroupTypeId );
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
            if ( controls.Count() >= 3 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                GroupPicker groupPicker = controls[0] as GroupPicker;
                groupPicker.RenderControl( writer );
                RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
                cbChildGroups.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                RockCheckBoxList cblRoles = controls[2] as RockCheckBoxList;
                cblRoles.RenderControl( writer );
                RockDropDownList ddlGroupMemberStatus = controls[3] as RockDropDownList;
                ddlGroupMemberStatus.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            int groupId = ( controls[0] as GroupPicker ).SelectedValueAsInt() ?? 0;
            Guid? groupGuid = null;
            var group = new GroupService( new RockContext() ).Get( groupId );
            if ( group != null )
            {
                groupGuid = group.Guid;
            }

            var value2 = ( controls[2] as RockCheckBoxList ).SelectedValues.AsDelimited( "," );
            var value3 = ( controls[1] as CheckBox ).Checked.ToString();
            var value4 = ( controls[3] as RockDropDownList ).SelectedValue;
            return groupGuid.ToString() + "|" + value2 + "|" + value3 + "|" + value4;
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
                Guid groupGuid = selectionValues[0].AsGuid();
                var group = new GroupService( new RockContext() ).Get( groupGuid );
                if ( group != null )
                {
                    ( controls[0] as GroupPicker ).SetValue( group.Id );
                }

                CheckBox cbChildGroups = controls[1] as CheckBox;
                if ( selectionValues.Length >= 3 )
                {
                    cbChildGroups.Checked = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                gp_SelectItem( this, new EventArgs() );

                string[] selectedRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                RockCheckBoxList cblRole = controls[2] as RockCheckBoxList;

                foreach ( var item in cblRole.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleGuids.Contains( item.Value );
                }

                RockDropDownList ddlGroupMemberStatus = controls[3] as RockDropDownList;
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
                int groupId = 0;
                Guid groupGuid = selectionValues[0].AsGuid();
                var groupService = new GroupService( (RockContext)serviceInstance.Context );
                var group = groupService.Get( groupGuid );
                if ( group != null )
                {
                    groupId = group.Id;
                }

                bool includeChildGroups = false;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                var groupMemberServiceQry = groupMemberService.Queryable();

                if ( includeChildGroups )
                {
                    var childGroupIds = groupService.GetAllDescendents( group.Id ).Select( a => a.Id ).Distinct().ToList();
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.GroupId == groupId || childGroupIds.Contains( xx.GroupId ) );
                }
                else
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.GroupId == groupId );
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