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

using System.Threading.Tasks;

using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Cms.ContentCollection.IndexDocuments
{
    /// <summary>
    /// The indexed details of a content channel item.
    /// </summary>
    /// <seealso cref="IndexDocumentBase" />
    internal class ContentChannelItemDocument : IndexDocumentBase
    {
        #region Methods

        /// <summary>
        /// Creates a new document from the <see cref="ContentChannelItem"/>.
        /// </summary>
        /// <param name="contentChannelItem">The item to use as the source for this document.</param>
        /// <param name="source">The collection source this document belongs to.</param>
        /// <returns>A new instance of <see cref="ContentChannelItemDocument"/> that represents the content channel item.</returns>
        internal static async Task<ContentChannelItemDocument> LoadByModelAsync( ContentChannelItem contentChannelItem, ContentCollectionSourceCache source )
        {
            var documentId = GetDocumentId( contentChannelItem.Id, source.Id );

            var document = new ContentChannelItemDocument
            {
                Id = documentId,
                EntityId = contentChannelItem.Id,
                Name = contentChannelItem.Title,
                NameSort = contentChannelItem.Title,
                Content = contentChannelItem.Content,
                SourceId = source.Id,
                SourceGuid = source.Guid,
                SourceIdKey = IdHasher.Instance.GetHash( source.Id ),
                SourceType = typeof( ContentChannel ).FullName,
                ItemType = typeof( ContentChannelItem ).FullName,
                RelevanceDateTime = contentChannelItem.StartDateTime,
                Year = contentChannelItem.StartDateTime.Year
            };

            var yearValue = document.Year.ToString();
            FieldValueHelper.AddFieldValue( source.ContentCollectionId, nameof( document.Year ), yearValue, yearValue );

            document.AddPersonalizationData( contentChannelItem, source );
            document.AddIndexableAttributes( contentChannelItem, source );
            await document.AddExistingTrendingDataAsync( source );

            return document;
        }

        #endregion
    }
}
