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
using System.Threading.Tasks;

using Rock.Cms.ContentCollection.Attributes;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Cms.ContentCollection.IndexDocuments
{
    /// <summary>
    /// The indexed details of a content channel item.
    /// </summary>
    /// <seealso cref="IndexDocumentBase" />
    internal class EventItemDocument : IndexDocumentBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the list of upcoming occurrence dates and times for
        /// this event item document.
        /// </summary>
        /// <value>
        /// The list of upcoming occurrence dates and times.
        /// </value>
        [IndexField( FieldType = IndexFieldType.DateTime )]
        public List<DateTime> EventItemOccurrences { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new document from the <see cref="EventItem"/>.
        /// </summary>
        /// <param name="eventItem">The item to use as the source for this document.</param>
        /// <param name="source">The collection source this document belongs to.</param>
        /// <returns>A new instance of <see cref="EventItemDocument"/> that represents the event item.</returns>
        internal static async Task<EventItemDocument> LoadByModelAsync( EventItem eventItem, ContentCollectionSourceCache source )
        {
            var documentId = GetDocumentId( eventItem.Id, source.Id );

            var document = new EventItemDocument
            {
                Id = documentId,
                EntityId = eventItem.Id,
                Name = eventItem.Name,
                NameSort = eventItem.Name,
                Content = eventItem.Summary,
                SourceId = source.Id,
                SourceGuid = source.Guid,
                SourceIdKey = IdHasher.Instance.GetHash( source.Id ),
                SourceType = typeof( EventCalendar ).FullName,
                ItemType = typeof( EventItem ).FullName
            };

            var dates = eventItem.EventItemOccurrences
                .Where( o => o.NextStartDateTime.HasValue && o.NextStartDateTime.Value >= RockDateTime.Now )
                .Select( o => o.NextStartDateTime.Value )
                .OrderBy( d => d )
                .ToList();

            if ( dates.Any() )
            {
                document.RelevanceDateTime = dates.First();
                document.Year = document.RelevanceDateTime.Value.Year;
                document.EventItemOccurrences = dates.Take( source.OccurrencesToShow ).ToList();

                var yearValue = document.Year.ToString();
                FieldValueHelper.AddFieldValue( source.ContentCollectionId, nameof( document.Year ), yearValue, yearValue );
            }
            else
            {
                document.RelevanceDateTime = null;
                document.Year = null;
                document.EventItemOccurrences = new List<DateTime>();
            }

            document.AddPersonalizationData( eventItem, source );
            document.AddIndexableAttributes( eventItem, source );
            await document.AddExistingTrendingDataAsync( source );

            return document;
        }

        #endregion
    }
}
