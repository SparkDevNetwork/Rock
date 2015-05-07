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
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// A Report Field that shows the number of Members in the Group with a specified Status or Role type.
    /// </summary>
    [Description( "Shows the number of Members in the Group with a specified Status or Role type" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Member Count" )]
    public class MemberCountSelect : DataSelectComponent
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
                return "Statistics";
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
                return "Member Count";
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
            get { return typeof( int? ); }
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
                return "Member Count";
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
            return "Member Count";
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
            var values = selection.Split( '|' );

            IQueryable<int> memberCountQuery;

            if ( values.Length >= 2 )
            {
                bool? isLeader = values[0].AsBooleanOrNull();
                GroupMemberStatus? memberStatusValue = (GroupMemberStatus?)values[1].AsIntegerOrNull();

                memberCountQuery = new GroupService( context ).Queryable()
                                                            .Select( p => p.Members.Count( a =>
                                                                                         ( !memberStatusValue.HasValue || a.GroupMemberStatus == memberStatusValue )
                                                                                         && ( !isLeader.HasValue || ( a.GroupRole.IsLeader == isLeader.Value ) ) ) );
            }
            else
            {
                memberCountQuery = new GroupService( context ).Queryable().Select( p => (int)0 );
            }

            var selectExpression = SelectExpressionExtractor.Extract<Rock.Model.Group>( memberCountQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            // Define Control: Is Leader?
            RockDropDownList ddlLeader = new RockDropDownList();
            ddlLeader.ID = string.Format( "{0}_ddlMemberType", parentControl.ID );
            ddlLeader.AddCssClass( "js-filter-control js-member-is-leader" );
            ddlLeader.Label = "Member Type";
            ddlLeader.Items.Add( new ListItem( string.Empty, string.Empty ) );
            ddlLeader.Items.Add( new ListItem( "Leader", "true" ) );
            ddlLeader.Items.Add( new ListItem( "Not Leader", "false" ) );
            parentControl.Controls.Add( ddlLeader );

            // Define Control: Member Status
            RockDropDownList ddlMemberStatus = new RockDropDownList();
            ddlMemberStatus.ID = string.Format( "{0}_ddlMemberStatus", parentControl.ID );
            ddlMemberStatus.AddCssClass( "js-filter-control js-member-status" );
            ddlMemberStatus.Label = "Member Status";
            ddlMemberStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( GroupMemberStatus memberStatus in Enum.GetValues( typeof( GroupMemberStatus ) ) )
            {
                ddlMemberStatus.Items.Add( new ListItem( memberStatus.ConvertToString(), memberStatus.ConvertToInt().ToString() ) );
            }

            parentControl.Controls.Add( ddlMemberStatus );

            return new System.Web.UI.Control[] { ddlLeader, ddlMemberStatus };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var ddlLeader = (DropDownList)controls[0];
            var ddlMemberStatus = (DropDownList)controls[1];

            return string.Format( "{0}|{1}", ddlLeader.SelectedValue, ddlMemberStatus.SelectedValue );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var values = selection.Split( '|' );

            var ddlLeader = (DropDownList)controls[0];
            var ddlMemberStatus = (DropDownList)controls[1];

            if ( values.Length >= 2 )
            {
                ddlLeader.SelectedValue = values[0];
                ddlMemberStatus.SelectedValue = values[1];
            }
        }

        #endregion
    }
}
