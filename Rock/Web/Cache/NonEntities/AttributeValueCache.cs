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
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    [LavaType( "AttributeId", "EntityId", "Value", "ValueFormatted", "PersistedTextValue", "PersistedFormattedValue", "PersistedCondensedTextValue", "PersistedCondensedFormattedValue", "IsPersistedValueDirty", "AttributeName", "AttributeAbbreviatedName", "AttributeKey", "AttributeIsGridColumn", "AttributeCategoryIds" )]
    [DotLiquid.LiquidType( "AttributeId", "EntityId", "Value", "ValueFormatted", "PersistedTextValue", "PersistedFormattedValue", "PersistedCondensedTextValue", "PersistedCondensedFormattedValue", "IsPersistedValueDirty", "AttributeName", "AttributeAbbreviatedName", "AttributeKey", "AttributeIsGridColumn", "AttributeCategoryIds" )]
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
        /// Initializes a new instance of the <see cref="AttributeValueCache" /> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="persistedTextValue">The persisted text value.</param>
        /// <param name="persistedHtmlValue">The persisted HTML value.</param>
        /// <param name="persistedCondensedTextValue">The persisted condensed text value.</param>
        /// <param name="persistedCondensedHtmlValue">The persisted condensed HTML value.</param>
        /// <param name="isPersistedValueDirty">if set to <c>true</c> the persisted values are considered dirty.</param>
        public AttributeValueCache( int attributeId, int? entityId, string value, string persistedTextValue, string persistedHtmlValue, string persistedCondensedTextValue, string persistedCondensedHtmlValue, bool isPersistedValueDirty )
            : this( attributeId, entityId, value )
        {
            PersistedTextValue = persistedTextValue;
            PersistedHtmlValue = persistedHtmlValue;
            PersistedCondensedTextValue = persistedCondensedTextValue;
            PersistedCondensedHtmlValue = persistedCondensedHtmlValue;
            IsPersistedValueDirty = isPersistedValueDirty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        public AttributeValueCache( int attributeId, int? entityId, string value )
        {
            AttributeId = attributeId;
            EntityId = entityId;
            Value = value;
            IsPersistedValueDirty = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public AttributeValueCache( AttributeValue model )
            : this( model.AttributeId, model.EntityId, model.Value, model.PersistedTextValue, model.PersistedHtmlValue, model.PersistedCondensedTextValue, model.PersistedCondensedHtmlValue, model.IsPersistedValueDirty )
        {
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
        [LavaVisible]
        [DataMember]
        public virtual string ValueFormatted
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.FieldType.Field.FormatValue( null, attribute.EntityTypeId, EntityId, Value, attribute.QualifierValues, false ) : Value;
            }
        }

        /// <summary>
        /// Gets the persisted text value.
        /// </summary>
        /// <value>The persisted text value.</value>
        [DataMember]
        public string PersistedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted HTML value.
        /// </summary>
        /// <value>The persisted HTML value.</value>
        [DataMember]
        public string PersistedHtmlValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed text value.
        /// </summary>
        /// <value>The persisted condensed text value.</value>
        [DataMember]
        public string PersistedCondensedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed HTML value.
        /// </summary>
        /// <value>The persisted condensed HTML value.</value>
        [DataMember]
        public string PersistedCondensedHtmlValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the persisted values are
        /// considered dirty. If the values are dirty then it should be assumed
        /// that they are not in sync with the <see cref="Value"/> property.
        /// </summary>
        /// <value><c>true</c> if the persisted values are considered dirty; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsPersistedValueDirty { get; private set; }

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
        [LavaVisible]
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
        [LavaVisible]
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
        [LavaVisible]
        public virtual string AttributeKey
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.Key : string.Empty;
            }
        }

        /// <summary>
        /// Gets the attribute category ids.
        /// </summary>
        /// <value>
        /// The attribute category ids.
        /// </value>
        [LavaVisible]
        public virtual List<int> AttributeCategoryIds
        {
            get
            {
                var attribute = AttributeCache.Get( AttributeId );
                return attribute != null ? attribute.CategoryIds : null;
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
        [LavaVisible]
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

        #region Methods

        #endregion Methods
    }
}