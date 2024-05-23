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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.PersonSuggestionList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block for displaying people that have been suggested to current person to follow.
    /// </summary>
    [DisplayName( "Person Suggestion List" )]
    [Category( "Follow" )]
    [Description( "Block for displaying people that have been suggested to current person to follow." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "fd10b5b8-494c-4665-8f1f-8f92f2194f7e" )]
    [Rock.SystemGuid.BlockTypeGuid( "d29619d6-2ffe-4ef7-adaf-14db588944ea" )]
    [CustomizedGrid]
    public class PersonSuggestionList : RockListBlockType<PersonSuggestionList.PersonSuggestionData>
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string FilterIncludeIgnored = "filter-include-ignored";
        }

        #endregion

        #region Properties

        protected bool FilterIncludeIgnored => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIncludeIgnored )
            .AsBoolean();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonSuggestionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonSuggestionListOptionsBag GetBoxOptions()
        {
            var options = new PersonSuggestionListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonSuggestionData> GetListQueryable( RockContext rockContext )
        {
            var personAliasEntityType = EntityTypeCache.Get( "Rock.Model.PersonAlias" );
            var currentPersonAlias = GetCurrentPerson().PrimaryAlias;
            IEnumerable<PersonSuggestionData> qry = new List<PersonSuggestionData>();

            if ( personAliasEntityType != null && currentPersonAlias != null )
            {
                // PersonAlias query for joining the followed entity id to
                var personAliasQry = new PersonAliasService( rockContext ).Queryable();

                // Get all the people that the current person currently follows
                var followedPersonIds = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityType.Id &&
                        string.IsNullOrEmpty( f.PurposeKey ) &&
                        f.PersonAliasId == currentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => p.PersonId )
                    .Distinct();

                // Get all the person suggestions for the current person that they are not already following
                qry = new FollowingSuggestedService( rockContext )
                    .Queryable( "SuggestionType" )
                    .Where( s => FilterIncludeIgnored || s.Status != FollowingSuggestedStatus.Ignored )
                    .Where( s =>
                        s.SuggestionType != null &&
                        s.EntityTypeId == personAliasEntityType.Id &&
                        s.PersonAliasId == currentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => new { s, p } )
                    .Where( j => !followedPersonIds.Contains( j.p.PersonId ) )
                    .Select( j => new PersonSuggestionData
                    {
                        FollowingSuggested = j.s,
                        LastPromotedDateTime = j.s.LastPromotedDateTime,
                        StatusChangedDateTime = j.s.StatusChangedDateTime,
                        ReasonNote = j.s.SuggestionType.ReasonNote,
                        Status = j.s.Status,
                        Person = j.p.Person,
                        LastName = j.p.Person.LastName,
                        NickName = j.p.Person.NickName
                    } );
            }

            return qry.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonSuggestionData> GetOrderedListQueryable( IQueryable<PersonSuggestionData> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.Person.LastName ).ThenBy( p => p.Person.NickName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonSuggestionData> GetGridBuilder()
        {
            return new GridBuilder<PersonSuggestionData>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.FollowingSuggested?.IdKey )
                .AddDateTimeField( "lastPromotedDateTime", a => a.LastPromotedDateTime )
                .AddTextField( "reasonNote", a => a.ReasonNote )
                .AddField( "status", a => a.Status )
                .AddField( "guid", a => a.FollowingSuggested?.Guid.ToString() )
                .AddDateTimeField( "statusChangedDateTime", a => a.StatusChangedDateTime )
                .AddTextField( "personIdKey", a => a.Person?.IdKey )
                .AddPersonField( "person", a => a.Person );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Follows the specified persons.
        /// </summary>
        /// <param name="selectedItems">The selected suggestions.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Follow( List<Guid> selectedItems )
        {
            // Get the personAlias entity type
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );
            var currentPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;

            // If we have a valid current person and items were selected
            if ( personAliasEntityType != null && currentPersonAliasId.HasValue && selectedItems.Any() )
            {
                // Get all the person alias id's that were selected
                var followingSuggestedService = new FollowingSuggestedService( RockContext );
                var selectedPersonAliasIds = followingSuggestedService
                    .Queryable()
                    .Where( f => selectedItems.Contains( f.Guid ) )
                    .Select( f => f.EntityId )
                    .Distinct()
                    .ToList();

                // Find any of the selected person alias ids that current person is already following
                var followingService = new FollowingService( RockContext );
                var alreadyFollowing = followingService
                    .Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityType.Id &&
                        f.PersonAliasId == currentPersonAliasId.Value &&
                        string.IsNullOrEmpty( f.PurposeKey ) &&
                        selectedPersonAliasIds.Contains( f.EntityId ) )
                    .Select( f => f.EntityId )
                    .Distinct()
                    .ToList();

                // For each selected person alias id that the current person is not already following
                foreach ( var personAliasId in selectedPersonAliasIds.Where( p => !alreadyFollowing.Contains( p ) ) )
                {
                    // Add a following record
                    var following = new Following
                    {
                        EntityTypeId = personAliasEntityType.Id,
                        EntityId = personAliasId,
                        PersonAliasId = currentPersonAliasId.Value
                    };
                    followingService.Add( following );
                }

                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        /// <summary>
        /// Updates the status of the selected items to <see cref="FollowingSuggestedStatus.Ignored"/>
        /// </summary>
        /// <param name="selectedItems">The selected suggestions.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Ignore( List<Guid> selectedItems )
        {
            // If any items were selected
            if ( selectedItems.Any() )
            {
                // Update the status of each suggestion to be ignored
                var followingSuggestedService = new FollowingSuggestedService( RockContext );
                foreach ( var suggestion in followingSuggestedService.Queryable()
                    .Where( f => selectedItems.Contains( f.Guid ) ) )
                {
                    suggestion.Status = FollowingSuggestedStatus.Ignored;
                }

                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The data format used when building the results for the grid
        /// </summary>
        public class PersonSuggestionData
        {
            public string IdKey { get; set; }
            public Person Person { get; set; }
            public FollowingSuggested FollowingSuggested { get; set; }
            public DateTime? LastPromotedDateTime { get; set; }
            public DateTime? StatusChangedDateTime { get; set; }
            public string ReasonNote { get; set; }
            public FollowingSuggestedStatus Status { get; set; }
            public string LastName { get; set; }
            public string NickName { get; set; }
        }

        #endregion
    }
}
