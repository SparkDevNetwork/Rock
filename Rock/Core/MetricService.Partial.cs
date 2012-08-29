//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Core
{
	/// <summary>
	/// Metric POCO Service class
	/// </summary>
    public partial class MetricService : Service<Metric, MetricDto>
    {
		/// <summary>
		/// Gets Metrics by Type
		/// </summary>
		/// <param name="entity">Type.</param>
		/// <returns>An enumerable list of Metric objects.</returns>
	    public IEnumerable<Metric> GetByType( bool? type )
        {
            return Repository.Find( t => ( t.Type == type || ( type == null && t.Type == null ) ) ).OrderBy( t => t.Order );
        }

		/// <summary>
		/// Gets Metric by Id
		/// </summary>
		/// <param name="metricId">metricId.</param>
		/// <returns>Metric object.</returns>
		public Metric GetById( int? metricId )
		{
			return Repository.FirstOrDefault( t => t.Id == metricId );
		}

		/// <summary>
		/// Gets Metric by Guid
		/// </summary>
		/// <param name="guid">Guid.</param>
		/// <returns>Metric object.</returns>
		public Metric GetByGuid( Guid guid )
		{
			return Repository.FirstOrDefault( t => t.Guid == guid );
		}
    }
}
