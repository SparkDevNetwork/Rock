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
    /// Field Attribute to select a connection opportunity.
    /// Stored as ConnectionOpportunity.Guid
    /// </summary>
    public class ConnectionOpportunityFieldAttribute: FieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityFieldAttribute"/> class.
        /// If using Object Initializer syntax include all properties in the obj init syntax.
        /// </summary>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public ConnectionOpportunityFieldAttribute()
            : base( SystemGuid.FieldType.CONNECTION_OPPORTUNITY.AsGuid(), string.Empty, string.Empty, true, string.Empty, string.Empty, 0, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public ConnectionOpportunityFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "", bool includeInactive = false, string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.CONNECTION_OPPORTUNITY.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            IncludeInactive = includeInactive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ConnectionOpportunityFieldAttribute( string name )
            : base( SystemGuid.FieldType.CONNECTION_OPPORTUNITY.AsGuid(), name )
        {
            IncludeInactive = false;
        }

        /// <summary>
        /// Determines if inactive connection opportunities are included.
        /// </summary>
        public bool IncludeInactive
        {
            get => FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBoolean();
            set => FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( value.ToString() ) );
        }
    }
}
