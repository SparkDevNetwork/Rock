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
    public class MetricValueCache : Rock.Core.MetricValueDto
    {
		private MetricValueCache() : base() { }
		private MetricValueCache( Rock.Core.MetricValue model ) : base( model ) { }

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
		public static MetricValueCache Read( Rock.Core.MetricValue metricValueModel )
        {
            string cacheKey = MetricValueCache.CacheKey( metricValueModel.Id );

            ObjectCache cache = MemoryCache.Default;
            MetricValueCache metricValue = cache[cacheKey] as MetricValueCache;

			if ( metricValue != null )
				return metricValue;
			else
			{
				metricValue = MetricValueCache.CopyModel( metricValueModel );
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
		public static MetricValueCache Read( int id )
		{
			string cacheKey = MetricValueCache.CacheKey( id );

			ObjectCache cache = MemoryCache.Default;
			MetricValueCache metricValue = cache[cacheKey] as MetricValueCache;

			if ( metricValue != null )
				return metricValue;
			else
			{
				Rock.Core.MetricValueService metricService = new Rock.Core.MetricValueService();
				Rock.Core.MetricValue metricValueModel = metricService.Get( id );
				if ( metricValueModel != null )
				{
					metricValue = MetricValueCache.CopyModel( metricValueModel );

					cache.Set( cacheKey, metricValue, new CacheItemPolicy() );

					return metricValue;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Copies the properties of a <see cref="Rock.Core.MetricValue"/> object to a <see cref="MetricValueCache"/> object/>
        /// </summary>
        /// <param name="metricValueModel">The metricValuemodel.</param>
        /// <returns></returns>
        public static MetricValueCache CopyModel( Rock.Core.MetricValue metricValueModel )
        {
			MetricValueCache metricValue = new MetricValueCache( metricValueModel );
			return metricValue;
		}

		/// <summary>
		/// Removes metricValue from cache
		/// </summary>
		/// <param name="id">The id of the metricValue to remove from cache</param>
		public static void Flush( int id )
		{
			ObjectCache cache = MemoryCache.Default;
			cache.Remove( MetricValueCache.CacheKey( id ) );
		}

		#endregion
	}
}