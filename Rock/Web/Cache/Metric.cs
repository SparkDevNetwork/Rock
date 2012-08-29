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
	/// Information about an metric that is required by the rendering engine.
	/// This information will be cached by Rock. 
	/// 
	/// NOTE: Because this metric object is cached and shared by all entities 
	/// using the metric, a particlar instance's values are not included as a 
	/// property of this metric object.
	/// </summary>
	public class Metric
	{
		/// <summary>
		/// Use Static Read() method to instantiate a new Metric object
		/// </summary>
		private Metric() { }

		/// <summary>
		/// Gets the id.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Gets or sets the Type.
		/// </summary>
		public bool Type { get; set; }

		/// <summary>
		/// Gets the category.
		/// </summary>
		public string Category
		{
			get
			{
				return string.IsNullOrEmpty( category ) ? "Metrics" : category;
			}

			private set
			{
				if ( value == "Metrics" )
					category = null;
				else
					category = value;
			}
		}
		private string category;

		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the Subtitle.
		/// </summary>
		public string Subtitle { get; set; }

		/// <summary>
		/// Gets the description.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Gets or sets the MinValue.
		/// </summary>
		public int MinValue { get; set; }

		/// <summary>
		/// Gets or sets the MaxValue.
		/// </summary>
		public int MaxValue { get; set; }

		/// <summary>
		/// Gets or sets the CollectionFrequency.
		/// </summary>
		public int CollectionFrequencyId { get; set; }

		/// <summary>
		/// Gets or sets the LastCollected date.
		/// </summary>
		public DateTime? LastCollected { get; set; }

		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// Gets or sets the SourceSQL.
		/// </summary>
		public string SourceSQL { get; set; }

		#region Static Methods

		private static string CacheKey( int id )
		{
			return string.Format( "Rock:Metric:{0}", id );
		}

		/// <summary>
		/// Adds Metric model to cache, and returns cached object
		/// </summary>
		/// <param name="metricModel">The metricModel to cache</param>
		/// <returns></returns>
		public static Metric Read( Rock.Core.Metric metricModel )
		{
			Metric metric = Metric.CopyModel( metricModel );

			string cacheKey = Metric.CacheKey( metricModel.Id );
			ObjectCache cache = MemoryCache.Default;
			cache.Set( cacheKey, metric, new CacheItemPolicy() );

			return metric;
		}

		/// <summary>
		/// Returns Metric object from cache.  If metric does not already exist in cache, it
		/// will be read and added to cache
		/// </summary>
		/// <param name="id">The id of the Metric to read</param>
		/// <returns></returns>
		public static Metric Read( int id )
		{
			string cacheKey = Metric.CacheKey( id );

			ObjectCache cache = MemoryCache.Default;
			Metric metric = cache[cacheKey] as Metric;

			if ( metric != null )
				return metric;
			else
			{
				Rock.Core.MetricService metricService = new Rock.Core.MetricService();
				Rock.Core.Metric metricModel = metricService.Get( id );
				if ( metricModel != null )
				{
					metric = Metric.CopyModel( metricModel );

					cache.Set( cacheKey, metric, new CacheItemPolicy() );

					return metric;
				}
				else
					return null;

			}
		}

		/// <summary>
		/// Copies the properties of a <see cref="Rock.Core.Metric"/> object to a <see cref="Metric"/> object/>
		/// </summary>
		/// <param name="metricModel">The metric model.</param>
		/// <returns></returns>
		public static Metric CopyModel( Rock.Core.Metric metricModel )
		{
			Metric metric = new Metric();
			metric.Id = metricModel.Id;
			metric.Type = metricModel.Type;
			metric.Category = metricModel.Category;
			metric.Title = metricModel.Title;
			metric.Subtitle = metricModel.Subtitle;
			metric.Description = metricModel.Description;
			metric.MinValue = metricModel.MinValue;
			metric.MaxValue = metricModel.MaxValue;
			metric.CollectionFrequencyId = metricModel.CollectionFrequencyId;
			metric.LastCollected = metricModel.LastCollected;
			metric.Source = metricModel.Source;
			metric.SourceSQL = metricModel.SourceSQL;

			// copy metric values?         

			return metric;
		}

		/// <summary>
		/// Removes metric from cache
		/// </summary>
		/// <param name="id">The id of the metric to remove from cache</param>
		public static void Flush( int id )
		{
			ObjectCache cache = MemoryCache.Default;
			cache.Remove( Metric.CacheKey( id ) );
		}

		#endregion
	}
}