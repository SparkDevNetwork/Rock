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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Account Field Attribute.  Stored as FinancialAccount's Guid
    /// </summary>
    public class AccountFieldAttribute : FieldAttribute
    {
        private const string DISPLAY_PUBLIC_NAME = "displaypublicname";
        private const string DISPLAY_CHILD_ITEM_COUNTS = "displaychilditemcounts";
        private const string DISPLAY_ACTIVE_ONLY = "displayactiveitemsonly";
        private const string ENHANCED_FOR_LONG_LISTS = "enhancedforlonglists";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AccountFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null ) : 
            base( name, description, required, defaultValue, category, order, key, typeof( AccountFieldType ).FullName )
        {
            DisplayActiveItemsOnly = true;
            DisplayChildItemCounts = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enhanced for long lists].
        /// </summary>
        /// <value><c>true</c> if [enhanced for long lists]; otherwise, <c>false</c>.</value>
        public virtual bool EnhancedForLongLists
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENHANCED_FOR_LONG_LISTS ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENHANCED_FOR_LONG_LISTS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display public name].
        /// </summary>
        /// <value><c>true</c> if [display public name]; otherwise, <c>false</c>.</value>
        public virtual bool DisplayPublicName
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( DISPLAY_PUBLIC_NAME ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_PUBLIC_NAME, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display child item counts].
        /// </summary>
        /// <value><c>true</c> if [display child item counts]; otherwise, <c>false</c>.</value>
        public virtual bool DisplayChildItemCounts
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( DISPLAY_CHILD_ITEM_COUNTS ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_CHILD_ITEM_COUNTS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display active items only].
        /// </summary>
        /// <value><c>true</c> if [display active items only]; otherwise, <c>false</c>.</value>
        public virtual bool DisplayActiveItemsOnly
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( DISPLAY_ACTIVE_ONLY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_ACTIVE_ONLY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
