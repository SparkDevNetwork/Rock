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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Show if person is in a group of a specific group type" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select if Person in specific group type" )]
    public class InGroupGroupTypeSelect : DataSelectComponent
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
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "In Group Type";
            }
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
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( bool? ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "In Group Type";
            }
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
            return "In Group of Group Type";
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
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                GroupMemberService groupMemberService = new GroupMemberService( context );
                Guid groupTypeGuid = selectionValues[0].AsGuid();

                var groupMemberServiceQry = groupMemberService.Queryable().Where( xx => xx.Group.GroupType.Guid == groupTypeGuid );

                var groupRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                if ( groupRoleGuids.Count() > 0 )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupRoleGuids.Contains( xx.GroupRole.Guid ) );
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

                var qry = new PersonService( context ).Queryable()
                    .Select( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression selectExpression = SelectExpressionExtractor.Extract( qry, entityIdProperty, "p" );

                return selectExpression;
            }

            return null;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            var groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = parentControl.ID + "_groupTypePicker";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            parentControl.Controls.Add( groupTypePicker );

            int? selectedGroupTypeId = parentControl.Page.Request.Params[groupTypePicker.UniqueID].AsIntegerOrNull();
            groupTypePicker.SelectedGroupTypeId = selectedGroupTypeId;

            var cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Role(s)";
            cblRole.ID = parentControl.ID + "_cblRole";
            parentControl.Controls.Add( cblRole );

            PopulateGroupRolesCheckList( groupTypePicker.SelectedGroupTypeId ?? 0, cblRole );

            RockDropDownList ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = parentControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Select a specific group member status only include to only show true for group members with that status. Leaving this blank will return true for all members.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            parentControl.Controls.Add( ddlGroupMemberStatus );

            return new Control[3] { groupTypePicker, cblRole, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            var groupTypePicker = sender as GroupTypePicker;
            if ( groupTypePicker != null )
            {
                var cblRole = groupTypePicker.Parent.FindControl( groupTypePicker.ID.Replace( "_groupTypePicker", "_cblRole" ) ) as RockCheckBoxList;
                if ( cblRole != null )
                {
                    int groupTypeId = groupTypePicker.SelectedValueAsId() ?? 0;
                    PopulateGroupRolesCheckList( groupTypeId, cblRole );
                }
            }
        }

        /// <summary>
        /// Populates the group roles.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="cblRole">The CBL role.</param>
        private void PopulateGroupRolesCheckList( int groupTypeId, RockCheckBoxList cblRole )
        {
            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType != null )
            {
                cblRole.Items.Clear();
                foreach ( var item in new GroupTypeRoleService( new RockContext() ).GetByGroupTypeId( groupType.Id ) )
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
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var groupTypePicker = ( controls[0] as GroupTypePicker );
            var cblRoles = ( controls[1] as RockCheckBoxList );
            var ddlMemberStatus = ( controls[2] as RockDropDownList );

            int groupTypeId = groupTypePicker.SelectedValueAsId() ?? 0;
            Guid? groupTypeGuid = null;
            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType != null )
            {
                groupTypeGuid = groupType.Guid;
            }

            var rolesGuidCommaList = cblRoles.SelectedValues.AsDelimited( "," );

            var memberStatusValue = ddlMemberStatus.SelectedValue;

            return groupTypeGuid.ToString() + "|" + rolesGuidCommaList + "|" + memberStatusValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Guid groupTypeGuid = selectionValues[0].AsGuid();
                var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeGuid );
                var groupTypePicker = ( controls[0] as GroupTypePicker );
                groupTypePicker.SetValue( groupType != null ? groupType.Id : (int?)null );

                groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );

                string[] selectedRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                RockCheckBoxList cblRole = ( controls[1] as RockCheckBoxList );

                foreach ( var item in cblRole.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleGuids.Contains( item.Value );
                }

                RockDropDownList ddlGroupMemberStatus = controls[2] as RockDropDownList;
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

        #endregion
    }
}
