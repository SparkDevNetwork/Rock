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
    /// Service/Data access class for <see cref="Rock.Model.Metric"/> entity objects.
    /// </summary>
    public partial class MetricService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Metric">Metrics</see> by the type flag.
        /// </summary>
        /// <param name="type">A <see cref="System.Boolean"/> that represents the type flag value to search by. <remarks>Type refers to 
        ///  if multiple values are a part of the Metric. When <c>true</c> multiple values will be returned, otherwise <c>false</c></remarks>.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Metric">Metrics</see> with a specified Type value.
        /// </returns>
        public IEnumerable<Metric> GetByType( bool? type )
        {
            return Repository.Find( t => ( t.Type == type || ( type == null && t.Type == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Metric"/> by it's Id value.
        /// </summary>
        /// <param name="metricId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Metric"/> to search for/return.</param>
        /// <returns>The <see cref="Rock.Model.Metric"/> with the provided Id value. If a matching <see cref="Rock.Model.Metric"/> is not found, null will be returned.</returns>
        public Metric GetById( int? metricId )
        {
            return Repository.FirstOrDefault( t => t.Id == metricId );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Metric"/> by it's Guid value.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> value representing the Guid value of the <see cref="Rock.Model.Metric"/> to search for/return.</param>
        /// <returns>The <see cref="Rock.Model.Metric"/> with a provided Guid value. If a <see cref="Rock.Model.Metric"/> is not found, null will be returned.</returns>
        public Metric GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
    }
}
