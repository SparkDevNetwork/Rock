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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Rock.Net;
using Rock.Web.Cache;
using Rock.ViewModels.Controls;

namespace Rock.Reporting.DataFilter.ConnectionRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Would allow filtering requests to a specific Connection Type." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Type Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "6FADE7F1-DF86-47F9-BBD9-31C7188C1714" )]
    public class ConnectionTypeFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.ConnectionRequest ).FullName; }
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
            var connectionTypeOptions = ConnectionTypeCache.All()
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .Select( ct => ct.ToListItemBag() )
                .ToList();

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/ConnectionRequest/connectionTypeFilter.obs" ),
                Options = new Dictionary<string, string>
                {
                    { "connectionTypeOptions", connectionTypeOptions.ToCamelCaseJson(false, true) }
                }
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = SelectionConfig.Parse( selection );

            var data = new Dictionary<string, string>
            {
                { "connectionType", config?.ConnectionTypeGuid.ToString() },
            };

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionConfig = new SelectionConfig
            {
                ConnectionTypeGuid = data.GetValueOrNull( "connectionType" )?.AsGuid(),
            };

            return selectionConfig.ToJson();
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
            return "Connection Type";
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
  var connectionTypeName = $('.js-connection-type-picker', $content).find(':selected').text()
  var result = 'Connection Type: ' + connectionTypeName;

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
            string result = "Connection Type";
            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.ConnectionTypeGuid.HasValue )
            {
                var connectionType = new ConnectionTypeService( new RockContext() ).Get( selectionConfig.ConnectionTypeGuid.Value );

                if ( connectionType != null )
                {
                    result = string.Format( "Connection type: {0}", connectionType.Name );
                }
            }

            return result;
        }

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList connectionTypePicker = new RockDropDownList();
            connectionTypePicker.CssClass = "js-connection-type-picker";
            connectionTypePicker.ID = filterControl.ID + "_connectionTypePicker";
            connectionTypePicker.Label = "Connection Type";

            connectionTypePicker.Items.Clear();
            connectionTypePicker.Items.Insert( 0, new ListItem() );
            var connectionTypeList = new ConnectionTypeService( new RockContext() ).Queryable()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            foreach ( var connectionType in connectionTypeList )
            {
                connectionTypePicker.Items.Add( new ListItem( connectionType.Name, connectionType.Id.ToString() ) );
            }

            filterControl.Controls.Add( connectionTypePicker );

            return new Control[] { connectionTypePicker };
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
            var selectionConfig = new SelectionConfig();
            var connectionTypeId = ( controls[0] as RockDropDownList ).SelectedValueAsId();
            var connectionType = new ConnectionTypeService( new RockContext() ).Get( connectionTypeId ?? 0 );
            if ( connectionType != null )
            {
                selectionConfig.ConnectionTypeGuid = connectionType.Guid;
            }

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
                if ( controls.Length > 0 && selectionConfig.ConnectionTypeGuid.HasValue )
                {
                    var connectionType = new ConnectionTypeService( new RockContext() ).Get( selectionConfig.ConnectionTypeGuid.Value );
                    if ( connectionType != null )
                    {
                        ( controls[0] as RockDropDownList ).SetValue( connectionType.Id );
                    }
                }
            }
        }

#endif

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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( !selectionConfig.ConnectionTypeGuid.HasValue )
            {
                return null;
            }

            var connectionType = new ConnectionTypeService( new RockContext() ).Get( selectionConfig.ConnectionTypeGuid.Value );
            int? connectionTypeId = null;
            if ( connectionType != null )
            {
                connectionTypeId = connectionType.Id;
            }

            var qry = new ConnectionRequestService( ( RockContext ) serviceInstance.Context ).Queryable()
                .Where( p => p.ConnectionOpportunity.ConnectionTypeId == connectionTypeId );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ConnectionRequest>( qry, parameterExpression, "p" );

            return extractedFilterExpression;

        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
            }

            /// <summary>
            /// Gets or sets the connection type identifiers.
            /// </summary>
            /// <value>
            /// The connection type identifiers.
            /// </value>
            public Guid? ConnectionTypeGuid { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }
    }
}