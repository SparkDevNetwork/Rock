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

using Rock.Data;

namespace Rock.Core.Automation
{
    /// <summary>
    /// Describes a single request that will pass through the automation system
    /// from the trigger to the events.
    /// </summary>
    internal class AutomationRequest
    {
        /// <summary>
        /// The well known keys that are used in the <see cref="Values"/>
        /// dictionary. These are not reserved but should be used when
        /// they make sense for a specific value.
        /// </summary>
        public static class KnownKeys
        {
            /// <summary>
            /// The primary entity for the automation request, which must be
            /// an <see cref="IEntity"/>. For example, on an entity saved
            /// trigger, this would be the entity that was saved. Another value
            /// might contain the Person (entity) that performed the save.
            /// </summary>
            public const string Entity = "Entity";
        }

        /// <summary>
        /// Gets the primary entity for the automation request if there is one.
        /// This will be <c>null</c> if there is no <c>Entity</c> value or if it
        /// is not an <see cref="IEntity"/>.
        /// </summary>
        public IEntity Entity
        {
            get
            {
                if ( Values.TryGetValue( KnownKeys.Entity, out var entity ) )
                {
                    return entity as IEntity;
                }

                return null;
            }
        }

        /// <summary>
        /// The values that make up the automation request.
        /// </summary>
        public IReadOnlyDictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    }
}
