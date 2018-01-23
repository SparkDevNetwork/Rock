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
    /// Field used to save and display a social network icons
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class SocialMediaAccountFieldAttribute : FieldAttribute
    {
        private const string NAME_KEY = "name";
        private const string ICONCSSCLASS_KEY = "iconcssclass";
        private const string COLOR_KEY = "color";
        private const string TEXT_TEMPLATE = "texttemplate";
        private const string BASEURL = "baseurl";

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaAccountFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="socialNetworkName">The name of the socal media network.</param>
        /// <param name="iconCssClass">The icon that represents the social media network.</param>
        /// <param name="color">The color to use for making buttons for the social media network.</param>
        /// <param name="textTemplate">The text template.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        internal SocialMediaAccountFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "", 
            string socialNetworkName = "", string iconCssClass = "", string color = "", string textTemplate = "", string baseUrl = "",
            string category = "", int order = 0, string key = null, string fieldTypeClass = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SocialMediaAccountFieldType ).FullName )
        {

            if ( !string.IsNullOrWhiteSpace( socialNetworkName ) )
            {
                var configValue = new Field.ConfigurationValue( socialNetworkName );
                FieldConfigurationValues.Add( NAME_KEY, configValue );
            }

            if ( !string.IsNullOrWhiteSpace( iconCssClass ) )
            {
                var configValue = new Field.ConfigurationValue( iconCssClass );
                FieldConfigurationValues.Add( ICONCSSCLASS_KEY, configValue );
            }

            if ( !string.IsNullOrWhiteSpace( color ) )
            {
                var configValue = new Field.ConfigurationValue( color );
                FieldConfigurationValues.Add( COLOR_KEY, configValue );
            }

            if ( !string.IsNullOrWhiteSpace( textTemplate ) )
            {
                var configValue = new Field.ConfigurationValue( textTemplate );
                FieldConfigurationValues.Add( TEXT_TEMPLATE, configValue );
            }

            if ( !string.IsNullOrWhiteSpace( baseUrl ) )
            {
                var configValue = new Field.ConfigurationValue( baseUrl );
                FieldConfigurationValues.Add( BASEURL, configValue );
            }
        }

    }
}
