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

namespace Rock.Tasks
{
    /// <summary>
    /// Deletes a binary file
    /// </summary>
    public sealed class DeleteBinaryFileAttribute : BusStartedTask<DeleteBinaryFileAttribute.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( message.BinaryFileGuid );
                if ( binaryFile != null )
                {
                    string guidAsString = binaryFile.Guid.ToString();

                    // If any attribute still has this file as a default value, don't delete it
                    if ( new AttributeService( rockContext ).Queryable().Any( a => a.DefaultValue == guidAsString ) )
                    {
                        return;
                    }

                    // If any attribute value still has this file as a value, don't delete it
                    if ( new AttributeValueService( rockContext ).Queryable().Any( a => a.Value == guidAsString ) )
                    {
                        return;
                    }

                    binaryFileService.Delete( binaryFile );

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the binary file unique identifier.
            /// </summary>
            /// <value>
            /// The binary file unique identifier.
            /// </value>
            public Guid BinaryFileGuid { get; set; }
        }
    }
}