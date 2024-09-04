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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select a comma-delimited list of members of the group" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Group Member List" )]
    [Rock.SystemGuid.EntityTypeGuid( "42D54FD1-74AA-4D47-BE84-BF92334ECD33")]
    public class MemberListSelect : DataSelectComponent, IRecipientDataSelect
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
                return typeof( Rock.Model.Group ).FullName;
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
            get
            {
                return base.Section;
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
                return "MemberList";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( IEnumerable<MemberInfo> ); }
        }

        /// <summary>
        /// little class so that we only need to fetch the columns that we need from Person
        /// </summary>
        private class MemberInfo
        {
            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? SuffixValueId { get; set; }

            public int PersonId { get; set; }

            public int GroupMemberId { get; set; }

            public override string ToString()
            {
                return NickName + " " + LastName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ShowAsLinkType
        {
            NameOnly = 0,
            PersonLink = 1,
            GroupMemberLink = 2
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var callbackField = new CallbackField();
            string basePersonUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/Person/" );
            string baseGroupMemberUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/GroupMember/" );
            var selectionParts = selection.Split( '|' );
            ShowAsLinkType showAsLinkType = selectionParts.Length > 0 ? selectionParts[0].ConvertToEnum<ShowAsLinkType>( ShowAsLinkType.NameOnly ) : ShowAsLinkType.NameOnly;
            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                var groupMemberList = e.DataValue as IEnumerable<MemberInfo>;
                if ( groupMemberList != null )
                {
                    var formattedList = new List<string>();
                    foreach ( var groupMember in groupMemberList )
                    {
                        var formattedPersonFullName = Rock.Model.Person.FormatFullName( groupMember.NickName, groupMember.LastName, groupMember.SuffixValueId );
                        string formattedValue;
                        if ( showAsLinkType == ShowAsLinkType.PersonLink )
                        {
                            formattedValue = "<a href='" + basePersonUrl + groupMember.PersonId.ToString() + "'>" + formattedPersonFullName + "</a>";
                        }
                        else if ( showAsLinkType == ShowAsLinkType.GroupMemberLink )
                        {
                            formattedValue = "<a href='" + baseGroupMemberUrl + groupMember.GroupMemberId.ToString() + "'>" + formattedPersonFullName + "</a>";
                        }
                        else
                        {
                            formattedValue = formattedPersonFullName;
                        }

                        formattedList.Add( formattedValue );
                    }

                    e.FormattedValue = formattedList.AsDelimited( ", " );
                }
                else
                {
                    e.FormattedValue = string.Empty;
                }
            };

            return callbackField;
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
                return "Member List";
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
            return "Member List";
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
            // disable sorting on this column since it is an IEnumerable
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
            var qryGroupService = new GroupService( context ).Queryable();

            Expression<Func<Rock.Model.GroupMember, bool>> memberWhereGroupType = a => 1 == 1;
            Expression<Func<Rock.Model.GroupMember, bool>> memberWhereGroupRoles = a => 1 == 1;
            Expression<Func<Rock.Model.GroupMember, bool>> memberWhereStatus = a => 1 == 1;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 3 )
            {
                GroupMemberService groupMemberService = new GroupMemberService( context );
                int? groupTypeId = null;

                Guid groupTypeGuid = selectionValues[1].AsGuid();

                var groupType = GroupTypeCache.Get( groupTypeGuid );
                if ( groupType != null )
                {
                    groupTypeId = groupType.Id;
                }

                if ( groupTypeId.HasValue )
                {
                    memberWhereGroupType = xx => xx.Group.GroupTypeId == groupTypeId;
                }

                var groupRoleGuids = selectionValues[2].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                List<int> groupRoleIds = null;
                if ( groupRoleGuids.Count() > 0 )
                {
                    groupRoleIds = new GroupTypeRoleService( context ).Queryable().Where( a => groupRoleGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();
                    memberWhereGroupRoles = xx => groupRoleIds.Contains( xx.GroupRoleId );
                }

                GroupMemberStatus? groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();

                if ( groupMemberStatus.HasValue )
                {
                    memberWhereStatus = xx => xx.GroupMemberStatus == groupMemberStatus.Value;
                }
            }

            var memberListQuery = qryGroupService.Select( p => p.Members.AsQueryable()
                    .Where( memberWhereGroupType )
                    .Where( memberWhereGroupRoles )
                    .Where( memberWhereStatus )
                    .Where( m => !m.IsArchived )
                    .Select( m => new MemberInfo
                    {
                        NickName = m.Person.NickName,
                        LastName = m.Person.LastName,
                        SuffixValueId = m.Person.SuffixValueId,
                        PersonId = m.PersonId,
                        GroupMemberId = m.Id
                    } ).OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) );

            var selectChildrenExpression = SelectExpressionExtractor.Extract( memberListQuery, entityIdProperty, "p" );

            return selectChildrenExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockRadioButtonList rblShowAsLinkType = new RockRadioButtonList();
            rblShowAsLinkType.ID = parentControl.ID + "_rblShowAsLinkType";
            rblShowAsLinkType.Items.Add( new ListItem( "Show Name Only", ShowAsLinkType.NameOnly.ConvertToInt().ToString() ) );
            rblShowAsLinkType.Items.Add( new ListItem( "Show as Person Link", ShowAsLinkType.PersonLink.ConvertToInt().ToString() ) );
            rblShowAsLinkType.Items.Add( new ListItem( "Show as Group Member Link", ShowAsLinkType.GroupMemberLink.ConvertToInt().ToString() ) );
            parentControl.Controls.Add( rblShowAsLinkType );

            var groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = parentControl.ID + "_groupTypePicker";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.CssClass = "js-grouptype-picker";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            parentControl.Controls.Add( groupTypePicker );

            int? selectedGroupTypeId = parentControl.Page.Request.Params[groupTypePicker.UniqueID].AsIntegerOrNull();
            groupTypePicker.SelectedGroupTypeId = selectedGroupTypeId;

            var cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Role(s)";
            cblRole.CssClass = "js-group-role";
            cblRole.ID = parentControl.ID + "_cblRole";
            parentControl.Controls.Add( cblRole );

            PopulateGroupRolesCheckList( groupTypePicker );

            RockDropDownList ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = parentControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Select a specific group member status to only include group members with that status. Leaving this blank will return all members.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            parentControl.Controls.Add( ddlGroupMemberStatus );

            return new System.Web.UI.Control[] { rblShowAsLinkType, groupTypePicker, cblRole, ddlGroupMemberStatus };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateGroupRolesCheckList( sender as GroupTypePicker );
        }

        /// <summary>
        /// Populates the group roles check list.
        /// </summary>
        /// <param name="groupTypePicker">The group type picker.</param>
        private void PopulateGroupRolesCheckList( GroupTypePicker groupTypePicker )
        {
            var groupTypeId = groupTypePicker.SelectedGroupTypeId;
            RockCheckBoxList cblRole = groupTypePicker.Parent.ControlsOfTypeRecursive<RockCheckBoxList>().FirstOrDefault( a => a.HasCssClass( "js-group-role" ) );
            if ( groupTypeId.HasValue )
            {
//                cblRole.Items.Clear();
                foreach ( var item in new GroupTypeRoleService( new RockContext() ).GetByGroupTypeId( groupTypeId.Value ) )
                {
                    cblRole.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                cblRole.Style[HtmlTextWriterStyle.Display] = cblRole.Items.Count > 0 ? "" : "none";
            }
            else
            {
                cblRole.Style[HtmlTextWriterStyle.Display] = "none";
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
            RockRadioButtonList rblShowAsLinkType = controls[0] as RockRadioButtonList;

            var groupTypePicker = controls[1] as GroupTypePicker;
            var cblRoles = controls[2] as RockCheckBoxList;
            var ddlMemberStatus = controls[3] as RockDropDownList;

            int groupTypeId = groupTypePicker.SelectedValueAsId() ?? 0;
            Guid? groupTypeGuid = null;
            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType != null )
            {
                groupTypeGuid = groupType.Guid;
            }

            var rolesGuidCommaList = cblRoles.SelectedValues.AsDelimited( "," );

            var memberStatusValue = ddlMemberStatus.SelectedValue;

            return rblShowAsLinkType.SelectedValue + "|" + groupTypeGuid.ToString() + "|" + rolesGuidCommaList + "|" + memberStatusValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                RockRadioButtonList rblShowAsLinkType = controls[0] as RockRadioButtonList;
                GroupTypePicker groupTypePicker = controls[1] as GroupTypePicker;
                RockCheckBoxList cblRole = controls[2] as RockCheckBoxList;
                RockDropDownList ddlGroupMemberStatus = controls[3] as RockDropDownList;

                rblShowAsLinkType.SelectedValue = selectionValues[0].ConvertToEnum<ShowAsLinkType>( ShowAsLinkType.NameOnly ).ConvertToInt().ToString();

                if ( selectionValues.Length >= 3 )
                {
                    Guid groupTypeGuid = selectionValues[1].AsGuid();
                    var groupTypeId = new GroupTypeService( new RockContext() ).GetId( groupTypeGuid );
                    if ( groupTypeId.HasValue )
                    {
                        groupTypePicker.SetValue( groupTypeId.Value );
                    }

                    groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );

                    string[] selectedRoleGuids = selectionValues[2].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                    foreach ( var item in cblRole.Items.OfType<ListItem>() )
                    {
                        item.Selected = selectedRoleGuids.Contains( item.Value );
                    }

                    ddlGroupMemberStatus.SetValue( selectionValues[3] );
                }
            }
        }

        #endregion

        #region IRecipientDataSelect implementation

        /// <summary>
        /// Gets the type of the recipient column field.
        /// </summary>
        /// <value>
        /// The type of the recipient column field.
        /// </value>
        public Type RecipientColumnFieldType
        {
            get { return typeof( IEnumerable<int> ); }
        }

        /// <summary>
        /// Gets the recipient person identifier expression.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public Expression GetRecipientPersonIdExpression( System.Data.Entity.DbContext dbContext, MemberExpression entityIdProperty, string selection )
        {
            var rockContext = dbContext as RockContext;
            if ( rockContext != null )
            {
                var qryGroupService = new GroupService( rockContext ).Queryable();

                Expression<Func<Rock.Model.GroupMember, bool>> memberWhereGroupType = a => 1 == 1;
                Expression<Func<Rock.Model.GroupMember, bool>> memberWhereGroupRoles = a => 1 == 1;
                Expression<Func<Rock.Model.GroupMember, bool>> memberWhereStatus = a => 1 == 1;

                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 3 )
                {
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    int? groupTypeId = null;

                    Guid groupTypeGuid = selectionValues[1].AsGuid();

                    var groupType = GroupTypeCache.Get( groupTypeGuid );
                    if ( groupType != null )
                    {
                        groupTypeId = groupType.Id;
                    }

                    if ( groupTypeId.HasValue )
                    {
                        memberWhereGroupType = xx => xx.Group.GroupTypeId == groupTypeId;
                    }

                    var groupRoleGuids = selectionValues[2].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                    List<int> groupRoleIds = null;
                    if ( groupRoleGuids.Count() > 0 )
                    {
                        groupRoleIds = new GroupTypeRoleService( rockContext ).Queryable().Where( a => groupRoleGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();
                        memberWhereGroupRoles = xx => groupRoleIds.Contains( xx.GroupRoleId );
                    }

                    GroupMemberStatus? groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();

                    if ( groupMemberStatus.HasValue )
                    {
                        memberWhereStatus = xx => xx.GroupMemberStatus == groupMemberStatus.Value;
                    }
                }

                var memberListQuery = qryGroupService.Select( p => p.Members.AsQueryable()
                        .Where( memberWhereGroupType )
                        .Where( memberWhereGroupRoles )
                        .Where( memberWhereStatus )
                        .Select( m => m.PersonId ) );

                var selectChildrenExpression = SelectExpressionExtractor.Extract( memberListQuery, entityIdProperty, "p" );

                return selectChildrenExpression;
            }

            return null;
        }

        #endregion
    }
}
