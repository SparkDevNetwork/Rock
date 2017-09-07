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

namespace RockWeb
{
    /// <summary>
    /// Attribute Value Collection
    /// </summary>
    public class AttributeValuesConfig : ConfigurationElementCollection
    {
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

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new AttributeValueConfig();
        }

        protected override object GetElementKey( System.Configuration.ConfigurationElement element )
        {
            return string.Format("{0}-{1}", ( ( AttributeValueConfig ) element ).AttributeKey, ( ( AttributeValueConfig ) element ).EntityId);
        }
    }
}