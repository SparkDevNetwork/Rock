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
    public class NumericPropertyFilterSettings
    {
        /// <summary>
        /// Gets or sets the type of comparison to use when evaluating the filter.
        /// </summary>
        public ComparisonType Comparison { get; set; } = ComparisonType.EqualTo;

        public decimal? LowerValue { get; set; }
        public decimal? UpperValue { get; set; }
    }

    public class NumericPropertyDataFilterFactory : NumericDataFilterFactoryBase
    {
        internal override Type FilterEntityType => typeof( Rock.Reporting.DataFilter.PropertyFilter );

        public override string FilterStringFormat => "json";
    }


    public class AgeDataFilterFactory : NumericDataFilterFactoryBase
    {
        internal override Type FilterEntityType => typeof( Rock.Reporting.DataFilter.Person.AgeFilter );

        public override string FilterStringFormat => "delimited";
    }

    /// <summary>
    /// A factory for creating query filters for a numeric range.
    /// </summary>
    /// <remarks>
    /// This class provides a simple means of constructing a valid DataViewFilter for a numeric property that can be used in a DataView.
    /// </remarks>
    public abstract class NumericDataFilterFactoryBase
    {
        public virtual string FilterStringFormat
        {
            get
            {
                return "json";
            }
        }

        /// <summary>
        /// Gets or sets the name of the property to which the filter will be applied.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter value.
        /// </summary>
        public string Value { get; set; }

        public void ValidateSettings( NumericPropertyFilterSettings settings )
        {
            if ( string.IsNullOrWhiteSpace( PropertyName ) )
            {
                throw new Exception( "Property Name is required." );
            }

            if ( string.IsNullOrWhiteSpace( Value )
                 && ( settings.Comparison != ComparisonType.IsBlank && settings.Comparison != ComparisonType.IsNotBlank ) )
            {
                throw new Exception( "Value is required for the selected Comparison type." );
            }
        }

        /// <summary>
        /// Gets a DataViewFilter instance for the current settings.
        /// </summary>
        /// <returns></returns>
        public virtual DataViewFilter GetFilter( NumericPropertyFilterSettings settings )
        {
            var filter = new DataViewFilter();
            filter.ExpressionType = FilterExpressionType.Filter;
            filter.EntityTypeId = EntityTypeCache.GetId( this.FilterEntityType );

            if ( this.FilterStringFormat.ToLower() == "delimited" )
            {
                filter.Selection = GetSettingsString( settings );
            }
            else
            {
                filter.Selection = GetSettingsString( settings );
            }

            return filter;
        }

        /// <summary>
        /// The Entity Type that implements the filter logic.
        /// </summary>
        internal virtual Type FilterEntityType
        {
            get
            {
                return typeof( Rock.Reporting.DataFilter.PropertyFilter );
            }
        }

        /// <summary>
        /// Gets the settings for the filter as a formatted string.
        /// </summary>
        /// <returns></returns>
        public string GetSettingsString( NumericPropertyFilterSettings settings )
        {
            if ( this.FilterStringFormat == "delimited" )
            {
                return OnGetSettingsAsDelimitedString( settings );
            }
            else
            {
                return OnGetSettingsAsJsonString( settings );
            }
        }

        /// <summary>
        /// Gets the settings for the filter as a formatted string.
        /// </summary>
        /// <returns></returns>
        protected virtual string OnGetSettingsAsDelimitedString( NumericPropertyFilterSettings settings )
        {
            var settingsString = $"{( int ) settings.Comparison}";

            var lowerValue = settings.LowerValue ?? 0;

            if ( settings.Comparison == ComparisonType.Between )
            {
                var upperValue = settings.UpperValue ?? decimal.MaxValue;

                settingsString += $"|{lowerValue},{upperValue}";
            }
            else
            {
                settingsString += $"|{lowerValue}";
            }

            return settingsString;
        }

        /// <summary>
        /// Gets the settings for the filter as a formatted string.
        /// </summary>
        /// <returns></returns>
        protected virtual string OnGetSettingsAsJsonString( NumericPropertyFilterSettings settings )
        {
            var settingsList = new List<string>();

            settingsList.Add( $"{( int ) settings.Comparison}" );

            var lowerValue = settings.LowerValue ?? 0;

            if ( settings.Comparison == ComparisonType.Between )
            {
                var upperValue = settings.UpperValue ?? decimal.MaxValue;

                settingsList.Add( $"|{lowerValue},{upperValue}" );
            }
            else
            {
                settingsList.Add( $"{lowerValue}" );
            }

            var settingsJson = settingsList.ToJson();
            return settingsJson;
        }

        /// <summary>
        /// Gets the settings for the filter from a formatted string.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public static NumericPropertyFilterSettings GetSettingsFromString( string selection )
        {
            var settings = new NumericPropertyFilterSettings();

            var values = selection.Split( '|' );
            if ( values.Length >= 2 )
            {
                settings.Comparison = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                settings.LowerValue = values[1].AsDecimalOrNull();

                if ( values.Length >= 3 )
                {
                    var valuesList = values[2].ToStringSafe().Split( ',' );
                    if ( valuesList.Length > 0 )
                    {
                        settings.LowerValue = valuesList[0].AsDecimalOrNull();
                    }
                    if ( valuesList.Length > 1 )
                    {
                        settings.UpperValue = valuesList[1].AsDecimalOrNull();
                    }
                }
            }

            return settings;
        }
    }
}
