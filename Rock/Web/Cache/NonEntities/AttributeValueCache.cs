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

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModel;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    [LavaType( "AttributeId", "EntityId", "Value", "ValueFormatted", "AttributeName", "AttributeAbbreviatedName", "AttributeKey", "AttributeIsGridColumn", "AttributeCategoryIds" )]
    [DotLiquid.LiquidType( "AttributeId", "EntityId", "Value", "ValueFormatted", "AttributeName", "AttributeAbbreviatedName", "AttributeKey", "AttributeIsGridColumn", "AttributeCategoryIds" )]
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
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        public AttributeValueCache( int attributeId, int? entityId, string value )
        {
            AttributeId = attributeId;
            EntityId = entityId;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public AttributeValueCache( AttributeValue model )
            : this( model.AttributeId, model.EntityId, model.Value )
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

        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public AttributeValueViewModel ToViewModel( Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new ViewModelHelper<AttributeValueCache, AttributeValueViewModel>();
            var viewModel = helper.CreateViewModel( this, currentPerson, loadAttributes );
            return viewModel;
        }

        #endregion Methods
    }

    /// <summary>
    /// AttributeValueCache View Model Helper
    /// </summary>
    public partial class AttributeValueViewModelHelper : ViewModelHelper<AttributeValueCache, AttributeValueViewModel>
    {
        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override AttributeValueViewModel CreateViewModel( AttributeValueCache model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new AttributeValueViewModel
            {
                AttributeId = model.AttributeId,
                EntityId = model.EntityId,
                Value = model.Value
            };

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );
            return viewModel;
        }

        /// <summary>
        /// Applies the additional properties and security to view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public override void ApplyAdditionalPropertiesAndSecurityToViewModel( AttributeValueCache model, AttributeValueViewModel viewModel, Person currentPerson = null, bool loadAttributes = true )
        {
            viewModel.Attribute = AttributeCache.Get( model.AttributeId ).ToViewModel();
        }
    }
}