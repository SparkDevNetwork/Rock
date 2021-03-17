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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ProcessEntityTypeBulkIndex : BusStartedTask<ProcessEntityTypeBulkIndex.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var entityType = EntityTypeCache.Get( message.EntityTypeId );
            Type type = entityType.GetEntityType();

            if ( type != null )
            {
                object classInstance = Activator.CreateInstance( type, null );
                MethodInfo bulkItemsMethod = type.GetMethod( "BulkIndexDocuments" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    bulkItemsMethod.Invoke( classInstance, null );
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the viewer person id.
            /// </summary>
            /// <value>
            /// The viewer person id.
            /// </value>
            public int EntityTypeId { get; set; }
        }
    }
}