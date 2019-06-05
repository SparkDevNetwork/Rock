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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    [DotLiquid.LiquidType( "AttributeId", "EntityId", "Value", "ValueFormatted", "AttributeName", "AttributeAbbreviatedName", "AttributeKey", "AttributeIsGridColumn" )]
    public class AttributeValueCache
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        public AttributeValueCache()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public AttributeValueCache( AttributeValue model )
        {
            AttributeId = model.AttributeId;
            Value = model.Value;
            EntityId = model.EntityId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets the value using the most appropriate datatype
        /// </summary>
        /// <value>
        /// The field type value.
        /// </value>
        public object ValueAsType
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.FieldType.Field.ValueAsFieldType( null, Value, attribute.QualifierValues ) : Value;
            }
        }

        /// <summary>
        /// Get the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <value>
        /// The field type value.
        /// </value>
        public object SortValue
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.FieldType.Field.SortValue( null, Value, attribute.QualifierValues ) : Value;
            }
        }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [LavaInclude]
        public virtual string ValueFormatted
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.FieldType.Field.FormatValue( null, attribute.EntityTypeId, EntityId, Value, attribute.QualifierValues, false ) : Value;
            }
        }

        /// <summary>
        /// Gets the name of the attribute 
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The name of the attribute.
        /// </value>
        [LavaInclude]
        public virtual string AttributeName
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.Name : string.Empty;
            }
        }

        /// <summary>
        /// Gets the name of the attribute abbreviated.
        /// </summary>
        /// <value>
        /// The name of the attribute abbreviated.
        /// </value>
        [LavaInclude]
        public virtual string AttributeAbbreviatedName
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                if ( attribute == null )
                {
                    return string.Empty;
                }

                return attribute.AbbreviatedName.IsNotNullOrWhiteSpace() ? attribute.AbbreviatedName : attribute.Name;
            }
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The attribute key.
        /// </value>
        [LavaInclude]
        public virtual string AttributeKey
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.Key : string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether attribute is grid column.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// <c>true</c> if [attribute is grid column]; otherwise, <c>false</c>.
        /// </value>
        [LavaInclude]
        public virtual bool AttributeIsGridColumn
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null && attribute.IsGridColumn;
            }
        }
        /// <summary>
        /// Returns the Formatted Value of this Attribute Value
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ValueFormatted;
        }

        #endregion

    }
}