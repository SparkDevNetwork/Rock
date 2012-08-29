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
	/// MetricValue POCO Service class
	/// </summary>
    public partial class MetricValueService : Service<MetricValue, MetricValueDTO>
    {
		/// <summary>
		/// Gets MetricValues by MetricId
		/// </summary>
		/// <param name="metricId">metricId.</param>
		/// <returns>An enumerable list of MetricValue objects.</returns>
	    public IEnumerable<MetricValue> MetricId( int? metricId )
        {
			return Repository.Find( t => ( t.MetricId == metricId || ( metricId == null && t.MetricId == null ) ) ).OrderBy( t => t.Order );
        }

		/// <summary>
		/// Gets MetricValue by Guid
		/// </summary>
		/// <param name="guid">Guid.</param>
		/// <returns>MetricValue object.</returns>
		public MetricValue GetByGuid( Guid guid )
		{
			return Repository.FirstOrDefault( t => t.Guid == guid );
		}
    }
}
