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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseList;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the knowledge bases the current person is authorized to view as
    /// a grid of cards. Selecting a card navigates to the knowledge base detail
    /// page; the add affordance navigates to the same page in add mode.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Knowledge Base List" )]
    [Category( "AI" )]
    [Description( "Displays the knowledge bases the current person is authorized to view." )]
    [IconCssClass( "ti ti-books" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page that will show the knowledge base details.",
        IsRequired = true,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B3F2A847-9C5E-4D81-A2C7-3E1F4B5D6A09" )]
    [Rock.SystemGuid.BlockTypeGuid( "D7E4F0A2-1C3B-4982-B5D6-7C8E9F0A1B23" )]
    public class KnowledgeBaseList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string KnowledgeBaseId = "KnowledgeBaseId";
        }

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();
            var summaries = LoadKnowledgeBaseSummaries( RockContext, currentPerson );

            var box = new KnowledgeBaseListInitializationBox
            {
                KnowledgeBaseSummaries = summaries,
                IsAddEnabled = GetIsAddEnabled( currentPerson ),
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        #endregion RockBlockType Implementation

        #region Private Methods

        /// <summary>
        /// Determines whether the add affordance should be shown by checking
        /// whether the current person can edit a new <see cref="KnowledgeBase"/>
        /// at the entity-type scope.
        /// </summary>
        /// <param name="currentPerson">The currently authenticated person.</param>
        /// <returns><c>true</c> if the add affordance should be shown.</returns>
        private bool GetIsAddEnabled( Person currentPerson )
        {
            return new KnowledgeBase().IsAuthorized( Authorization.EDIT, currentPerson );
        }

        /// <summary>
        /// Loads the list of knowledge bases the current person is authorized
        /// to view, hydrating each with its folder count.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="currentPerson">The currently authenticated person.</param>
        /// <returns>A list of <see cref="KnowledgeBaseSummaryBag"/>.</returns>
        private List<KnowledgeBaseSummaryBag> LoadKnowledgeBaseSummaries( RockContext rockContext, Person currentPerson )
        {
            var authorizedKnowledgeBases = KnowledgeBaseCache.All( rockContext )
                .Where( kb => kb.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( kb => kb.Name )
                .ToList();

            if ( !authorizedKnowledgeBases.Any() )
            {
                return new List<KnowledgeBaseSummaryBag>();
            }

            // Folder counts come from the folder cache so no database round-trip
            // is needed to hydrate the cards. The cache exposes KnowledgeBaseId
            // on each entry, which is all we need to group.
            var folderCountsByKnowledgeBaseId = KnowledgeBaseFolderCache.All( rockContext )
                .GroupBy( f => f.KnowledgeBaseId )
                .ToDictionary( g => g.Key, g => g.Count() );

            var summaries = authorizedKnowledgeBases
                .Select( kb => new KnowledgeBaseSummaryBag
                {
                    Id = kb.Id,
                    Name = kb.Name,
                    Description = kb.Description,
                    FolderCount = folderCountsByKnowledgeBaseId.TryGetValue( kb.Id, out var count ) ? count : 0
                } )
                .ToList();

            summaries.ForEach( s => s.TranslateIdToIdKey() );

            return summaries;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.KnowledgeBaseId, "((Key))" )
            };
        }

        #endregion Private Methods
    }
}
