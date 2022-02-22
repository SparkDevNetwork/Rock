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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class ContentChannelItem
    {
        /// <summary>
        /// Save hook implementation for <see cref="ContentChannelItem"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ContentChannelItem>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
                    Entity.ChildItems.Clear();
                    Entity.ParentItems.Clear();

                    Entity.DeleteRelatedSlugs( RockContext );
                }
                else
                {
                    Entity.AssignItemGlobalKey( RockContext );
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();

                var contentChannelItemSerivce = new ContentChannelItemService( RockContext );
                var contentChannelSlugSerivce = new ContentChannelItemSlugService( RockContext );

                if ( !contentChannelSlugSerivce.Queryable().Any( a => a.ContentChannelItemId == Entity.Id ) && contentChannelItemSerivce.Queryable().Any( a => a.Id == Entity.Id ) )
                {
                    contentChannelSlugSerivce.SaveSlug( Entity.Id, Entity.Title, null );
                }
            }
        }
    }
}