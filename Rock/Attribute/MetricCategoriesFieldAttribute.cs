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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more Metrics
    /// Technically, in order to support putting a metric in multiple categories, you are selecting a MetricCategory, then getting the MetricCategory.Metric
    /// NOTE: Stored as a List of Metric.Guid|MetricCategory.Guid (MetricCategory.Guid included so we can preserve which category the metric was selected from)
    /// HINT: use Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue( "...." ));
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class MetricCategoriesFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricCategoriesFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultMetricsGuids">The default metrics guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public MetricCategoriesFieldAttribute( string name = "Metrics", string description = "", bool required = true, string defaultMetricsGuids = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultMetricsGuids, category, order, key, typeof( Rock.Field.Types.MetricCategoriesFieldType ).FullName )
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public class MetricCategoryPair
        {
            /// <summary>
            /// Gets or sets the metric unique identifier.
            /// </summary>
            /// <value>
            /// The metric unique identifier.
            /// </value>
            public Guid MetricGuid { get; set; }

            /// <summary>
            /// Gets or sets the category unique identifier.
            /// </summary>
            /// <value>
            /// The category unique identifier.
            /// </value>
            public Guid? CategoryGuid { get; set; }
        }

        /// <summary>
        /// Gets the value as unique identifier pairs.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static List<MetricCategoryPair> GetValueAsGuidPairs( string value )
        {
            return (value ?? string.Empty).Split( ',' ).Select( a =>
            {
                var parts = a.Split( '|' );

                if ( parts.Length == 2 )
                {
                    return new MetricCategoryPair { MetricGuid = parts[0].AsGuid(), CategoryGuid = parts[1].AsGuidOrNull() };
                }
                else
                {
                    return null;
                }

            } ).Where( a => a != null ).ToList();
        }
    }
}