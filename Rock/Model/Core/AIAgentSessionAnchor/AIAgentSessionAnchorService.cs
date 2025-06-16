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
using Rock.AI.Agent;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AIAgentSessionAnchorService
    {
        internal static void UpdateFromEntity( AIAgentSessionAnchor anchor, RockContext rockContext )
        {
            ContextAnchor context = null;

            if ( anchor.EntityTypeId == EntityTypeCache.Get<Person>( true, rockContext ).Id )
            {
                var person = new PersonService( rockContext ).Get( anchor.EntityId );

                if ( person != null )
                {
                    context = new ContextAnchor
                    {
                        Name = person.FullName
                    };
                }
            }
            else if ( anchor.EntityTypeId == EntityTypeCache.Get<Group>( true, rockContext ).Id )
            {
                var group = new GroupService( rockContext ).Get( anchor.EntityId );

                if ( group != null )
                {
                    context = new GroupContextAnchor
                    {
                        Name = group.Name,
                        GroupTypeId = group.GroupTypeId,
                        GroupTypeName = GroupTypeCache.Get( group.GroupTypeId, rockContext )?.Name
                    };
                }
            }

            if ( context != null )
            {
                context.EntityTypeId = anchor.EntityTypeId;
                context.EntityTypeName = EntityTypeCache.Get( anchor.EntityTypeId, rockContext )?.Name;
                context.EntityId = anchor.EntityId;

                anchor.Name = context.Name.Truncate( 100, false );
                anchor.PayloadJson = context.ToJson();
            }
            else
            {
                anchor.Name = string.Empty;
                anchor.PayloadJson = string.Empty;
            }

            if ( anchor.Name.IsNullOrWhiteSpace() )
            {
                anchor.Name = $"EntityTypeId={anchor.EntityTypeId}, EntityId={anchor.EntityId}";
            }

            anchor.LastRefreshedDateTime = RockDateTime.Now;
        }

        private class GroupContextAnchor : ContextAnchor
        {
            public int GroupTypeId { get; set; }

            public string GroupTypeName { get; set; }
        }
    }
}
