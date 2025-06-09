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
using Rock.ViewModels.Utility;
using Rock.ViewModels.Controls;

namespace Rock.Reporting.DataFilter.ConnectionRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Would allow filtering requests to a specific Connection Opportunity." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Opportunity Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "A2B6B6D1-02FB-46A9-8588-1D93FE44E51E" )]
    public class ConnectionOpportunityFilter : DataFilterComponent
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
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/ConnectionRequest/connectionOpportunityFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = SelectionConfig.Parse( selection );

            var connectionOpportunityOptions = new Dictionary<string, List<ListItemBag>>();

            var connectionTypeOptions = ConnectionTypeCache.All()
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .Select( ct => ct.ToListItemBag() )
                .ToList();

            var connectionTypeGuids = connectionTypeOptions.Select( ct => ct.Value.AsGuid() ).ToList();

            foreach ( var ct in connectionTypeGuids )
            {
                var connectionOpportunities = new ConnectionOpportunityService( rockContext ).Queryable()
                    .Where( co => co.ConnectionType.Guid == ct && co.IsActive )
                    .OrderBy( co => co.Order )
                    .ThenBy( co => co.Name )
                    .ToList()
                    .Select( co => co.ToListItemBag() )
                    .ToList();
                connectionOpportunityOptions.Add( ct.ToString(), connectionOpportunities );
            }

            var data = new Dictionary<string, string>
            {
                { "connectionTypeOptions", connectionTypeOptions.ToCamelCaseJson(false, true) },
                { "connectionType", config?.ConnectionTypeGuid.ToString() },
                { "connectionOpportunityOptions", connectionOpportunityOptions.ToCamelCaseJson(false, true) },
                { "connectionOpportunity", config?.ConnectionOpportunityGuid.ToString() },
            };

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionConfig = new SelectionConfig
            {
                ConnectionTypeGuid = data.GetValueOrNull( "connectionType" )?.AsGuid(),
                ConnectionOpportunityGuid = data.GetValueOrNull( "connectionOpportunity" )?.AsGuid(),
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
            return "Connection Opportunity";
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
  var connectionOpportunity = $('.js-connectionopportunity-picker option:selected', $content).text();
  var connectionType = $('.js-connectiontype-picker option:selected', $content).text();
  var result = 'Connection Opportunity';
  if (connectionOpportunity && connectionType) {
     result = 'Connection Opportunity: '+ connectionOpportunity + ' in '+ connectionType + ' Connection Type';
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
            string result = "Connection Opportunity";
            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.ConnectionOpportunityGuid.HasValue )
            {
                var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( selectionConfig.ConnectionOpportunityGuid.Value );
                if ( connectionOpportunity != null )
                {
                    result = string.Format( "Connection Opportunity: {0} in {1} Connection Type", connectionOpportunity.Name, connectionOpportunity.ConnectionType.Name );
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
            var connectionTypePicker = new RockDropDownList();
            connectionTypePicker.ID = filterControl.ID + "_connectionTypePicker";
            connectionTypePicker.Label = "Connection Type";
            connectionTypePicker.SelectedIndexChanged += connectionTypePicker_SelectedIndexChanged;
            connectionTypePicker.AutoPostBack = true;
            connectionTypePicker.CssClass = "js-connectiontype-picker";
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

            var connectionOpportunityPicker = new RockDropDownList();
            connectionOpportunityPicker.CssClass = "js-connectionopportunity-picker";
            connectionOpportunityPicker.ID = filterControl.ID + "_connectionOpportunityPicker";
            connectionOpportunityPicker.Label = "Connection Opportunity";
            filterControl.Controls.Add( connectionOpportunityPicker );
            PopulateConnectionOpportunityDropdownList( filterControl );

            return new Control[2] { connectionTypePicker, connectionOpportunityPicker };
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
            var connectionTypePicker = controls[0] as RockDropDownList;
            var connectionOpportunityPicker = controls[1] as RockDropDownList;

            var connectionOpportunityId = connectionOpportunityPicker.SelectedValueAsId();
            var connectionTypeId = connectionTypePicker.SelectedValueAsId();
            if ( connectionTypeId.HasValue && connectionOpportunityId.HasValue )
            {
                var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityId.Value );
                if ( connectionOpportunity != null )
                {
                    selectionConfig.ConnectionTypeGuid = connectionOpportunity.ConnectionType.Guid;
                    selectionConfig.ConnectionOpportunityGuid = connectionOpportunity.Guid;
                }
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
                if ( controls.Length > 0 && selectionConfig.ConnectionOpportunityGuid.HasValue )
                {
                    var connectionTypePicker = controls[0] as RockDropDownList;
                    var connectionType = new ConnectionTypeService( new RockContext() ).Get( selectionConfig.ConnectionTypeGuid.Value );
                    if ( connectionType != null )
                    {
                        connectionTypePicker.SetValue( connectionType.Id );
                    }

                    connectionTypePicker_SelectedIndexChanged( connectionTypePicker, new EventArgs() );

                    var connectionOpportunityGuid = selectionConfig.ConnectionOpportunityGuid.Value;
                    var connectionOpportunityPicker = controls[1] as RockDropDownList;
                    var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityGuid );
                    if ( connectionOpportunity != null )
                    {
                        connectionOpportunityPicker.SetValue( connectionOpportunity.Id );
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
            if ( !selectionConfig.ConnectionOpportunityGuid.HasValue )
            {
                return null;
            }

            var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( selectionConfig.ConnectionOpportunityGuid.Value );
            int? connectionOpportunityId = null;
            if ( connectionOpportunity != null )
            {
                connectionOpportunityId = connectionOpportunity.Id;
            }

            var qry = new ConnectionRequestService( ( RockContext ) serviceInstance.Context ).Queryable()
                .Where( p => p.ConnectionOpportunityId == connectionOpportunityId );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.ConnectionRequest>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Handles the SelectedIndexChanged event of the connectionTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void connectionTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            PopulateConnectionOpportunityDropdownList( filterField );
        }

        /// <summary>
        /// Populates the connection opportunity list.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        private void PopulateConnectionOpportunityDropdownList( FilterField filterField )
        {
            var connectionTypePicker = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-connectiontype-picker" ) );
            var connectionOpportunityPicker = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-connectionopportunity-picker" ) );
            var connectionTypeId = connectionTypePicker.SelectedValueAsId();

            if ( connectionTypeId.HasValue )
            {
                connectionOpportunityPicker.Items.Clear();
                var connectionOpportunityList = new ConnectionOpportunityService( new RockContext() ).Queryable().Where( a => a.ConnectionTypeId == connectionTypeId.Value && a.IsActive )
                 .OrderBy( a => a.Order )
                 .ThenBy( a => a.Name )
                 .ToList();
                foreach ( var connectionOpportunity in connectionOpportunityList )
                {
                    connectionOpportunityPicker.Items.Add( new ListItem( connectionOpportunity.Name, connectionOpportunity.Id.ToString() ) );
                }

                connectionOpportunityPicker.Visible = connectionOpportunityPicker.Items.Count > 0;
            }
            else
            {
                connectionOpportunityPicker.Visible = false;
            }
        }
#endif

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
            /// Gets or sets the connection opportunity identifiers.
            /// </summary>
            /// <value>
            /// The connection opportunity identifiers.
            /// </value>
            public Guid? ConnectionOpportunityGuid { get; set; }

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