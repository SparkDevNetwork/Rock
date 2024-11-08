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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Rock.Data.Entity and EntityType extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Sets the value to the entity's id value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="entity">The entity.</param>
        public static void SetValue( this ListControl listControl, IEntity entity )
        {
            listControl.SetValue( entity == null ? "0" : entity.Id.ToString() );
        }

        /// <summary>
        /// Sets the value to the entity's id value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="entity">The entity.</param>
        public static void SetValue( this ListControl listControl, Rock.Web.Cache.IEntityCache entity )
        {
            listControl.SetValue( entity == null ? "0" : entity.Id.ToString() );
        }

        #region IEntity extensions

        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="id">The id.</param>
        public static void RemoveEntity<T>( this List<T> list, int id ) where T : Rock.Data.IEntity
        {
            var item = list.FirstOrDefault( a => a.Id.Equals( id ) );
            if ( item != null )
            {
                list.Remove( item );
            }
        }

        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="guid">The GUID.</param>
        public static void RemoveEntity<T>( this List<T> list, Guid guid ) where T : Rock.Data.IEntity
        {
            var item = list.FirstOrDefault( a => a.Guid.Equals( guid ) );
            if ( item != null )
            {
                list.Remove( item );
            }
        }

        /// <summary>
        /// Determines whether the Entity is an EF Proxy of the Entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static bool IsDynamicProxyEntity( this IEntity entity )
        {
            if ( entity != null )
            {
                return entity.GetType().IsDynamicProxyType();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     <para>
        ///     Changes the order of an entity in a list of entities by using its
        ///     item key (Id, Guid, IdKey) and the item key of the item it should
        ///     be placed before.
        ///     </para>
        ///     <para>
        ///     If <typeparamref name="TEntity"/> is of type <see cref="IOrdered"/>
        ///     then the <see cref="IOrdered.Order"/> property will be updated
        ///     on all entities after the move is performed.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to be moved.</typeparam>
        /// <param name="entities">The list of entities containing the item to move.</param>
        /// <param name="itemKey">The key that identifies the item to be moved.</param>
        /// <param name="beforeItemKey">The key that identifies the item it should be placed before.</param>
        /// <returns><c>true</c> if the reorder operation was successful or <c>false</c> if one of the items could not be found.</returns>
        internal static bool ReorderEntity<TEntity>( this List<TEntity> entities, string itemKey, string beforeItemKey )
            where TEntity : IEntity
        {
            if ( itemKey.IsNullOrWhiteSpace() )
            {
                return false;
            }

            TEntity item;

            // Find the source entity to be moved.
            if ( Guid.TryParse( itemKey, out var itemGuid ) )
            {
                item = entities.FirstOrDefault( s => s.Guid == itemGuid );
            }
            else if ( int.TryParse( itemKey, out var itemId ) )
            {
                item = entities.FirstOrDefault( s => s.Id == itemId );
            }
            else
            {
                item = entities.FirstOrDefault( s => s.IdKey == itemKey );
            }

            if ( item == null )
            {
                return false;
            }

            // If there is a before item key then find it.
            if ( beforeItemKey.IsNotNullOrWhiteSpace() )
            {
                TEntity beforeItem;

                if ( Guid.TryParse( beforeItemKey, out var beforeItemGuid ) )
                {
                    beforeItem = entities.FirstOrDefault( s => s.Guid == beforeItemGuid );
                }
                else if ( int.TryParse( beforeItemKey, out var beforeItemId ) )
                {
                    beforeItem = entities.FirstOrDefault( s => s.Id == beforeItemId );
                }
                else
                {
                    beforeItem = entities.FirstOrDefault( s => s.IdKey == beforeItemKey );
                }

                if ( beforeItem == null )
                {
                    return false;
                }

                entities.Remove( item );
                entities.Insert( entities.IndexOf( beforeItem ), item );
            }
            else
            {
                // Otherwise just move it to the end.
                entities.Remove( item );
                entities.Add( item );
            }

            // If the TEntity type implements IOrdered then update the Order
            // property of all the entities as well.
            if ( typeof( IOrdered ).IsAssignableFrom( typeof( TEntity ) ) )
            {
                // Set all the Order properties.
                for ( int i = 0; i < entities.Count; i++ )
                {
                    ( entities[i] as IOrdered ).Order = i;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts a single entity to a ListItemBag.
        /// </summary>
        /// <param name="entity">The entity to be represented by the bag.</param>
        /// <returns>A <see cref="ListItemBag"/> that represents the entity.</returns>
        public static ListItemBag ToListItemBag( this IEntity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = new ListItemBag
            {
                Value = entity.Guid.ToString(),
                Text = entity.ToString()
            };

            return viewModel;
        }

        /// <summary>
        /// Converts a single entity to a ListItemBag.
        /// </summary>
        /// <param name="entity">The entity to be represented by the bag.</param>
        /// <param name="text">The value to use for the Text property of the ListItemBag.</param>
        /// <returns>A <see cref="ListItemBag"/> that represents the entity.</returns>
        public static ListItemBag ToListItemBag( this IEntity entity, string text )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = new ListItemBag
            {
                Value = entity.Guid.ToString(),
                Text = text
            };

            return viewModel;
        }

        /// <summary>
        /// Converts a collection of entities to ListItemBags.
        /// </summary>
        /// <param name="entities">The entities to be represented by the bags.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> that represents the entities.</returns>
        public static List<ListItemBag> ToListItemBagList( this IEnumerable<IEntity> entities )
        {
            if ( entities == null )
            {
                return new List<ListItemBag>();
            }

            return entities.Select( e => e.ToListItemBag() ).ToList();
        }

        /// <summary>
        /// Converts a collection of entities to ListItemBags.
        /// </summary>
        /// <param name="entities">The entities to be represented by the bags.</param>
        /// <param name="toText">A function that can set the Text property of the ListItemBag for the entity.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> that represents the entities.</returns>
        public static List<ListItemBag> ToListItemBagList( this IEnumerable<IEntity> entities, Func<IEntity, string> toText )
        {
            if ( entities == null )
            {
                return new List<ListItemBag>();
            }

            return entities.Select( e => e.ToListItemBag( toText( e ) ) ).ToList();
        }

        /// <summary>
        /// Converts a single entity to a ListItemBag.
        /// </summary>
        /// <param name="entity">The entity to be represented by the bag.</param>
        /// <returns>A <see cref="ListItemBag"/> that represents the entity.</returns>
        public static ListItemBag ToListItemBag( this IEntityCache entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = new ListItemBag
            {
                Value = entity.Guid.ToString(),
                Text = entity.ToString()
            };

            return viewModel;
        }

        /// <summary>
        /// Converts a collection of entities to ListItemBags.
        /// </summary>
        /// <param name="entities">The entities to be represented by the bags.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> that represents the entities.</returns>
        public static List<ListItemBag> ToListItemBagList( this IEnumerable<IEntityCache> entities )
        {
            if ( entities == null )
            {
                return new List<ListItemBag>();
            }

            return entities.Select( e => e.ToListItemBag() ).ToList();
        }


        #endregion IEntity extensions

        #region IModel Extensions

        /// <summary>
        /// Gets the <see cref="EntityAuditBag"/> that contains the information
        /// used by the standard (Obsidian) audit detail control.
        /// </summary>
        /// <param name="model">The model whose audit details are requested.</param>
        /// <returns>An instance of <see cref="EntityAuditBag"/> that represents the audit information.</returns>
        internal static EntityAuditBag GetEntityAuditBag( this IModel model )
        {
            return new EntityAuditBag
            {
                Id = model.Id,
                IdKey = model.IdKey,
                Guid = model.Guid,
                CreatedByPersonId = model.CreatedByPersonAlias?.PersonId,
                CreatedByName = model.CreatedByPersonAlias?.Person?.FullName,
                CreatedRelativeTime = model.CreatedDateTime?.ToRelativeDateString(),
                CreatedDateTime = model.CreatedDateTime,
                ModifiedByPersonId = model.ModifiedByPersonAlias?.PersonId,
                ModifiedByName = model.ModifiedByPersonAlias?.Person?.FullName,
                ModifiedRelativeTime = model.ModifiedDateTime?.ToRelativeDateString(),
                ModifiedDateTime = model.ModifiedDateTime
            };
        }

        #endregion

        #region EntityType Extensions

        /// <summary>
        /// Gets the name of the friendly type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetFriendlyTypeName( this Type type )
        {
            if ( type.Namespace == null )
            {
                // Anonymous types will not have a namespace
                return "Item";
            }

            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            if ( type.Namespace.Equals( "Rock.Model" ) )
            {
                var entityType = EntityTypeCache.Get( type, false );
                if ( entityType != null && entityType.FriendlyName != null )
                {
                    return entityType.FriendlyName;
                }
                else
                {
                    return type.Name.SplitCase();
                }
            }
            else
            {
                return type.Name.SplitCase();
            }
        }

        /// <summary>
        /// Determines whether the type is a DynamicProxy of the type (type is from System.Data.Entity.DynamicProxies)
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsDynamicProxyType( this Type type )
        {
            if ( type != null )
            {
                return type.Namespace == "System.Data.Entity.DynamicProxies";
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the System.Type of the individual elements of the specified collection type.
        /// </summary>
        /// <param name="collectionType">A collection Type.</param>
        /// <returns></returns>
        public static Type GetCollectionElementType( this Type collectionType )
        {
            if ( collectionType == null )
            {
                return null;
            }

            // Get the type of an Array element.
            if ( collectionType.IsArray )
            {
                return collectionType.GetElementType();
            }

            // Get the element type of IEnumerable<T>.
            if ( collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
            {
                return collectionType.GetGenericArguments()[0];
            }

            // Get the element type of an object that implements or extends IEnumerable<T>.
            var implementedType = collectionType.GetInterfaces()
                .Where( t => t.IsGenericType &&
                        t.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
                .Select( t => t.GenericTypeArguments[0] ).FirstOrDefault();
            if ( implementedType != null )
            {
                return implementedType;
            }

            return null;
        }

        /// <summary>
        /// Returns a flag indicating if the specified type can be represented by the Rock.Data.IEntity interface.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRockEntityType( this Type type )
        {
            if ( type == null )
            {
                return false;
            }

            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            var isIEntity = type.GetInterface( nameof( Rock.Data.IEntity ) ) != null;
            return isIEntity;
        }

        /// <summary>
        /// Returns a flag indicating if the specified object can be represented by the Rock.Data.IEntity interface.
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        public static bool IsRockEntityObject<T>( this T inputObject )
            where T : class
        {
            var isEntityType = IsRockEntityType( inputObject?.GetType() );
            return isEntityType;
        }

        /// <summary>
        /// Returns a flag indicating if the specified object represents a collection of elements that implement the Rock.Data.IEntity interface.
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        public static bool IsRockEntityCollection<T>( this T inputObject )
            where T : class
        {
            // First, try to detect the element type using the Type system.
            // This is available even if the input object is null.
            var elementType = GetCollectionElementType( typeof( T ) );

            var isRockEntity = IsRockEntityType( elementType );
            if ( !isRockEntity )
            {
                // If the collection is not strongly-typed, try to inspect the first element.
                // This will force the collection to materialize if it is defined by a Linq query,
                // which may be expensive and therefore should be performed last.
                var inputEnumerable = inputObject as IEnumerable<object>;
                var firstElement = inputEnumerable?.FirstOrDefault();

                isRockEntity = IsRockEntityObject( firstElement );
            }

            return isRockEntity;
        }

        #endregion EntityType Extensions
    }
}
