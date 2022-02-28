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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class DefinedTypeCache : ModelCache<DefinedTypeCache, DefinedType>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        [DataMember]
        public int? FieldTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Defined Type is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Defined Values associated with this Defined Type can be grouped into categories.
        /// </summary>
        /// <value>
        /// <c>true</c> if this DefinedType supports values that can be grouped into categories; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? CategorizedValuesEnabled { get; set; }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public CategoryCache Category
        {
            get
            {
                if ( CategoryId.HasValue )
                {
                    return CategoryCache.Get( CategoryId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldTypeCache FieldType
        {
            get
            {
                if ( FieldTypeId.HasValue )
                {
                    return FieldTypeCache.Get( FieldTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the first defined value that has the specified Value (case-insensitive)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public DefinedValueCache GetDefinedValueFromValue( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return null;
            }

            if ( _definedValueIdsByValue == null )
            {
                // create case-insensitive dictionary 
                _definedValueIdsByValue = DefinedValues.Where( a => !string.IsNullOrEmpty( a.Value ) ).GroupBy( a => a.Value.ToLower() ).ToDictionary( k => k.Key, v => v.FirstOrDefault().Id, StringComparer.OrdinalIgnoreCase );
            }

            int? definedValueId = _definedValueIdsByValue.GetValueOrNull( value );

            return definedValueId.HasValue ? DefinedValueCache.Get( definedValueId.Value ) : null;
        }

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<DefinedValueCache> DefinedValues
        {
            get
            {
                var definedValues = new List<DefinedValueCache>();

                if ( _definedValueIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _definedValueIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _definedValueIds = new DefinedValueService( rockContext )
                                    .GetByDefinedTypeId( Id )
                                    .Select( v => v.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _definedValueIds )
                {
                    var definedValue = DefinedValueCache.Get( id );
                    if ( definedValue != null )
                    {
                        definedValues.Add( definedValue );
                    }
                }

                return definedValues;
            }
        }
        private List<int> _definedValueIds = null;
        private Dictionary<string, int> _definedValueIdsByValue = null;

        /// <summary>
        /// Reloads the defined values.
        /// </summary>
        [Obsolete("This will not work with a distributed cache system such as Redis")]
        [RockObsolete("1.10")]
        public void ReloadDefinedValues()
        {
            // set definedValueIds to null so it load them all at once on demand
            _definedValueIds = null;
            _definedValueIdsByValue = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var definedType = entity as DefinedType;
            if ( definedType == null )
                return;

            IsSystem = definedType.IsSystem;
            FieldTypeId = definedType.FieldTypeId;
            Order = definedType.Order;
            CategoryId = definedType.CategoryId;
            Name = definedType.Name;
            Description = definedType.Description;
            IsActive = definedType.IsActive;
            CategorizedValuesEnabled = definedType.CategorizedValuesEnabled;

            // set definedValueIds to null so it load them all at once on demand
            _definedValueIds = null;
            _definedValueIdsByValue = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}