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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Obsidian.ViewModel
{
    /// <summary>
    /// Attribute Value Args
    /// </summary>
    public class AttributeValueArgs : ViewArgs<AttributeValue>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }

    /// <summary>
    /// Attribute Value View Model
    /// </summary>
    [ViewModelOf( typeof( AttributeValue ) )]
    public class AttributeValueViewModel : AttributeValueArgs, IViewModel
    {
        /// <summary>
        /// Gets or sets the attribute key.
        /// </summary>
        /// <value>
        /// The attribute key.
        /// </value>
        public string AttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute abbreviated.
        /// </summary>
        /// <value>
        /// The name of the attribute abbreviated.
        /// </value>
        public string AttributeAbbreviatedName { get; set; }

        /// <summary>
        /// Gets or sets the attribute field type unique identifier.
        /// </summary>
        /// <value>
        /// The attribute field type unique identifier.
        /// </value>
        public Guid AttributeFieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the attribute description.
        /// </summary>
        /// <value>
        /// The attribute description.
        /// </value>
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Sets the properties from entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void SetPropertiesFromEntity( IEntity entity )
        {
            entity.CopyPropertiesTo( this );

            var attributeId = ( entity as AttributeValue )?.AttributeId ?? 0;
            var attribute = AttributeCache.Get( attributeId );
            SetPropertiesFromEntity( attribute );
        }

        /// <summary>
        /// Sets the properties from entity.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public void SetPropertiesFromEntity( AttributeCache attribute )
        {
            if ( attribute != null )
            {
                AttributeName = attribute.Name;
                AttributeAbbreviatedName = attribute.AbbreviatedName;
                AttributeFieldTypeGuid = attribute.FieldType.Guid;
                AttributeKey = attribute.Key;
                AttributeDescription = attribute.Description;
            }
        }
    }
}
