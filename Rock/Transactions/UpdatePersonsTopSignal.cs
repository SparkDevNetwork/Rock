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

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Update all Person records that have a signal of the given SignalType. Because
    /// it is possible that thousands of Person records have this signal type, we
    /// process in the background and process in small chunks.
    /// </summary>
    public class UpdatePersonsTopSignal : ITransaction
    {
        /// <summary>
        /// Gets or sets the list of person identifiers to update.
        /// </summary>
        /// <value>
        /// The list of person identifiers to update.
        /// </value>
        public List<int> PersonIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonsTopSignal"/> class.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        public UpdatePersonsTopSignal( List<int> personIds )
        {
            PersonIds = personIds;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            while ( PersonIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personIdSet = PersonIds.Take( 100 );
                    PersonIds = PersonIds.Skip( 100 ).ToList();

                    new PersonService( rockContext ).Queryable()
                        .Where( p => personIdSet.Contains( p.Id ) )
                        .ToList()
                        .ForEach( p => p.CalculateSignals() );

                    rockContext.SaveChanges();
                }
            }
        }
    }
}