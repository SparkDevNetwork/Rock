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
using System.Configuration;

namespace Rock.Configuration
{
    /// <summary>
    /// Read RockConfig section from Web.Config
    /// </summary>
    public class RockConfig : ConfigurationSection
    {
        private static RockConfig config
        = ConfigurationManager.GetSection( "rockConfig" ) as RockConfig;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static RockConfig Config
        {
            get
            {
                return config;
            }
        }

        /// <summary>
        /// Gets the attribute Value Collection.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [System.Configuration.ConfigurationProperty( "attributeValues" )]
        [ConfigurationCollection( typeof( AttributeValuesConfig ), AddItemName = "attributeValue" )]
        public AttributeValuesConfig AttributeValues
        {
            get
            {
                object o = this["attributeValues"];
                return o as AttributeValuesConfig;
            }
        }
    }
}