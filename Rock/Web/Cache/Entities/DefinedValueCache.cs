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
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Defined Value Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class DefinedValueCache : ModelCache<DefinedValueCache, DefinedValue>
    {

        #region Properties

        /// <inheritdoc cref="Rock.Model.DefinedValue.IsSystem" />
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.DefinedTypeId" />
        [DataMember]
        public int DefinedTypeId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.Value" />
        [DataMember]
        public string Value { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.Description" />
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }
        
        /// <inheritdoc cref="Rock.Model.DefinedValue.CategoryId" />
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DefinedValue.DefinedType" />
        public DefinedTypeCache DefinedType => DefinedTypeCache.Get( DefinedTypeId );

        /// <inheritdoc cref="Rock.Model.DefinedValue.ParentAuthority" />
        public override ISecured ParentAuthority => DefinedType;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var definedValue = entity as DefinedValue;
            if ( definedValue == null ) return;

            IsSystem = definedValue.IsSystem;
            DefinedTypeId = definedValue.DefinedTypeId;
            Order = definedValue.Order;
            Value = definedValue.Value;
            Description = definedValue.Description;
            IsActive = definedValue.IsActive;
            CategoryId = definedValue.CategoryId;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the Value of the DefinedValue
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetValue( int? id )
        {
            if ( !id.HasValue )
                return null;

            var definedValue = Get( id.Value );
            return definedValue?.Value;
        }

        /// <summary>
        /// Gets the Value of the DefinedValue
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int? id )
        {
            return GetValue( id );
        }

        #endregion
    }
}