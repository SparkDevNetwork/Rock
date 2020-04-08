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
namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a site.
    /// Stored as Site.Id
    /// </summary>
    public class SiteFieldAttribute : FieldAttribute
    {
        private const string SHORTENING_SITES_ONLY = "shorteningSitesOnly";

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultSiteId">The default site identifier.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="shorteningSitesOnly">if set to <c>true</c> [shortening sites only].</param>
        public SiteFieldAttribute( string name = "Site", string description = "", bool required = true, string defaultSiteId = "", string category = "", int order = 0, string key = null, bool shorteningSitesOnly = false )
            : base( name, description, required, defaultSiteId, category, order, key, typeof( Rock.Field.Types.SiteFieldType ).FullName )
        {
            var htmlConfig = new Field.ConfigurationValue( shorteningSitesOnly.ToString() );
            FieldConfigurationValues.Add( SHORTENING_SITES_ONLY, htmlConfig );
        }
    }
}