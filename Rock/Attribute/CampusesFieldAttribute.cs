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
        public CampusesFieldAttribute( string name = "Campuses", string description = "", bool required = true, string defaultCampusGuids = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultCampusGuids, category, order, key, typeof( Rock.Field.Types.CampusesFieldType ).FullName )
        {
            FieldConfigurationValues.Add( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( "False" ) );
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
        public CampusesFieldAttribute( string name = "Campuses", string description = "", bool required = true, string defaultCampusGuids = "", bool includeInactive = false, string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultCampusGuids, category, order, key, typeof( Rock.Field.Types.CampusesFieldType ).FullName )
        {
            FieldConfigurationValues.Add( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( includeInactive.ToString() ) );
        }
    }
}