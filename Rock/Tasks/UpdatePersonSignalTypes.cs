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
    /// Update all Person records that have a signal of the given SignalType. Because
    /// it is possible that thousands of Person records have this signal type, we
    /// process in the background and process in small chunks.
    /// </summary>
    public sealed class UpdatePersonSignalTypes : BusStartedTask<UpdatePersonSignalTypes.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            while ( message.PersonIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personIdSet = message.PersonIds.Take( 100 );
                    message.PersonIds = message.PersonIds.Skip( 100 ).ToList();

                    new PersonService( rockContext ).Queryable()
                        .Where( p => personIdSet.Contains( p.Id ) )
                        .ToList()
                        .ForEach( p => p.CalculateSignals() );

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
            /// Gets or sets the list of person identifiers to update.
            /// </summary>
            /// <value>
            /// The list of person identifiers to update.
            /// </value>
            public List<int> PersonIds { get; set; }
        }
    }
}