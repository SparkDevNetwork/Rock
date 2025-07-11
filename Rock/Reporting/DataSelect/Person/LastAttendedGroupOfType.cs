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
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Show the date of the last attendance in a group of type." )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select last attendance date of a person in a specific type of group." )]
    [Rock.SystemGuid.EntityTypeGuid( "F3C67ECD-5E80-4807-8F90-6AD110674ADF" )]
    public class LastAttendedGroupOfType : DataSelectComponent
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
            get
            {
                return "Last Attendance In Group Of Type";
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
            get { return typeof( DateTime? ); }
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
                return "Last Attendance In Group Of Type";
            }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var groupTypeOptions = new GroupTypeService( rockContext ).Queryable()
                .OrderBy( gt => gt.Order )
                .ThenBy( gt => gt.Name )
                .Select( gt => new ListItemBag { Text = gt.Name, Value = gt.Guid.ToString() } )
                .ToList();

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Person/lastAttendedGroupOfTypeSelect.obs" ),
                Options = new Dictionary<string, string>
                {
                    ["groupTypeOptions"] = groupTypeOptions.ToCamelCaseJson( false, true ),
                }
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new Dictionary<string, string> { ["groupType"] = selection };
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            return data.GetValueOrDefault( "groupType", string.Empty );
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
            return "Last Attendance In Group Of Type";
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
            Guid groupTypeGuid = selection.AsGuid();

            if ( groupTypeGuid != Guid.Empty )
            {
                AttendanceService attendanceService = new AttendanceService( context );
                var groupAttendanceQry = attendanceService.Queryable().Where( a => a.Occurrence.Group.GroupType.Guid == groupTypeGuid );

                var qry = new PersonService( context ).Queryable()
                    .Select( p => groupAttendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id && xx.DidAttend == true ).Max( xx => xx.StartDateTime ) );

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
            groupTypePicker.ID = parentControl.ID + "_0";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().ToList();
            groupTypePicker.AutoPostBack = true;
            parentControl.Controls.Add( groupTypePicker );

            return new Control[1] { groupTypePicker };
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
            // Get the selected Group Type as a Guid.
            var groupTypeId = ( controls[0] as GroupTypePicker ).SelectedValueAsId().GetValueOrDefault( 0 );

            string value1 = string.Empty;

            if ( groupTypeId > 0 )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                value1 = ( groupType == null ) ? string.Empty : groupType.Guid.ToString();
            }

            return value1;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var groupType = new GroupTypeService( new RockContext() ).Get( selection.AsGuid() );
            ( controls[0] as GroupTypePicker ).SetValue( groupType != null ? groupType.Id : ( int? ) null );
        }

        #endregion
    }
}
