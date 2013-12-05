//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.MetricValue"/> entity objects.
    /// </summary>
    public partial class MetricValueService 
    {
        /// <summary>
        /// Gets MetricValues by MetricId
        /// Returns an enumerable collection of <see cref="Rock.Model.MetricValue">MetricValues</see> by the MetricId of the <see cref="Rock.Model.Metric"/>
        /// that they are associated with.
        /// </summary>
        /// <param name="metricId">A <see cref="System.Int32"/> representing the MetricId to retreive <see cref="Rock.Model.MetricValue">MetricvValues</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.MetricValue">MetricValues</see> that belong to the specified <see cref="Rock.Model.Metric"/></returns>
        public IEnumerable<Rock.Model.MetricValue> GetByMetricId( int? metricId )
        {
            return Repository.Find( t => ( t.MetricId == metricId || ( metricId == null && t.MetricId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.MetricValue"/> by it's GUID identifier.
        /// </summary>
        /// <param name="guid">The GUID value to search by.</param>
        /// <returns>The <see cref="Rock.Model.MetricValue"/> that contains the provided Guid. If no match is found, this value will be null.</returns>
        public MetricValue GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
    }
}
