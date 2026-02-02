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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

using Rock.Security;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Result model for a person's profile.
    /// </summary>
    internal class EntityResultBase
    {
        #region Fields

        private static readonly ConcurrentDictionary<Type, (List<PropertyInfo> IndividualProperties, List<PropertyInfo> CollectionProperties)> _nestedPropertiesCache
            = new ConcurrentDictionary<Type, (List<PropertyInfo>, List<PropertyInfo>)>();

        #endregion

        #region Properties

        /// <summary>
        /// The entity id. This will not be show in the JSON output.
        /// </summary>
        [JsonIgnore]
        internal int Id { get; set; }

        /// <summary>
        /// Internal identifier of the phone number.
        /// </summary>
        public string IdKey => Id.AsIdKey();

        /// <summary>
        /// Gets or sets the date and time that the entity was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the entity was last modified.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person who created the entity.
        /// </summary>
        public PersonResult CreatedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the person who last modified the entity.
        /// </summary>
        public PersonResult ModifiedByPerson { get; set; }

        /// <summary>
        /// Attribute values of the entity.
        /// </summary>
        public List<AttributeValueResult> AttributeValues { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sanitizes the entity for security related to the request context.
        /// </summary>
        /// <param name="agentRequestContext">The context that describes the current request.</param>
        /// <returns><c>false</c> if the entire result should be excluded. This is used when nested properties to fully remove them.</returns>
        public virtual bool Sanitize( AgentRequestContext agentRequestContext )
        {
            SanitizeNestedProperties( agentRequestContext );
            SanitizeAttributeSecurity( agentRequestContext );

            return SanitizeResult( agentRequestContext );
        }

        /// <summary>
        /// Sanitizes this result for security related to the request context.
        /// This is the method you will want to override most of the time.
        /// </summary>
        /// <param name="agentRequestContext">The context that describes the current request.</param>
        /// <returns><c>false</c> if the entire result should be excluded. This is used when nested properties to fully remove them.</returns>
        protected virtual bool SanitizeResult( AgentRequestContext agentRequestContext )
        {
            return true;
        }

        /// <summary>
        /// Sanitizes any nested properties for security related to the request context.
        /// </summary>
        /// <param name="agentRequestContext">The context that describes the current request.</param>
        protected void SanitizeNestedProperties( AgentRequestContext agentRequestContext )
        {
            var cache = _nestedPropertiesCache.GetOrAdd( GetType(), rt =>
            {
                var entityResultBaseType = typeof( EntityResultBase );

                var individualProperties = rt.GetProperties()
                    .Where( pi => entityResultBaseType.IsAssignableFrom( pi.PropertyType ) )
                    .ToList();

                var collectionProperties = rt.GetProperties()
                    .Where( pi => pi.PropertyType.IsGenericType
                        && pi.PropertyType.GetGenericTypeDefinition() == typeof( ICollection<> )
                        && entityResultBaseType.IsAssignableFrom( pi.PropertyType.GetGenericArguments()[0] ) )
                    .ToList();

                return (individualProperties, collectionProperties);
            } );

            foreach ( var property in cache.IndividualProperties )
            {
                if ( property.GetValue( this ) is EntityResultBase nestedResult )
                {
                    var safe = nestedResult.Sanitize( agentRequestContext );

                    if ( !safe )
                    {
                        property.SetValue( this, null );
                    }
                }
            }

            foreach ( var property in cache.CollectionProperties )
            {
                if ( !( property.GetValue( this ) is IEnumerable nestedResults ) )
                {
                    continue;
                }

                foreach ( var nestedResultObj in nestedResults )
                {
                    if ( !( nestedResultObj is EntityResultBase nestedResult ) )
                    {
                        continue;
                    }

                    var safe = nestedResult.Sanitize( agentRequestContext );

                    if ( !safe && nestedResults is IList listResults )
                    {
                        listResults.Remove( nestedResultObj );
                    }
                }
            }
        }

        /// <summary>
        /// Removes any attributes that the current person does not have view access to.
        /// </summary>
        /// <param name="agentRequestContext">The context that describes the current request.</param>
        protected void SanitizeAttributeSecurity( AgentRequestContext agentRequestContext )
        {
            if ( AttributeValues == null )
            {
                return;
            }

            var currentPerson = agentRequestContext.RockRequestContext.CurrentPerson;

            for ( int i = AttributeValues.Count - 1; i >= 0; i-- )
            {
                var isAllowedViewAccess = AttributeCache.Get( AttributeValues[i].AttributeId )?.IsAuthorized( Authorization.VIEW, currentPerson ) ?? false;

                if ( !isAllowedViewAccess )
                {
                    AttributeValues.RemoveAt( i );
                }
            }
        }

        #endregion
    }
}
