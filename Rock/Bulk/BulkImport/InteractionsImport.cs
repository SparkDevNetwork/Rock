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
using Rock.Model;

namespace Rock.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Import to POST to ~/api/Interactions/Import" )]
    public class InteractionsImport
    {
        /// <summary>
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>
        /// The interactions.
        /// </value>
        public List<InteractionImport> Interactions { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Interactions.Count} Interactions";
        }

        /// <summary>
        /// Gets the valid person alias ids.
        /// </summary>
        /// <returns></returns>
        internal HashSet<int> GetValidPersonAliasIds()
        {
            var distinctPersonAliasIds = this.Interactions.Where( a => a.Interaction.PersonAliasId.HasValue ).Select( a => a.Interaction.PersonAliasId.Value ).Distinct().ToList();
            List<int> validPersonAliasIdBuilder = new List<int>();

            using ( var rockContext = new RockContext() )
            {
                while ( distinctPersonAliasIds.Any() )
                {
                    /* 2020-05-14 MDP
                      * If there over a 1000 distinct PersonAliasIds, we'll query for valid ones in chunks of 1000
                      * this will prevent SQL complexity errors.
                    */

                    // get 1000 at a time to prevent SQL Complexity errors
                    List<int> distinctPersonAliasIdChunk = distinctPersonAliasIds.Take( 1000 ).ToList();
                    var validPersonAliasIdsChunk = new PersonAliasService( rockContext ).GetByIds( distinctPersonAliasIdChunk ).Select( a => a.Id );

                    // add the valid personIds that we found
                    validPersonAliasIdBuilder.AddRange( validPersonAliasIdsChunk );

                    // remove the ones  we already looked up and keep looping if there are still more to lookup
                    distinctPersonAliasIds = distinctPersonAliasIds.Where( a => !distinctPersonAliasIdChunk.Contains( a ) ).ToList();
                }

                HashSet<int> validPersonAliasIds = new HashSet<int>( validPersonAliasIdBuilder );
                return validPersonAliasIds;
            }
        }
    }
}
