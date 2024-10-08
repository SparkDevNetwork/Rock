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

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Adds any <see cref="InteractionEntity"/> records for recently created
    /// entities.
    /// </summary>
    internal class AddInteractionEntityTransaction : AggregateTransaction<(int EntityTypeId, int EntityId, Guid InteractionGuid)>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInteractionEntityTransaction"/> class.
        /// </summary>
        /// <param name="entityTypeId">The <see cref="EntityType"/> identifier of the entity that was created.</param>
        /// <param name="entityId">The identifier of the <see cref="IEntity"/> that was created.</param>
        /// <param name="interactionGuid">The unique identifier of the interaction that was active when the entity was created.</param>
        public AddInteractionEntityTransaction( int entityTypeId, int entityId, Guid interactionGuid )
            : base( (entityTypeId, entityId, interactionGuid) )
        {
        }

        /// <inheritdoc/>
        protected override void Execute( IList<(int EntityTypeId, int EntityId, Guid InteractionGuid)> items )
        {
            InteractionEntityService.AddRelatedEntities( items );
        }
    }
}
