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
using Rock.Data;

namespace Rock.Tests.Integration
{
    public static class TestExtensionMethods
    {
        /// <summary>
        /// Get an Entity from a collection by matching the specified identifier, either an ID or a Guid value.
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

            return result;
        }
    }
}
