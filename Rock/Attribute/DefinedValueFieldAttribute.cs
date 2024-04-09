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

using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more DefinedValues for the given DefinedType Guid.
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple)
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DefinedValueFieldAttribute : FieldAttribute
    {
        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string ALLOW_ADDING_NEW_VALUES_KEY = "AllowAddingNewValues";
        private const string REPEAT_COLUMNS_KEY = "RepeatColumns";
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string DISPLAY_DESCRIPTION = "displaydescription";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public DefinedValueFieldAttribute( string name )
            : this(  "", name )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DefinedValueFieldAttribute( string definedTypeGuid, string name = "", string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
             : this( definedTypeGuid, name, description, required, allowMultiple, false, defaultValue, category, order, key )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute"/> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="enhanced">if set to <c>true</c> [enhanced].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DefinedValueFieldAttribute( string definedTypeGuid, string name, string description, bool required, bool allowMultiple, bool enhanced, string defaultValue, string category, int order, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.DefinedValueFieldType ).FullName )
        {
            this.DefinedTypeGuid = definedTypeGuid;
            this.AllowMultiple = allowMultiple;
            this.Enhanced = enhanced;
            if ( string.IsNullOrWhiteSpace( Name ) && RockInstanceConfig.DatabaseIsAvailable )
            {
                this.Name = DefinedValueCache.Get( definedTypeGuid.AsGuid() )?.Value;
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name?.Replace( " ", string.Empty );
            }

            this.AllowAddingNewValues = false;
            this.RepeatColumns = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute"/> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="enhanced">if set to <c>true</c> [enhanced].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="displayDescription">if set to <c>true</c> [display description].</param>
        public DefinedValueFieldAttribute( string definedTypeGuid, string name, string description, bool required, bool allowMultiple, bool enhanced, string defaultValue, string category, int order, string key = null, bool includeInactive = false, bool displayDescription = true )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.DefinedValueFieldType ).FullName )
        {
            this.DefinedTypeGuid = definedTypeGuid;
            this.AllowMultiple = allowMultiple;
            this.Enhanced = enhanced;
            this.IncludeInactive = includeInactive;
            this.DisplayDescription = displayDescription;

            if ( string.IsNullOrWhiteSpace( Name ) && RockInstanceConfig.DatabaseIsAvailable )
            {
                this.Name = DefinedValueCache.Get( definedTypeGuid.AsGuid() )?.Value;
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name?.Replace( " ", string.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the defined type unique identifier.
        /// </summary>
        /// <value>
        /// The defined type unique identifier.
        /// </value>
        public string DefinedTypeGuid
        {
            get
            {
                int? definedTypeId = FieldConfigurationValues.GetValueOrNull( DEFINED_TYPE_KEY ).AsIntegerOrNull();
                if ( definedTypeId.HasValue )
                {
                    return DefinedTypeCache.Get( definedTypeId.Value )?.Guid.ToString();
                }
                else
                {
                    return null;
                }
            }

            set
            {
                var definedTypeGuid = value.AsGuidOrNull();
                int? definedTypeId = null;
                if ( definedTypeGuid.HasValue && RockInstanceConfig.DatabaseIsAvailable )
                {
                    definedTypeId = DefinedTypeCache.GetId( definedTypeGuid.Value );
                }

                FieldConfigurationValues.AddOrReplace( DEFINED_TYPE_KEY, new Field.ConfigurationValue( definedTypeId?.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DefinedValueFieldAttribute"/> is will use the enhanced defined value picker.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enhanced; otherwise, <c>false</c>.
        /// </value>
        public bool Enhanced
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENHANCED_SELECTION_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENHANCED_SELECTION_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow adding new values].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow adding new values]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAddingNewValues
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_ADDING_NEW_VALUES_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_ADDING_NEW_VALUES_KEY, new Field.ConfigurationValue( value.ToTrueFalse() ) );
            }
        }

        /// <summary>
        /// Gets or sets the repeat columns, null values will be saved as a 4.
        /// </summary>
        /// <value>
        /// The repeat columns.
        /// </value>
        public int? RepeatColumns
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( REPEAT_COLUMNS_KEY ).AsIntegerOrNull();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( REPEAT_COLUMNS_KEY, new Field.ConfigurationValue( value?.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display description].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display description]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDescription
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_DESCRIPTION, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

    }
}