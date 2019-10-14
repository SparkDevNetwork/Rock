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

        #endregion IEntity extensions

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

        #endregion EntityType Extensions
    }
}
