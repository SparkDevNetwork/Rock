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
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

namespace Rock.Reporting.DataFilter.GroupMembers
{
    /// <summary>
    /// Operates against GroupMember
    /// </summary>
    [Description( "Filter GroupMembers based on their grouptype" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member GroupType Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "858E0A63-3FF8-48F1-82DE-9903DB01000D" )]
    public class GroupMemberGroupTypeFilter : DataFilterComponent
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
            get { return "Rock.Model.GroupMember"; }
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/GroupMember/groupMemberGroupTypeFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            ListItemBag groupType = GroupTypeCache
                .Get( selection.AsGuidOrNull() ?? Guid.Empty )
                ?.ToListItemBag();

            return new Dictionary<string, string>
            {
                ["groupType"] = groupType?.ToCamelCaseJson( false, true )
            };
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var groupTypeJson = data.GetValueOrDefault( "groupType", string.Empty );
            var groupType = groupTypeJson.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;

            return groupType;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetTitle( Type entityType )
        {
            return "Group Type";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
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
  var groupTypeName = $('.group-type-picker', $content).find(':selected').text()
  var result = 'Group type: ' + groupTypeName;

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
            string result = "Group Type";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var groupType = GroupTypeCache.Get( selectionValues[0].AsGuid() );

                if ( groupType != null )
                {
                    result = string.Format( "Group type: {0}", groupType.Name );
                }
            }

            return result;
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            GroupTypePicker groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.GetChildControlInstanceName( "_groupTypePicker" );
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().ToList();
            filterControl.Controls.Add( groupTypePicker );

            return new Control[] { groupTypePicker };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            int? groupTypeId = ( controls[0] as GroupTypePicker ).SelectedValueAsId();
            Guid? groupTypeGuid = null;
            var groupType = GroupTypeCache.Get( groupTypeId ?? 0 );
            if ( groupType != null )
            {
                groupTypeGuid = groupType.Guid;
            }

            return groupTypeGuid.ToString();
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
            if ( selectionValues.Length >= 1 )
            {
                var groupType = GroupTypeCache.Get( selectionValues[0].AsGuid() );
                if ( groupType != null )
                {
                    ( controls[0] as GroupTypePicker ).SetValue( groupType.Id );
                }
            }
        }
#endif

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var groupType = GroupTypeCache.Get( selectionValues[0].AsGuid() );
                int? groupTypeId = null;
                if ( groupType != null )
                {
                    groupTypeId = groupType.Id;
                }

                var qry = new GroupMemberService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => p.Group.GroupTypeId == groupTypeId );

                return FilterExpressionExtractor.Extract<Rock.Model.GroupMember>( qry, parameterExpression, "p" );
            }

            return null;
        }

        #endregion
    }
}
