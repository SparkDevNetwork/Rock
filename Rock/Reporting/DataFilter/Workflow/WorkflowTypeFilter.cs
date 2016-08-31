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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter workflows by workflow type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Type Filter" )]
    public class WorkflowTypeFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.Workflow ).FullName; }
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
            return "Workflow Type";
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

  var workflowTypeName = $('.js-workflow-type-picker', $content).find('.js-item-name-value').val();
  var result = 'Workflow type: ' + workflowTypeName;

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
            string result = "Workflow Type";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );

                if ( workflowType != null )
                {
                    result = string.Format( "Workflow type: {0}", workflowType.Name );
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
            WorkflowTypePicker workflowTypePicker = new WorkflowTypePicker();
            workflowTypePicker.ID = filterControl.ID + "_workflowTypePicker";
            workflowTypePicker.CssClass = "js-workflow-type-picker";
            workflowTypePicker.Label = "Workflow Type";
            filterControl.Controls.Add( workflowTypePicker );

            return new Control[] { workflowTypePicker };
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
            int? workflowTypeId = ( controls[0] as WorkflowTypePicker ).SelectedValueAsId();
            Guid? workflowTypeGuid = null;
            var workflowType = new WorkflowTypeService( new RockContext() ).Get( workflowTypeId ?? 0 );
            if ( workflowType != null )
            {
                workflowTypeGuid = workflowType.Guid;
            }

            return workflowTypeGuid.ToString();
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
                var workflowType = new WorkflowTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                if ( workflowType != null )
                {
                    ( controls[0] as WorkflowTypePicker ).SetValue( workflowType.Id );
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
            if ( selectionValues.Length >= 1 )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                int? workflowTypeId = null;
                if ( workflowType != null )
                {
                    workflowTypeId = workflowType.Id;
                }

                var qry = new WorkflowService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => p.WorkflowTypeId == workflowTypeId );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Workflow>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}