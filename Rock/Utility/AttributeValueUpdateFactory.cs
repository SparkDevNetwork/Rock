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
using System.Collections.Concurrent;

using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// A helper factory for creating <see cref="AttributeValueUpdate"/> objects.
    /// </summary>
    public static class AttributeValueUpdateFactory
    {
        private static readonly ConcurrentDictionary<Guid, int> _attributeIdCache = new ConcurrentDictionary<Guid, int>();

        /// <summary>
        /// Creates an <see cref="AttributeValueUpdate"/> with a date/time value.
        /// </summary>
        public static AttributeValueUpdate Create( Guid attributeGuid, DateTime? value )
        {
            return Create( attributeGuid, value.ToISO8601DateString() );
        }

        /// <summary>
        /// Creates an <see cref="AttributeValueUpdate"/> with an integer value.
        /// </summary>
        public static AttributeValueUpdate Create( Guid attributeGuid, int? value )
        {
            return Create( attributeGuid, value.ToStringSafe() );
        }

        /// <summary>
        /// Creates an <see cref="AttributeValueUpdate"/> with a decimal value.
        /// </summary>
        public static AttributeValueUpdate Create( Guid attributeGuid, decimal? value )
        {
            return Create( attributeGuid, value.ToStringSafe() );
        }

        /// <summary>
        /// Creates an <see cref="AttributeValueUpdate"/> with a string value.
        /// </summary>
        public static AttributeValueUpdate Create( Guid attributeGuid, string value )
        {
            var attributeId = _attributeIdCache.GetOrAdd( attributeGuid, guid =>
            {
                var attribute = AttributeCache.Get( guid );
                return attribute?.Id ?? 0;
            } );

            if ( attributeId == 0 )
            {
                // Return 0 ID so consuming logic can handle or log it
                return new AttributeValueUpdate { AttributeId = 0, Value = value };
            }

            return new AttributeValueUpdate
            {
                AttributeId = attributeId,
                Value = value
            };
        }
    }

    /// <summary>
    /// Minimal representation of a pending attribute value update.
    /// </summary>
    public sealed class AttributeValueUpdate
    {
        /// <summary>
        /// Gets or sets the attribute Id to update.
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the raw attribute value to persist.
        /// </summary>
        public string Value { get; set; }
    }
}
