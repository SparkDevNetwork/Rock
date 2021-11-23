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

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonSearchKey
    {
        #region Methods

        /// <summary>
        /// This method is called in the
        /// <see cref="M:Rock.Data.Model`1.PreSaveChanges(Rock.Data.DbContext,System.Data.Entity.Infrastructure.DbEntityEntry,System.Data.Entity.EntityState)" />
        /// method. Use it to populate <see cref="P:Rock.Data.Model`1.HistoryItems" /> if needed.
        /// These history items are queued to be written into the database post save (so that they
        /// are only written if the save actually occurs).
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        protected override void BuildHistoryItems( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            // Sometimes, especially if the model is being deleted, some properties might not be
            // populated, but we can query to try to get their original value. We need to use a new
            // rock context to get the actual value from the DB
            var rockContext = new RockContext();
            var service = new PersonSearchKeyService( rockContext );
            var originalModel = service.Queryable( "PersonAlias" )
                .FirstOrDefault( fpsa => fpsa.Id == Id );

            // Use the original value for the person alias or the new value if that is not set
            var personId = ( originalModel?.PersonAlias ?? PersonAlias )?.PersonId;

            if ( !personId.HasValue )
            {
                // If this model is new, it won't have any virtual properties hydrated or an original
                // record in the database
                if ( PersonAliasId.HasValue )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var personAlias = personAliasService.Get( PersonAliasId.Value );
                    personId = personAlias?.PersonId;
                }

                // We can't log history if we don't know who the saved account belongs to
                if ( !personId.HasValue )
                {
                    return;
                }
            }

            History.HistoryVerb verb;

            switch ( state )
            {
                case EntityState.Added:
                    verb = History.HistoryVerb.Add;
                    break;
                case EntityState.Deleted:
                    verb = History.HistoryVerb.Delete;
                    break;
                case EntityState.Modified:
                    verb = History.HistoryVerb.Modify;
                    break;
                default:
                    // As of now, there is no requirement to log other events
                    return;
            }

            var caption = verb == History.HistoryVerb.Modify ?
                "Person Search Key" :
                GetCaptionForHistory( originalModel?.SearchValue ?? SearchValue, originalModel?.SearchTypeValueId ?? SearchTypeValueId );

            var historyChangeList = new History.HistoryChangeList();

            if ( verb != History.HistoryVerb.Modify )
            {
                historyChangeList.AddChange( verb, History.HistoryChangeType.Record, "Person Search Key" );
            }
            else
            {
                History.EvaluateChange( historyChangeList, $"SearchValue", entry.OriginalValues["SearchValue"].ToStringSafe(), SearchValue, false );

                var originalSearchType = DefinedValueCache.Get( entry.OriginalValues["SearchTypeValueId"].ToStringSafe().AsInteger() );
                var currentSearchType = DefinedValueCache.Get( SearchTypeValueId );
                History.EvaluateChange( historyChangeList, $"SearchType", originalSearchType?.Value, currentSearchType?.Value, false );
            }

            HistoryItems = HistoryService.GetChanges(
                typeof( Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                personId.Value,
                historyChangeList,
                caption,
                typeof( PersonSearchKey ),
                Id,
                dbContext.GetCurrentPersonAlias()?.Id,
                dbContext.SourceOfChange );
        }

        /// <summary>
        /// Gets the caption for history.
        /// </summary>
        /// <param name="searchKeyValue">The search key value.</param>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns></returns>
        private string GetCaptionForHistory( string searchKeyValue, int definedValueId )
        {
            var definedValue = DefinedValueCache.Get( definedValueId );
            return $"{ definedValue?.Value ?? "<Unknown Key Type>" }: '{ searchKeyValue ?? "<No key value>" }'";
        }

        #endregion
    }
}
