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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Transaction to execute when a page's name is changed
    /// </summary>
    public sealed class AddPageRenameInteraction : BusStartedTask<AddPageRenameInteraction.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var pageCache = PageCache.Get( message.PageGuid );

            if ( pageCache == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var interactionComponentService = new InteractionComponentService( rockContext );
            var componentQuery = interactionComponentService.QueryByPage( pageCache );

            rockContext.BulkUpdate( componentQuery, ic => new InteractionComponent { Name = pageCache.InternalName } );
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the page unique identifier.
            /// </summary>
            public Guid PageGuid { get; set; }
        }
    }
}