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
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.ViewModel
{
    /// <summary>
    /// AttributeViewModel
    /// </summary>
    /// <seealso cref="Rock.ViewModel.IViewModel" />
    public partial class AttributeViewModel
    {
        /// <summary>
        /// Gets or sets the category Guids.
        /// </summary>
        /// <value>
        /// The category Guids.
        /// </value>
        public Guid[] CategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>
        /// The field type unique identifier.
        /// </value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values.
        /// </value>
        public Dictionary<string, ConfigurationValue> QualifierValues { get; set; }

        /// <summary>
        /// Sets the properties from entity.
        /// </summary>
        /// <param name="entity">The entity, cache item, or some object.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public override void SetPropertiesFrom( object entity, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( entity == null )
            {
                return;
            }

            base.SetPropertiesFrom( entity, currentPerson, loadAttributes );
            AttributeCache attributeCache = null;

            if ( entity is IEntity entityWithId )
            {
                attributeCache = AttributeCache.Get( entityWithId.Id );
            }
            else if ( entity is AttributeCache asAttributeCache )
            {
                attributeCache = asAttributeCache;
            }

            if ( attributeCache != null )
            {
                FieldTypeGuid = FieldTypeCache.Get( attributeCache.FieldTypeId ).Guid;
                CategoryGuids = attributeCache.Categories.Select( c => c.Guid ).ToArray();
                QualifierValues = attributeCache.QualifierValues;
            }
        }
    }
}
