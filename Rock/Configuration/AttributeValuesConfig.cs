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
    /// Attribute Value Collection
    /// </summary>
    public class AttributeValuesConfig : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets the <see cref="AttributeValueConfig"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="AttributeValueConfig"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public AttributeValueConfig this[int index]
        {
            get
            {
                return base.BaseGet( index ) as AttributeValueConfig;
            }
            set
            {
                if ( base.BaseGet( index ) != null )
                {
                    base.BaseRemoveAt( index );
                }
                this.BaseAdd( index, value );
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AttributeValueConfig"/> with the specified response string.
        /// </summary>
        /// <value>
        /// The <see cref="AttributeValueConfig"/>.
        /// </value>
        /// <param name="responseString">The response string.</param>
        /// <returns></returns>
        public new AttributeValueConfig this[string responseString]
        {
            get { return ( AttributeValueConfig ) BaseGet( responseString ); }
            set
            {
                if ( BaseGet( responseString ) != null )
                {
                    BaseRemoveAt( BaseIndexOf( BaseGet( responseString ) ) );
                }
                BaseAdd( value );
            }
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </summary>
        /// <returns>
        /// A newly created <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new AttributeValueConfig();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override object GetElementKey( System.Configuration.ConfigurationElement element )
        {
            return string.Format("{0}-{1}", ( ( AttributeValueConfig ) element ).AttributeKey, ( ( AttributeValueConfig ) element ).EntityId);
        }
    }
}