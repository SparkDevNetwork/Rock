//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.UI;


namespace Rock.Web.Cache
{
    /// <summary>
    /// The value of a metric
    /// </summary>
    [Serializable]
    public class MetricValue : Rock.Core.MetricValueDto
    {
		private MetricValue() : base() { }
		private MetricValue( Rock.Core.MetricValue model ) : base( model ) { }

		#region Static Methods

		private static string CacheKey( int id )
		{
			return string.Format( "Rock:MetricValue:{0}", id );
		}

		/// <summary>
		/// Adds MetricValue model to cache, and returns cached object
		/// </summary>
		/// <param name="metricValueModel">The metricValueModel to cache</param>
		/// <returns></returns>
		public static MetricValue Read( Rock.Core.MetricValue metricValueModel )
        {
            string cacheKey = MetricValue.CacheKey( metricValueModel.Id );

            ObjectCache cache = MemoryCache.Default;
            MetricValue metricValue = cache[cacheKey] as MetricValue;

			if ( metricValue != null )
				return metricValue;
			else
			{
				metricValue = MetricValue.CopyModel( metricValueModel );
				cache.Set( cacheKey, metricValue, new CacheItemPolicy() );

				return metricValue;
			}
        }

		/// <summary>
		/// Returns MetricValue object from cache.  If metricValue does not already exist in cache, it
		/// will be read and added to cache
		/// </summary>
		/// <param name="id">The id of the MetricValue to read</param>
		/// <returns></returns>
		public static MetricValue Read( int id )
		{
			string cacheKey = MetricValue.CacheKey( id );

			ObjectCache cache = MemoryCache.Default;
			MetricValue metricValue = cache[cacheKey] as MetricValue;

			if ( metricValue != null )
				return metricValue;
			else
			{
				Rock.Core.MetricValueService metricService = new Rock.Core.MetricValueService();
				Rock.Core.MetricValue metricValueModel = metricService.Get( id );
				if ( metricValueModel != null )
				{
					metricValue = MetricValue.CopyModel( metricValueModel );

					cache.Set( cacheKey, metricValue, new CacheItemPolicy() );

					return metricValue;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Copies the properties of a <see cref="Rock.Core.MetricValue"/> object to a <see cref="MetricValue"/> object/>
        /// </summary>
        /// <param name="metricValueModel">The metricValuemodel.</param>
        /// <returns></returns>
        public static MetricValue CopyModel( Rock.Core.MetricValue metricValueModel )
        {
			MetricValue metricValue = new MetricValue( metricValueModel );
			return metricValue;
		}

		/// <summary>
		/// Removes metricValue from cache
		/// </summary>
		/// <param name="id">The id of the metricValue to remove from cache</param>
		public static void Flush( int id )
		{
			ObjectCache cache = MemoryCache.Default;
			cache.Remove( MetricValue.CacheKey( id ) );
		}

		#endregion
	}
}