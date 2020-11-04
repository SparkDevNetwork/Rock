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

using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Class that represents the Location List field attribute.
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    public class LocationListFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public LocationListFieldAttribute(
            string name,
            string description = "",
            bool required = true,
            string defaultValue = "",
            string category = "",
            int order = 0,
            string key = null ) :
            base( name, description, required, defaultValue, category, order, key, typeof( LocationListFieldType ).FullName )
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [address required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [address required]; otherwise, <c>false</c>.
        /// </value>
        public bool AddressRequired
        {
            get => FieldConfigurationValues.GetConfigurationValueAsString( LocationListFieldType.ConfigurationKey.AddressRequired ).AsBoolean();
            set => FieldConfigurationValues[LocationListFieldType.ConfigurationKey.AddressRequired] = new Field.ConfigurationValue( value.ToString() );
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow adding new locations].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow adding new locations]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAddingNewLocations
        {
            get => FieldConfigurationValues.GetConfigurationValueAsString( LocationListFieldType.ConfigurationKey.AllowAddingNewLocations ).AsBoolean();
            set => FieldConfigurationValues[LocationListFieldType.ConfigurationKey.AllowAddingNewLocations] = new Field.ConfigurationValue( value.ToString() );
        }

        /// <summary>
        /// Gets or sets the type of the location.
        /// </summary>
        /// <value>
        /// The type of the location.
        /// </value>
        public int? LocationType
        {
            get => FieldConfigurationValues.GetConfigurationValueAsString( LocationListFieldType.ConfigurationKey.LocationType ).AsIntegerOrNull();
            set => FieldConfigurationValues[LocationListFieldType.ConfigurationKey.LocationType] = new Field.ConfigurationValue( value.ToStringSafe() );
        }

        /// <summary>
        /// Gets or sets a value indicating whether [parent location].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [parent location]; otherwise, <c>false</c>.
        /// </value>
        public bool ParentLocation
        {
            get => FieldConfigurationValues.GetConfigurationValueAsString( LocationListFieldType.ConfigurationKey.ParentLocation ).AsBoolean();
            set => FieldConfigurationValues[LocationListFieldType.ConfigurationKey.ParentLocation] = new Field.ConfigurationValue( value.ToStringSafe() );
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show city state].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show city state]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCityState
        {
            get => FieldConfigurationValues.GetConfigurationValueAsString( LocationListFieldType.ConfigurationKey.ShowCityState ).AsBoolean();
            set => FieldConfigurationValues[LocationListFieldType.ConfigurationKey.ShowCityState] = new Field.ConfigurationValue( value.ToString() );
        }
    }
}
