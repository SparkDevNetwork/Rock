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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting.DataFilter.PrayerRequest
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter prayer requests that are associated with a specific campus." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Prayer Request Campus Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "D90AF16E-E4B0-400F-9217-68A71B080B26" )]
    public class CampusFilter : BaseCampusFilter, IUpdateSelectionFromPageParameters
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
            get { return typeof( Rock.Model.PrayerRequest ).FullName; }
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
            return $@"
function() {{
    var campusPicker = $('.{this.ControlClassName}', $content);
    var campusName = $(':selected', campusPicker).text();

    return 'Campus: ' + campusName;
}}";
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
                Guid campusGuid = selectionValues[0].AsGuid();
                var campus = CampusCache.Get( campusGuid );
                if ( campus != null )
                {
                    result = "Campus: " + campus.Name;
                }
            }

            return result;
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Updates the selection from page parameters.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public string UpdateSelectionFromPageParameters( string selection, Rock.Web.UI.RockBlock rockBlock )
        {
            string[] selectionValues = selection?.Split( '|' ) ?? new string[] { "" };
            if ( selectionValues.Length >= 1 )
            {
                var campusId = rockBlock.PageParameter( "CampusId" ).AsIntegerOrNull();
                if ( campusId == null )
                {
                    var campusEntity = rockBlock.ContextEntity<Campus>();
                    if ( campusEntity != null )
                    {
                        campusId = campusEntity.Id;
                    }
                }

                if ( campusId.HasValue )
                {
                    var selectedCampus = CampusCache.Get( campusId.Value );
                    if ( selectedCampus != null )
                    {
                        selectionValues[0] = selectedCampus.Guid.ToString();
                        return selectionValues.ToList().AsDelimited( "|" );
                    }
                }
            }

            return selection;
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
            var rockContext = ( RockContext ) serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var campus = CampusCache.Get( selectionValues[0].AsGuid() );
                if ( campus == null )
                {
                    return null;
                }

                var qry = new PrayerRequestService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => ( p.CampusId ?? 0 ) == campus.Id );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.PrayerRequest>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}
