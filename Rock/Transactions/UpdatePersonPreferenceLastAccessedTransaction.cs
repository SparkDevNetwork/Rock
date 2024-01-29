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

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Updates any person preferences that were recently accessed.
    /// </summary>
    internal class UpdatePersonPreferenceLastAccessedTransaction : AggregateTransaction<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonPreferenceLastAccessedTransaction"/> class.
        /// </summary>
        /// <param name="preferenceId">The person preference identifier.</param>
        public UpdatePersonPreferenceLastAccessedTransaction( int preferenceId )
            : base( preferenceId )
        {
        }

        /// <inheritdoc/>
        protected override void Execute( IList<int> items )
        {
            // Get the distinct set of items that were accessed.
            var itemIds = items
                .Distinct()
                .ToList();

            var now = RockDateTime.Now;

            while ( itemIds.Any() )
            {
                try
                {
                    var batchIds = itemIds.Take( 250 );
                    itemIds = itemIds.Skip( 250 ).ToList();

                    // Load all person preference records in this batch and
                    // then update the last accessed date and time. It doesn't
                    // matter that this isn't the exact correct second. Since
                    // we only update once a day it doesn't matter.
                    using ( var rockContext = new RockContext() )
                    {
                        var personPreferenceService = new PersonPreferenceService( rockContext );
                        var personPreferences = personPreferenceService.Queryable()
                            .Where( pp => batchIds.Contains( pp.Id ) )
                            .ToList();

                        personPreferences.ForEach( pp => pp.LastAccessedDateTime = now );

                        rockContext.SaveChanges( true );
                    }
                }
                catch ( Exception ex )
                {
                    // Log the exception but keep going.
                    ExceptionLogService.LogException( ex );
                }
            }
        }
    }
}
