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
        internal SocialMediaAccountFieldAttribute( string name )
            : base( SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT.AsGuid(), name )
        {
            SocialNetworkName = string.Empty;
            IconCssClass = string.Empty;
            Color = string.Empty;
            TextTemplate = string.Empty;
            BaseUrl = string.Empty;
        }

        /// <summary>
        /// The name of the social media network.
        /// </summary>
        public string SocialNetworkName
        {
            get => FieldConfigurationValues.GetValueOrNull( NAME_KEY );
            set => FieldConfigurationValues.AddOrReplace( NAME_KEY, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// The icon that represents the social media network.
        /// </summary>
        public string IconCssClass
        {
            get => FieldConfigurationValues.GetValueOrNull( ICONCSSCLASS_KEY );
            set => FieldConfigurationValues.AddOrReplace( ICONCSSCLASS_KEY, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// The color to use for making buttons for the social media network.
        /// </summary>
        public string Color
        {
            get => FieldConfigurationValues.GetValueOrNull( COLOR_KEY );
            set => FieldConfigurationValues.AddOrReplace( COLOR_KEY, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// The text template.
        /// </summary>
        public string TextTemplate
        {
            get => FieldConfigurationValues.GetValueOrNull( TEXT_TEMPLATE );
            set => FieldConfigurationValues.AddOrReplace( TEXT_TEMPLATE, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// The base URL.
        /// </summary>
        public string BaseUrl
        {
            get => FieldConfigurationValues.GetValueOrNull( BASEURL );
            set => FieldConfigurationValues.AddOrReplace( BASEURL, new Field.ConfigurationValue( value ) );
        }
    }
}
