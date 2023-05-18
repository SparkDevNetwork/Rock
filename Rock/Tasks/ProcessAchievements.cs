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
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Task to process achievements for updated source entities
    /// </summary>
    public sealed class ProcessAchievements : BusStartedTask<ProcessAchievements.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute( Message message )
        {
            var type = Type.GetType( message.EntityTypeName );
            var entityTypeId = EntityTypeCache.GetId( type );
            if ( entityTypeId == null || !AchievementTypeCache.HasActiveAchievementTypesForEntityTypeId(entityTypeId.Value) )
            {
                return;
            }

            var entity = Reflection.GetIEntityForEntityType( type, message.EntityGuid );

            if ( entity == null )
            {
                return;
            }

            AchievementTypeCache.ProcessAchievements( entity );
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the entity type name.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public string EntityTypeName { get; set; }

            /// <summary>
            /// Gets or sets the entity guid.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public Guid EntityGuid { get; set; }
        }
    }
}