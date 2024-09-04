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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter
{
    /// <summary>
    /// Represents the settings for a DataFilter that can be applied to the text property of an entity.
    /// </summary>
    /// <remarks>
    /// This class provides a simple means of constructing a valid DataViewFilter for an entity text property that can be used in a DataView.
    /// </remarks>
    public class TextPropertyFilterSettings
    {
        /// <summary>
        /// Gets or sets the type of comparison to use when evaluating the filter.
        /// </summary>
        public ComparisonType Comparison { get; set; } = ComparisonType.EqualTo;

        /// <summary>
        /// Gets or sets the name of the property to which the filter will be applied.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter value.
        /// </summary>
        public string Value { get; set; }

        public void ValidateSettings()
        {
            if ( string.IsNullOrWhiteSpace( PropertyName ) )
            {
                throw new Exception( "Property Name is required." );
            }

            if ( string.IsNullOrWhiteSpace( Value )
                 && ( Comparison != ComparisonType.IsBlank && Comparison != ComparisonType.IsNotBlank ) )
            {
                throw new Exception( "Value is required for the selected Comparison type." );
            }
        }

        /// <summary>
        /// Gets a DataViewFilter instance for the current settings.
        /// </summary>
        /// <returns></returns>
        public DataViewFilter GetFilter()
        {
            ValidateSettings();

            var filter = new DataViewFilter();
            filter.ExpressionType = FilterExpressionType.Filter;
            filter.EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Reporting.DataFilter.PropertyFilter ) );
            filter.Selection = GetSettingsString();

            return filter;
        }

        /// <summary>
        /// Gets the settings for the filter as a JSON string.
        /// </summary>
        /// <returns></returns>
        public string GetSettingsString()
        {
            var settings = new List<string> { $"Property_{PropertyName}", Comparison.ConvertToInt().ToString(), Value };
            var json = settings.ToJson();
            return json;
        }
    }
}
