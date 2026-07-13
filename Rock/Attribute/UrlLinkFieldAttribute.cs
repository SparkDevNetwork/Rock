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
    /// Field Attribute to set a URL.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class UrlLinkFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlLinkFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public UrlLinkFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.URL_LINK.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlLinkFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public UrlLinkFieldAttribute( string name )
            : base( SystemGuid.FieldType.URL_LINK.AsGuid(), name )
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether a trailing forward slash should be required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a trailing forward slash should be required; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldRequireTrailingForwardSlash
        {
            get
            {
                return FieldConfigurationValues.GetValueOrDefault( "ShouldRequireTrailingForwardSlash", new Field.ConfigurationValue( "false" ) ).Value.AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( "ShouldRequireTrailingForwardSlash", new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value should always show condensed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value should always show condensed; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldAlwaysShowCondensed
        {
            get
            {
                return FieldConfigurationValues.GetValueOrDefault( "ShouldAlwaysShowCondensed", new Field.ConfigurationValue( "false" ) ).Value.AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( "ShouldAlwaysShowCondensed", new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
