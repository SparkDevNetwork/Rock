using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using church.ccv.Datamart.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace church.ccv.Datamart.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people that are associated with a specific campus using Datamart." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Datamart Campus Filter" )]
    public class DatamartCampusFilter : Rock.Reporting.DataFilter.Person.CampusFilter
    {
        #region Properties

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Datamart Filters"; }
        }

        #endregion

        #region Public Methods

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
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var campus = CampusCache.Read( selectionValues[0].AsGuid() );
                if ( campus == null )
                {
                    return null;
                }

                string campusName = campus.Name;

                var datamartPersonService = new Service<DatamartPerson>( rockContext );
                var qryDatamartPerson = datamartPersonService.Queryable().Where( a => a.CampusName == campusName );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}
