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
	public class MetricValue
	{
		/// <summary>
		/// Use Static Read() method to instantiate a new MetricValue object
		/// </summary>
		private MetricValue() { }

		/// <summary>
		/// Gets the id.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Gets the MetricId.
		/// </summary>
		public int MetricId { get; private set; }

		/// <summary>
		/// Gets or sets the Value.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Gets the description.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Gets or sets the xValue.
		/// </summary>
		public int xValue { get; set; }

		/// <summary>
		/// Gets or sets the isDateBased flag.
		/// </summary>
		public bool isDateBased { get; set; }

		/// <summary>
		/// Gets or sets the Label.
		/// </summary>
		public string Label { get; set; }

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
			MetricValue metricValue = MetricValue.CopyModel( metricValueModel );

			string cacheKey = MetricValue.CacheKey( metricValueModel.Id );
			ObjectCache cache = MemoryCache.Default;
			cache.Set( cacheKey, metricValue, new CacheItemPolicy() );

			return metricValue;
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
			MetricValue metricValue = new MetricValue();
			metricValue.Id = metricValueModel.Id;
			metricValue.MetricId = metricValueModel.MetricId;
			metricValue.Value = metricValueModel.Value;
			metricValue.Description = metricValueModel.Description;
			metricValue.xValue = metricValueModel.xValue;
			metricValue.isDateBased = metricValueModel.isDateBased;
			metricValue.Label = metricValueModel.Label;

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