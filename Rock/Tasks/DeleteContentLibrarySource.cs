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
using System.Linq;
using System.Threading.Tasks;

using Rock.Cms.ContentLibrary;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Calls <see cref="IContentLibraryIndexer.DeleteAllContentLibrarySourceDocumentsAsync(int)"/> for the specified <see cref="Rock.Model.ContentLibrarySource"/>.
    /// </summary>
    public sealed class DeleteContentLibrarySource : BusStartedTaskAsync<DeleteContentLibrarySource.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override async Task ExecuteAsync( Message message )
        {
            var contentLibraryEntityTypes = EntityTypeCache.All().Where( et => et.IsContentLibraryIndexingEnabled );

            foreach ( var entityTypeCache in contentLibraryEntityTypes )
            {
                var indexer = ( IContentLibraryIndexer ) Activator.CreateInstance( entityTypeCache.ContentLibraryIndexerType );

                await indexer.DeleteAllContentLibrarySourceDocumentsAsync( message.SourceId );
            }
        }

        /// <summary>
        /// Message that identifies the entity to be processed.
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the source identifier to be processed.
            /// </summary>
            /// <value>
            /// The source identifier to be processed.
            /// </value>
            public int SourceId { get; set; }
        }
    }
}
