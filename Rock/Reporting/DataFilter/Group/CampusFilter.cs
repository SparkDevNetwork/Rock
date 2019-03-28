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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter groups by campus" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Campus Filter" )]
    public class CampusFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.Group ).FullName; }
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
            return "Campus";
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
  var campusName = $('.campus-picker', $content).find(':selected').text()
  
  if (campusName || '' != '') {
    return 'Campus: ' + campusName;
  }
  else {
    return 'No Campus';
  }

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
            string result = "Campus";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                int? campusId = GetCampusIdFromSelection( selectionValues );
                if ( campusId.HasValue )
                {
                    var campus = CampusCache.Get( campusId.Value );
                    if ( campus != null )
                    {
                        result = string.Format( "Campus: {0}", campus.Name );
                    }
                }
                else
                {
                    result = "No Campus";
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
            CampusPicker campusPicker = new CampusPicker();
            campusPicker.ID = filterControl.ID + "_campusPicker";
            campusPicker.Label = "Campus";
            campusPicker.Campuses = CampusCache.All();
            filterControl.Controls.Add( campusPicker );

            return new Control[] { campusPicker };
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
            int? campusId = ( controls[0] as CampusPicker ).SelectedCampusId;
            if ( campusId.HasValue )
            {
                var campus = CampusCache.Get( campusId.Value );
                if ( campus != null )
                {
                    return campus.Guid.ToString();
                }
            }

            return string.Empty;
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
                int? campusId = GetCampusIdFromSelection( selectionValues );
                ( controls[0] as CampusPicker ).SetValue( campusId );
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
                int? campusId = GetCampusIdFromSelection( selectionValues );

                var qry = new GroupService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => p.CampusId == campusId );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Group>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        /// <summary>
        /// Gets the campus identifier from selection.
        /// </summary>
        /// <param name="selectionValues">The selection values.</param>
        /// <returns></returns>
        private static int? GetCampusIdFromSelection( string[] selectionValues )
        {
            Guid? campusGuid = selectionValues[0].AsGuidOrNull();
            int? campusId = null;
            if ( campusGuid.HasValue )
            {
                var campus = CampusCache.Get( campusGuid.Value );
                if ( campus != null )
                {
                    campusId = campus.Id;
                }
            }
            return campusId;
        }

        #endregion
    }
}