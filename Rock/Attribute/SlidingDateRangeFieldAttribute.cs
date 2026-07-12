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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for setting a Sliding Date range.  For example, Last 7 days
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class SlidingDateRangeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Enabled SlidingDateRangeTypes
        /// </summary>
        protected const string ENABLED_SLIDING_DATE_RANGE_TYPES = "enabledSlidingDateRangeTypes";

        /// <summary>
        /// Enabled SlidingDateRangeUnits
        /// </summary>
        protected const string ENABLED_SLIDING_DATE_RANGE_UNITS = "enabledSlidingDateRangeUnits";

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingDateRangeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="enabledSlidingDateRangeTypes">The enabled sliding date range types. Choose from: 'Previous, Last, Current, Next, Upcoming, DateRange'</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public SlidingDateRangeFieldAttribute( string name, string description, bool required, string defaultValue, string category, int order, string key,
            string enabledSlidingDateRangeTypes )
            : this( name, description, required, defaultValue, category, order, key, enabledSlidingDateRangeTypes, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingDateRangeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="enabledSlidingDateRangeTypes">The enabled sliding date range types. Choose from: 'Previous, Last, Current, Next, Upcoming, DateRange'</param>
        /// <param name="enabledSlidingDateRangeUnits">The enabled sliding date range units.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public SlidingDateRangeFieldAttribute( string name, string description = "", bool required = true, string defaultValue = ",", string category = "", int order = 0, string key = null,
            string enabledSlidingDateRangeTypes = null, string enabledSlidingDateRangeUnits = null )
            : base( SystemGuid.FieldType.SLIDING_DATE_RANGE.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            if ( enabledSlidingDateRangeTypes != null )
            {
                EnabledSlidingDateRangeTypes = enabledSlidingDateRangeTypes;
            }
            if ( enabledSlidingDateRangeUnits != null )
            {
                EnabledSlidingDateRangeUnits = enabledSlidingDateRangeUnits;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingDateRangeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SlidingDateRangeFieldAttribute( string name )
            : base( SystemGuid.FieldType.SLIDING_DATE_RANGE.AsGuid(), name )
        {
        }

        /// <summary>
        /// Gets or sets the enabled sliding date range types.
        /// </summary>
        /// <value>
        /// The enabled sliding date range types.
        /// </value>
        public string EnabledSlidingDateRangeTypes
        {
            get => FieldConfigurationValues.GetValueOrNull( ENABLED_SLIDING_DATE_RANGE_TYPES );
            set => FieldConfigurationValues.AddOrReplace( ENABLED_SLIDING_DATE_RANGE_TYPES, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// Gets or sets the enabled sliding date range units.
        /// </summary>
        /// <value>
        /// The enabled sliding date range units.
        /// </value>
        public string EnabledSlidingDateRangeUnits
        {
            get => FieldConfigurationValues.GetValueOrNull( ENABLED_SLIDING_DATE_RANGE_UNITS );
            set => FieldConfigurationValues.AddOrReplace( ENABLED_SLIDING_DATE_RANGE_UNITS, new Field.ConfigurationValue( value ) );
        }
    }
}
