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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Rock;
using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CodingHelper
    {
        #region Properties

        /// <summary>
        /// The database context to perform all our operations in.
        /// </summary>
        public RockContext RockContext { get; private set; }

        /// <summary>
        /// Internal list of cached IEntityProcessors that we have created for
        /// this session.
        /// </summary>
        private Dictionary<Type, List<IEntityProcessor>> CachedProcessors { get; set; }

        /// <summary>
        /// Internal list of cached entity types. This is a map of the full class
        /// name to the class Type object.
        /// </summary>
        private Dictionary<string, Type> CachedEntityTypes { get; set; }

        /// <summary>
        /// Internal list of cached PropertyInfo definitions for the given Type.
        /// </summary>
        private Dictionary<Type, List<PropertyInfo>> CachedEntityProperties { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new Helper object for facilitating the export/import of entities.
        /// </summary>
        /// <param name="rockContext">The RockContext to work in when exporting or importing.</param>
        protected CodingHelper( RockContext rockContext )
        {
            CachedEntityTypes = new Dictionary<string, Type>();
            CachedProcessors = new Dictionary<Type, List<IEntityProcessor>>();
            CachedEntityProperties = new Dictionary<Type, List<PropertyInfo>>();
            RockContext = rockContext;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Determines the real Entity type of the given IEntity object. Because
        /// many IEntity objects are dynamic proxies created by Entity Framework
        /// we need to get the actual underlying type.
        /// TODO: This would probably be better served as an extension method or something.
        /// </summary>
        /// <param name="entity">The entity whose type we want to obtain.</param>
        /// <returns>The true IEntity type (such as Rock.Model.Person).</returns>
        static public Type GetEntityType( IEntity entity )
        {
            Type type = entity.GetType();

            return type.IsDynamicProxyType() ? type.BaseType : type;
        }

        /// <summary>
        /// Convert the given object value to the target type. This extends
        /// the IConvertable.ChangeType support since some things don't
        /// implement IConvertable, like Guid and Nullable.
        /// </summary>
        /// <param name="t">The target data type to convert to.</param>
        /// <param name="obj">The object value to be converted.</param>
        /// <returns>The value converted to the target type.</returns>
        static public object ChangeType( Type t, object obj )
        {
            Type u = Nullable.GetUnderlyingType( t );

            if ( u != null )
            {
                return ( obj == null ) ? null : ChangeType( u, obj );
            }
            else
            {
                if ( t.IsEnum )
                {
                    return Enum.Parse( t, obj.ToString() );
                }
                else if ( t == typeof( Guid ) && obj is string )
                {
                    return new Guid( ( string ) obj );
                }
                else if ( t == typeof( string ) && obj is Guid )
                {
                    return obj.ToString();
                }
                else
                {
                    return Convert.ChangeType( obj, t );
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the list of properties from the entity that should be stored or re-created.
        /// </summary>
        /// <param name="entity">The entity whose properties are to be retrieved.</param>
        /// <returns>A list of PropertyInfo objects.</returns>
        protected List<PropertyInfo> GetEntityProperties( IEntity entity )
        {
            Type entityType = GetEntityType( entity );

            if ( !CachedEntityProperties.ContainsKey( entityType ) )
            {
                //
                // Get all data member mapped properties and filter out any "local only"
                // properties that should not be exported.
                //
                CachedEntityProperties.AddOrReplace( entityType, entityType.GetProperties()
                    .Where( p => System.Attribute.IsDefined( p, typeof( DataMemberAttribute ) ) )
                    .Where( p => !System.Attribute.IsDefined( p, typeof( NotMappedAttribute ) ) )
                    .Where( p => !System.Attribute.IsDefined( p, typeof( DatabaseGeneratedAttribute ) ) )
                    .Where( p => p.Name != "Id" && p.Name != "Guid" )
                    .Where( p => p.Name != "ForeignId" && p.Name != "ForeignGuid" && p.Name != "ForeignKey" )
                    .Where( p => p.Name != "CreatedByPersonAliasId" && p.Name != "ModifiedByPersonAliasId" )
                    .ToList() );
            }

            return CachedEntityProperties[entityType];
        }

        /// <summary>
        /// Attempt to load an entity from the database based on it's Guid and entity type.
        /// </summary>
        /// <param name="entityType">The type of entity to load.</param>
        /// <param name="guid">The unique identifier of the entity.</param>
        /// <returns>The loaded entity or null if not found.</returns>
        public IEntity GetExistingEntity( string entityType, Guid guid )
        {
            var service = Reflection.GetServiceForEntityType( FindEntityType( entityType ), RockContext );

            if ( service != null )
            {
                var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );

                if ( getMethod != null )
                {
                    return ( IEntity ) getMethod.Invoke( service, new object[] { guid } );
                }
            }

            return null;
        }

        /// <summary>
        /// Attempt to load an entity from the database based on it's Guid and entity type.
        /// </summary>
        /// <param name="entityType">The type of entity to load.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The loaded entity or null if not found.
        /// </returns>
        public IEntity GetExistingEntity( string entityType, int id )
        {
            var service = Reflection.GetServiceForEntityType( FindEntityType( entityType ), RockContext );

            if ( service != null )
            {
                var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );

                if ( getMethod != null )
                {
                    return ( IEntity ) getMethod.Invoke( service, new object[] { id } );
                }
            }

            return null;
        }

        /// <summary>
        /// Find the given IEntity Type from the full class name. Uses a cache to
        /// save processing time.
        /// </summary>
        /// <param name="entityName">The full class name of the IEntity type.</param>
        /// <returns>The Type object for the class name or null if not found.</returns>
        public Type FindEntityType( string entityName )
        {
            if ( CachedEntityTypes.ContainsKey( entityName ) )
            {
                return CachedEntityTypes[entityName];
            }

            Type type = Reflection.FindType( typeof( IEntity ), entityName );

            if ( type != null )
            {
                CachedEntityTypes.Add( entityName, type );
            }

            return type;
        }

        /// <summary>
        /// Retrieve an enumerable list of processor objects for the given
        /// IEntity type.
        /// </summary>
        /// <param name="entityType">The Type object of the IEntity to get processors for.</param>
        /// <returns>Enumerable of IEntityProcessor objects that will pre- and post-process this entity.</returns>
        protected IEnumerable<IEntityProcessor> FindEntityProcessors( Type entityType )
        {
            if ( CachedProcessors.ContainsKey( entityType ) )
            {
                return CachedProcessors[entityType];
            }

            Type processorBaseType = typeof( EntityProcessor<> ).MakeGenericType( entityType );
            List<IEntityProcessor> processors = new List<IEntityProcessor>();
            foreach ( var processorType in Reflection.FindTypes( processorBaseType ) )
            {
                processors.Add( ( IEntityProcessor ) Activator.CreateInstance( processorType.Value ) );
            }

            CachedProcessors.Add( entityType, processors );

            return processors;
        }
    }
}
