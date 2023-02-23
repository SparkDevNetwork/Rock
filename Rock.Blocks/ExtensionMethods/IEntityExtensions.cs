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
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    internal static class IEntityExtensions
    {
        public static int? GetEntityId<TEntity>( this ListItemBag viewModel, RockContext rockContext )
            where TEntity : IEntity
        {
            var guid = viewModel?.Value.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            var entityType = EntityTypeCache.Get<TEntity>( false, rockContext );

            if ( entityType == null )
            {
                return null;
            }

            return Rock.Reflection.GetEntityIdForEntityType( entityType.Guid, guid.Value, rockContext );
        }
    }
}
