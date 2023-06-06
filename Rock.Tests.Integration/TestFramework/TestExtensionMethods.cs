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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;

namespace Rock.Tests.Integration
{
    public static class TestExtensionMethods
    {
        /// <summary>
        /// Get an Entity from a collection by matching the specified identifier, either an ID, a Guid value or a Name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static T GetByIdentifier<T>( this IEnumerable<T> entities, object identifier )
            where T : IEntity
        {
            T result = default( T );

            var key = identifier.ToStringSafe();
            var id = key.AsIntegerOrNull();

            if ( id.HasValue )
            {
                result = entities.FirstOrDefault( e => e.Id == id.Value );
                return result;
            }

            var guid = key.AsGuidOrNull();

            if ( guid.HasValue )
            {
                result = entities.FirstOrDefault( e => e.Guid == guid );
                return result;
            }

            result = GetByName( entities, key );

            return result;
        }

        /// <inheritdoc cref="TestExtensionMethods.GetByIdentifier" />
        public static T GetByIdentifierOrThrow<T>( this IEnumerable<T> entities, object identifier )
            where T : IEntity
        {
            var result = GetByIdentifier( entities, identifier );

            Assert.IsNotNull( result, $"Invalid Entity Reference [EntityType={typeof(T).Name}, Identifier={identifier}]" );

            return result;
        }

        /// <summary>
        /// Get an Entity from a collection by matching a value in the Name property, if it exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="name"></param>
        /// <param name="nameProperty"></param>
        /// <returns></returns>
        public static T GetByName<T>( this IEnumerable<T> entities, object name, string nameProperty = "Name" )
            where T : IEntity
        {
            T result = default( T );

            // If the name field does not exist, return the default value.
            var entityType = typeof( T );
            if ( entityType.GetProperty( nameProperty ) == null )
            {
                return result;
            }

            // Construct a predicate expression for a match with the specified name field, and return the first match.
            var parameter = Expression.Parameter( entityType, "entity" );
            var expEquals = Expression.Equal( Expression.Property( parameter, nameProperty ), Expression.Constant( name.ToStringSafe() ) );
            var expLambda = Expression.Lambda( expEquals, parameter ); 

            var predicate = expLambda.Compile() as System.Func<T, bool>;
            result = entities.FirstOrDefault( predicate );

            return result;
        }

    }
}
