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
using System.Data.Entity;

using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class EntityType
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this entity supports indexing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this entity supports indexing <c>false</c>.
        /// </value>
        public bool IsIndexingSupported
        {
            get
            {
                Type type = null;
                if ( !string.IsNullOrWhiteSpace( this.AssemblyName ) )
                {
                    try
                    {
                        type = Type.GetType( this.AssemblyName, false );
                    }
                    catch
                    {
                        /* 2020-05-22 MDP
                         * GetType (string typeName, bool throwOnError) can throw exceptions even if throwOnError is false!
                         * see https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype?view=netframework-4.5.2#System_Type_GetType_System_String_System_Boolean_

                          so, if this happens, we'll ignore any error it returns in those cases too
                         */
                    }
                }

                if ( type != null )
                {
                    return typeof( IRockIndexable ).IsAssignableFrom( type );
                }

                return false;
            }
        }
        
        /// <summary>
        /// Gets the name of the get index model.
        /// </summary>
        /// <value>
        /// The name of the get index model.
        /// </value>
        public Type IndexModelType
        {
            get
            {
                Type type = null;
                if ( !string.IsNullOrWhiteSpace( this.AssemblyName ) )
                {
                    type = Type.GetType( this.AssemblyName );
                }

                if ( type != null )
                {
                    if ( typeof( IRockIndexable ).IsAssignableFrom( type ) )
                    {
                        var constructor = type.GetConstructor( Type.EmptyTypes );
                        object instance = constructor.Invoke( new object[] { } );
                        var method = type.GetMethod( "IndexModelType" );

                        if ( method != null )
                        {
                            var indexModelType = ( Type ) method.Invoke( instance, null );
                            return indexModelType;
                        }
                    }
                }

                return null;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether person is authorized for the EntityType
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public virtual bool IsAuthorized( string action, Rock.Model.Person person )
        {
            if ( this.IsSecured )
            {
                object entity = null;
                try
                {
                    var type = EntityTypeCache.Get( this ).GetEntityType();
                    entity = System.Activator.CreateInstance( type );
                }
                catch
                {
                    // unable to create the entity, so return false since we can't do anything with it
                    return false;
                }

                if ( entity is Rock.Security.ISecured )
                {
                    Rock.Security.ISecured iSecured = ( Rock.Security.ISecured ) entity;
                    return iSecured.IsAuthorized( action, person );
                }
            }

            return true;
        }

        #endregion Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return EntityTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            EntityTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}
