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
    /// Read Attribute Value node in rockConffig
    /// </summary>
    public class AttributeValueConfig : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueConfig"/> class.
        /// </summary>
        public AttributeValueConfig()
        {

        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <value>
        /// The attribute key.
        /// </value>
        [ConfigurationProperty( "attributeKey", IsRequired = true )]
        public string AttributeKey
        {
            get
            {
                return this["attributeKey"] as string;
            }
        }

        /// <summary>
        /// Gets the entitytype Id.
        /// </summary>
        /// <value>
        /// The entitytype Id.
        /// </value>
        [ConfigurationProperty( "entityTypeId", IsRequired = true )]
        public string EntityTypeId
        {
            get
            {
                return this["entityTypeId"] as string;
            }
        }

        /// <summary>
        /// Gets the entitytype qualifier column.
        /// </summary>
        /// <value>
        /// The entitytype qualifier column.
        /// </value>
        [ConfigurationProperty( "entityTypeQualifierColumm" )]
        public string EntityTypeQualifierColumm
        {
            get
            {
                return this["entityTypeQualifierColumm"] as string;
            }
        }

        /// <summary>
        /// Gets the entitytype qualifier value.
        /// </summary>
        /// <value>
        /// The entitytype qualifier value.
        /// </value>
        [ConfigurationProperty( "entityTypeQualifierValue" )]
        public string EntityTypeQualifierValue
        {
            get
            {
                return this["entityTypeQualifierValue"] as string;
            }
        }

        /// <summary>
        /// Gets the entity Id.
        /// </summary>
        /// <value>
        /// The entity Id.
        /// </value>
        [ConfigurationProperty( "entityId", IsRequired = true )]
        public string EntityId
        {
            get
            {
                return this["entityId"] as string;
            }
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <value>
        /// The attribute value.
        /// </value>
        [ConfigurationProperty( "value", IsRequired = true )]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
        }

    }
}