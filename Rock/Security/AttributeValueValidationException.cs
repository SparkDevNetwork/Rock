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

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// This is a special use exception that is thrown from within the PostSave
    /// hook on AttributeValue. Do not use it for anything else.
    /// </summary>
    [RockInternal( "19.2", keepInternalForever: true )]
    public class AttributeValueValidationException : Exception
    {
        private readonly string _capturedTrace;

        /// <summary>
        /// Creates a new instance of <see cref="AttributeValueValidationException"/>.
        /// </summary>
        /// <param name="attributeCache">The attribute cache that identifies the attribute that caused the validation failure.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="capturedTrace">The captured stack trace.</param>
        public AttributeValueValidationException( AttributeCache attributeCache, int entityId, string reason, string capturedTrace )
            : base( GetExceptionMessage( attributeCache, entityId, reason ) )
        {
            _capturedTrace = capturedTrace;
        }

        private static string GetExceptionMessage( AttributeCache attributeCache, int entityId, string reason )
        {
            var entityTypeCache = attributeCache.EntityTypeId.HasValue
                ? EntityTypeCache.Get( attributeCache.EntityTypeId.Value )
                : null;

            if ( entityTypeCache != null )
            {
                return $"The value of the '{attributeCache.Name}' attribute (id: {attributeCache.Id}) on {entityTypeCache.Name} id {entityId} {reason}.";
            }

            return $"The value of the '{attributeCache.Name}' global attribute (id: {attributeCache.Id}) on entity id {entityId} {reason}.";
        }

        /// <inheritdoc/>
        public override string StackTrace => _capturedTrace ?? base.StackTrace;
    }
}
