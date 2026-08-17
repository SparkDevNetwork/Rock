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
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.PersonSuggestionNotice;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block for displaying a button and count of suggested people that can be used to navigate to person suggestion list block.
    /// </summary>
    [DisplayName( "Person Suggestion Notice" )]
    [Category( "Follow" )]
    [Description( "Block for displaying a button and count of suggested people that can be used to navigate to person suggestion list block." )]
    [IconCssClass( "ti ti-user-plus" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Suggestion Page",
        Key = AttributeKey.ListPage,
        Order = 0 )]

    [LinkedPage( "Followers Page",
        Key = AttributeKey.FollowersPage,
        Order = 1 )]

    [BooleanField( "Show Followers Page",
        Description = "Determines whether the link to the followers page should be shown",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowFollowersPage,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "59061706-F79B-4B26-B7B4-494ED3309D3A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A6DCFF22-3F31-40DC-AA82-1D285D300B4F" )]
    [Rock.SystemGuid.BlockTypeGuid( "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4" )]
    public class PersonSuggestionNotice : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ListPage = "ListPage";
            public const string FollowersPage = "FollowersPage";
            public const string ShowFollowersPage = "ShowFollowersPage";
        }

        private static class NavigationUrlKey
        {
            public const string ListPage = "ListPage";
            public const string FollowersPage = "FollowersPage";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PersonSuggestionNoticeBag, PersonSuggestionNoticeOptionsBag>
            {
                Bag = new PersonSuggestionNoticeBag
                {
                    SuggestionCount = GetSuggestionCount()
                },
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        /// <summary>
        /// Gets the URLs the block can navigate to. The followers page URL is
        /// only included when the block is configured to show that link.
        /// </summary>
        /// <returns>A dictionary of URLs keyed by <see cref="NavigationUrlKey"/> constants.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>
            {
                [NavigationUrlKey.ListPage] = this.GetLinkedPageUrl( AttributeKey.ListPage )
            };

            if ( GetAttributeValue( AttributeKey.ShowFollowersPage ).AsBoolean() )
            {
                urls[NavigationUrlKey.FollowersPage] = this.GetLinkedPageUrl( AttributeKey.FollowersPage );
            }

            return urls;
        }

        /// <summary>
        /// Gets the number of person suggestions for the current person, excluding
        /// people they already follow and suggestions they have ignored.
        /// </summary>
        /// <returns>The number of pending person suggestions.</returns>
        private int GetSuggestionCount()
        {
            var personAliasEntityType = EntityTypeCache.Get( typeof( PersonAlias ) );
            var currentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            if ( personAliasEntityType == null || !currentPersonAliasId.HasValue )
            {
                return 0;
            }

            // PersonAlias query for resolving followed and suggested entity ids to people.
            var personAliasQry = new PersonAliasService( RockContext )
                .Queryable().AsNoTracking();

            // Get all the people that the current person currently follows.
            var followedPersonIds = new FollowingService( RockContext )
                .Queryable().AsNoTracking()
                .Where( f =>
                    f.EntityTypeId == personAliasEntityType.Id &&
                    string.IsNullOrEmpty( f.PurposeKey ) &&
                    f.PersonAliasId == currentPersonAliasId.Value )
                .Join( personAliasQry, f => f.EntityId, p => p.Id, ( f, p ) => p.PersonId )
                .Distinct();

            // Count the person suggestions for the current person that they are not already following.
            return new FollowingSuggestedService( RockContext )
                .Queryable().AsNoTracking()
                .Where( s =>
                    s.SuggestionType != null &&
                    s.EntityTypeId == personAliasEntityType.Id &&
                    s.PersonAliasId == currentPersonAliasId.Value &&
                    s.Status != FollowingSuggestedStatus.Ignored )
                .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => new { s, p } )
                .Where( j => !followedPersonIds.Contains( j.p.PersonId ) )
                .Count();
        }

        #endregion
    }
}
