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
using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class ContentLibrarySource
    {
        /// <summary>
        /// Save hook implementation for <see cref="ContentLibrarySource"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ContentLibrarySource>
        {
            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();

                if ( State == EntityContextState.Deleted )
                {
                    var msg = new DeleteContentLibrarySource.Message
                    {
                        SourceId = Entity.Id
                    };

                    msg.SendWhen( RockContext.WrappedTransactionCompletedTask );
                }
                else if ( State == EntityContextState.Added )
                {
                    var msg = new AddContentLibrarySource.Message
                    {
                        SourceId = Entity.Id
                    };

                    msg.SendWhen( RockContext.WrappedTransactionCompletedTask );
                }
            }
        }
    }
}