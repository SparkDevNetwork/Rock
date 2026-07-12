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
    /// Field Attribute to select 0 or more Campuses stored as a comma-delimited list of Campus.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CampusesFieldAttribute : SelectFieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string FILTER_CAMPUS_TYPES_KEY = "filterCampusTypes";
        private const string FILTER_CAMPUS_STATUS_KEY = "filterCampusStatus";

        /// <summary>
        /// Initializes a new instance of the <see cref="CampusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultCampusGuids">The default campus guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public CampusesFieldAttribute( string name = "Campuses", string description = "", bool required = true, string defaultCampusGuids = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.CAMPUSES.AsGuid(), name )
        {
            Category = category;
            DefaultValue = defaultCampusGuids;
            Description = description;
            IsRequired = required;
            Order = order;

            if ( key.IsNotNullOrWhiteSpace() )
            {
                Key = key;
            }

            IncludeInactive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CampusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultCampusGuids">The default campus guids.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public CampusesFieldAttribute( string name = "Campuses", string description = "", bool required = true, string defaultCampusGuids = "", bool includeInactive = false, string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.CAMPUSES.AsGuid(), name )
        {
            Category = category;
            DefaultValue = defaultCampusGuids;
            Description = description;
            IsRequired = required;
            Order = order;

            if ( key.IsNotNullOrWhiteSpace() )
            {
                Key = key;
            }

            IncludeInactive = includeInactive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CampusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CampusesFieldAttribute( string name )
            : base( SystemGuid.FieldType.CAMPUSES.AsGuid(), name )
        {
            IncludeInactive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CampusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CampusesFieldAttribute( string name )
            : base( name, fieldTypeClass: typeof( Rock.Field.Types.CampusesFieldType ).FullName )
        {
            IncludeInactive = false;
        }

        /// <summary>
        /// Gets or sets the campus types filter.
        /// </summary>
        /// <value>
        /// The campus types filter.
        /// </value>
        public string CampusTypesFilter
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( FILTER_CAMPUS_TYPES_KEY ) ?? string.Empty;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( FILTER_CAMPUS_TYPES_KEY, new Field.ConfigurationValue( value ) );
            }
        }

        /// <summary>
        /// Gets or sets the campus status filter.
        /// </summary>
        /// <value>
        /// The campus status filter.
        /// </value>
        public string CampusStatusFilter
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( FILTER_CAMPUS_STATUS_KEY ) ?? string.Empty;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( FILTER_CAMPUS_STATUS_KEY, new Field.ConfigurationValue( value ) );
            }
        }

        /// <summary>
        /// Determines if inactive campuses are included.
        /// </summary>
        public bool IncludeInactive
        {
            get => FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY )?.AsBoolean() ?? false;
            set => FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( value.ToString() ) );
        }
    }
}
