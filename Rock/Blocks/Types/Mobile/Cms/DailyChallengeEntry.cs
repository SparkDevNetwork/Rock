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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays a set of challenges for the individual to complete today.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Daily Challenge Entry" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays a set of challenges for the individual to complete today." )]
    [IconCssClass( "fa fa-tasks" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [ContentChannelField( "Content Channel",
        Description = "The content channel that describes the challenge.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.ContentChannel )]

    [IntegerField( "Allow Backfill in Days",
        Description = "The number of days the individual should be allowed to go back and fill in if they missed some days.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKeys.AllowBackfillInDays )]

    [IntegerField(
        "Challenge Cache Duration",
        Description = "Number of seconds to cache the challenge configuration.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 2,
        Key = AttributeKeys.CacheDuration )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the block.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY,
        IsRequired = true,
        DefaultValue = "3DA15C4B-BD5B-44AF-97CD-E9F5FD97B55A",
        Key = AttributeKeys.Template,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CMS_DAILY_CHALLENGE_ENTRY )]
    [Rock.SystemGuid.BlockTypeGuid( "B702FF5B-2488-42C7-AAE8-2DD99E82326D")]
    public class DailyChallengeEntry : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The attribute keys defined for the <see cref="DailyChallengeEntry"/> block.
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The allow backfill in days attribute key.
            /// </summary>
            public const string AllowBackfillInDays = "AllowBackfillInDays";

            /// <summary>
            /// The cache duration in seconds attribute key.
            /// </summary>
            public const string CacheDuration = "CacheDuration";

            /// <summary>
            /// The content channel attribute key.
            /// </summary>
            public const string ContentChannel = "ContentChannel";

            /// <summary>
            /// The template attribute key.
            /// </summary>
            public const string Template = "Template";
        }

        /// <summary>
        /// Gets the number of days the individual should be allowed to go back and fill in if they missed some days.
        /// </summary>
        /// <value>
        /// The number of days the individual should be allowed to go back and fill in if they missed some days.
        /// </value>
        protected int AllowBackfillInDays => GetAttributeValue( AttributeKeys.AllowBackfillInDays ).AsInteger();

        /// <summary>
        /// Gets the number of seconds to cache the challenge configuration.
        /// </summary>
        /// <value>
        /// The number of seconds to cache the challenge configuration.
        /// </value>
        protected int CacheDuration => GetAttributeValue( AttributeKeys.CacheDuration ).AsInteger();

        /// <summary>
        /// Gets the content channel unique identifier that describes the challenge.
        /// </summary>
        /// <value>
        /// The content channel unique identifier that describes the challenge.
        /// </value>
        protected Guid? ContentChannelGuid => GetAttributeValue( AttributeKeys.ContentChannel ).AsGuidOrNull();

        /// <summary>
        /// Gets the template to use when rendering the block.
        /// </summary>
        /// <value>
        /// The template to use when rendering the block.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 2 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                Template = Template
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the progress <see cref="InteractionChannel"/> identifier. This
        /// is the <see cref="InteractionChannel"/> that contains the
        /// <see cref="Interaction">Interactions</see> for tracking progress of
        /// a single day - that is the individual check marks.
        /// </summary>
        /// <returns>The identifier of the <see cref="InteractionChannel"/>.</returns>
        private static int GetProgressChannelId()
        {
            var challengeProgressMediumId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHALLENGE_PROGRESS ).Id;
            var contentChannelEntityTypeId = EntityTypeCache.GetId<ContentChannel>();
            var contentChannelItemEntityTypeId = EntityTypeCache.GetId<ContentChannelItem>();

            return InteractionChannelCache.GetChannelIdByTypeIdAndEntityId(
                challengeProgressMediumId,
                null,
                "Challenge Progress",
                contentChannelEntityTypeId,
                contentChannelItemEntityTypeId );
        }

        /// <summary>
        /// Gets the progress <see cref="InteractionComponent"/> identifier.
        /// This holds the <see cref="Interaction">Interactions</see> for a
        /// single challenge.
        /// </summary>
        /// <param name="interactionChannelId">The interaction channel identifier.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <param name="contentChannelName">Name of the content channel.</param>
        /// <returns>The identifier of the <see cref="InteractionComponent"/>.</returns>
        private static int GetProgressComponentId( int interactionChannelId, int contentChannelId, string contentChannelName )
        {
            return InteractionComponentCache.GetComponentIdByChannelIdAndEntityId(
                interactionChannelId,
                contentChannelId,
                contentChannelName );
        }

        /// <summary>
        /// Gets the challenge day interaction or creates a new one.
        /// </summary>
        /// <param name="rockContext">The rock context to use when interacting with the database.</param>
        /// <param name="challenge">The challenge that is currently being worked on.</param>
        /// <param name="dailyChallenge">The item that describes the challenge day.</param>
        /// <param name="forDate">The date to use when looking for the interaction, or the current date if <c>null</c>.</param>
        /// <returns>The <see cref="Interaction"/> for the specified day.</returns>
        private Interaction GetChallengeDayInteraction( RockContext rockContext, CachedChallenge challenge, CachedDailyChallenge dailyChallenge, DateTime? forDate )
        {
            var interactionService = new InteractionService( rockContext );

            // Get the channel that will track ongoing progress of all challenges.
            var channelId = GetProgressChannelId();

            // Get the component that will track progress for this challenge.
            var componentId = GetProgressComponentId( channelId, challenge.Id, challenge.Name );

            var now = RockDateTime.Now;
            if ( forDate.HasValue )
            {
                now = forDate.Value;
            }

            var dateKey = now.ToString( "yyyyMMdd" ).AsInteger();

            // Try to get an existing interaction for this day item.
            var interaction = interactionService.Queryable()
                .Where( i => i.InteractionComponentId == componentId
                    && i.EntityId == dailyChallenge.Id
                    && i.InteractionDateKey == dateKey
                    && i.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                .FirstOrDefault();

            /*
             * If an interaction was not found and we don't have a specific date
             * then there is a sort of race condition we need to account for.
             *
             * Say the individual opened the challenge page at 11:50pm and
             * completed all but one item and kept the page open until past
             * midnight. Then at 12:02am the next day they complete the last
             * item. In this case we will not find an interaction and instead
             * write a new interaction for today when really we should have
             * updated yesterday's interaction.
             * 
             * Daniel Hazelbaker - 9/16/2021
             */
            if ( interaction == null && !forDate.HasValue )
            {
                // Look for an interaction yesterday for the same daily challenge
                // that is not complete.
                var yesterdayDateKey = now.AddDays( -1 ).ToString( "yyyyMMdd" ).AsInteger();

                interaction = interactionService.Queryable()
                    .Where( i => i.InteractionComponentId == componentId
                        && i.EntityId == dailyChallenge.Id
                        && i.InteractionDateKey == yesterdayDateKey
                        && i.Operation == "INCOMPLETE"
                        && i.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .FirstOrDefault();
            }

            // If not found, create a new one.
            if ( interaction == null )
            {
                interaction = interactionService.CreateInteraction(
                    componentId,
                    RequestContext.ClientInformation?.Browser?.String,
                    null,
                    RequestContext.ClientInformation.IpAddress,
                    null );

                interaction.InteractionDateTime = now;
                interaction.EntityId = dailyChallenge.Id;
                interaction.PersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;

                interactionService.Add( interaction );
            }

            interaction.InteractionEndDateTime = RockDateTime.Now;

            return interaction;
        }

        /// <summary>
        /// Creates the interaction that signals any streak tracking that the
        /// challenges for a specific day have all been completed.
        /// </summary>
        /// <param name="rockContext">The rock context to use when interacting with the database.</param>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <param name="dailyChallenge">The item that describes the challenge day.</param>
        /// <param name="data">The data to save in the interaction data.</param>
        /// <param name="forDate">The optional date to apply to the interaction.</param>
        private void CreateDayCompleteInteraction( RockContext rockContext, CachedChallenge challenge, CachedDailyChallenge dailyChallenge, InteractionChallengeDayData data, DateTime? forDate )
        {
            var challengesMediumId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHALLENGES ).Id;
            var contentChannelItemEntityTypeId = EntityTypeCache.GetId<ContentChannelItem>();
            var interactionService = new InteractionService( rockContext );

            // Get the channel that holds interactions for this challenge type.
            var channelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId(
                challengesMediumId,
                challenge.Id,
                challenge.Name,
                contentChannelItemEntityTypeId.Value,
                null );

            // Get the component that holds interactions for this day.
            var componentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId(
                channelId,
                dailyChallenge.Id,
                dailyChallenge.Title );

            // Create the interaction to signify that todays challenge was
            // completed. A day can be completed multiple times over the
            // year.
            var interaction = interactionService.CreateInteraction(
                componentId,
                RequestContext.ClientInformation?.Browser?.String,
                null,
                RequestContext.ClientInformation.IpAddress,
                null );

            interaction.Operation = "COMPLETE";
            interaction.InteractionData = data.ToJson();
            interaction.PersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
            interaction.InteractionEndDateTime = RockDateTime.Now;

            if ( forDate.HasValue )
            {
                interaction.InteractionDateTime = forDate.Value;
            }

            interactionService.Add( interaction );
        }

        /// <summary>
        /// Gets the recent interactions for the given challenge. The interactions
        /// are filtered to those that match an item from the challenge and also
        /// to only include interactions for the currently in progress challenge.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <returns>A collection of <see cref="Interaction"/> objects.</returns>
        private List<Interaction> GetRecentInteractions( RockContext rockContext, CachedChallenge challenge )
        {
            var interactionService = new InteractionService( rockContext );
            var channelId = GetProgressChannelId();
            var componentId = GetProgressComponentId( channelId, challenge.Id, challenge.Name );

            var recentDateKey = RockDateTime.Now.AddDays( -( challenge.DailyChallenges.Count + AllowBackfillInDays + 1 ) ).ToString( "yyyyMMdd" ).AsInteger();
            var todayDateKey = RockDateTime.Now.ToString( "yyyyMMdd" ).AsInteger();

            var recentInteractions = interactionService.Queryable()
                .Where( i => i.InteractionComponentId == componentId
                    && i.InteractionDateKey >= recentDateKey
                    && i.InteractionDateKey <= todayDateKey
                    && i.EntityId.HasValue
                    && i.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                .ToList();

            // Discard any interactions that are not valid for this challenge.
            // This can happen if a day was removed after the challenge started.
            var validItemIds = challenge.DailyChallenges.Select( i => i.Id ).ToList();
            recentInteractions = recentInteractions
                .Where( i => validItemIds.Contains( i.EntityId.Value ) )
                .OrderBy( i => i.InteractionDateTime )
                .ToList();

            // Walk the interaction in reverse order and to find where the
            // current challenge sequence starts.
            var orderedItems = challenge.DailyChallenges.OrderBy( i => i.Order ).ToList();
            int currentOrder = int.MaxValue;
            int challengeStartIndex = 0;

            for ( int index = recentInteractions.Count - 1; index >= 0; index-- )
            {
                var interaction = recentInteractions[index];
                int order = orderedItems.FindIndex( i => i.Id == interaction.EntityId.Value );

                if ( order >= currentOrder )
                {
                    challengeStartIndex = index + 1;
                    break;
                }

                currentOrder = order;
            }

            // Now remove everything that is not part of this challenge sequence.
            if ( challengeStartIndex > 0 )
            {
                recentInteractions = recentInteractions.Skip( challengeStartIndex ).ToList();
            }

            if ( recentInteractions.Any() )
            {
                var lastInteraction = recentInteractions.Last();

                // If the last item is completed, is the last day of the challenge
                // and is not today, then return an empty list to signal starting a new
                // challenge. This catches situations where we might accidentally
                // return interactions for a past challenge the day after the
                // challenge has been completed (and they are going to start over).
                if ( lastInteraction.Operation == "COMPLETE" && lastInteraction.EntityId == orderedItems.Last().Id && lastInteraction.InteractionDateKey < todayDateKey )
                {
                    return new List<Interaction>();
                }

                // If we don't have all the interactions for this sequence then
                // that means we got out of order somehow. Just start over. This
                // actually means something went wrong elsewhere, but without
                // this check the person is stuck trying to fill in day 1 forever.
                int expectedCount = 1 + orderedItems.FindIndex( c => c.Id == lastInteraction.EntityId );
                if ( recentInteractions.Count != expectedCount )
                {
                    return new List<Interaction>();
                }
            }

            return recentInteractions;
        }

        /// <summary>
        /// Gets the missed days that can still be filled in by the user.
        /// </summary>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <param name="recentInteractions">The recent interactions.</param>
        /// <param name="allowBackfillInDays">The number of days back to allow backfill to happen.</param>
        /// <returns>A dictionary whose keys identify the dates that can be filled in and whose values specify the details for that day.</returns>
        internal static (IDictionary<DateTimeOffset, DailyChallenge>, Guid) GetMissedDays( CachedChallenge challenge, List<Interaction> recentInteractions, int allowBackfillInDays )
        {
            var missedDays = new Dictionary<DateTimeOffset, DailyChallenge>();
            var orderedItems = challenge.DailyChallenges.OrderBy( i => i.Order ).ToList();
            var minimumRecentDate = RockDateTime.Now.AddDays( -allowBackfillInDays ).Date;
            var hardLimitDate = minimumRecentDate.AddDays( -1 );
            bool keepMissedDateValues = false;

            // No interactions, so we have no way to fill in missing data.
            if ( !recentInteractions.Any() )
            {
                return (missedDays, orderedItems[0].Guid);
            }

            var lastInteraction = recentInteractions.OrderBy( i => i.InteractionDateTime ).LastOrDefault();
            int order = orderedItems.FindIndex( i => i.Id == lastInteraction.EntityId.Value );

            var startDate = RockDateTime.Now.Date;
            order += startDate.Subtract( lastInteraction.InteractionDateTime.Date ).Days;

            // Account for cases where they allow a backfill of 5 days and
            // were at the last challenge day and then skipped a few days.
            while ( order >= orderedItems.Count )
            {
                order--;
                startDate = startDate.AddDays( -1 );
            }

            // This is the place where we would continue filling in once all
            // the missed dates are filled in.
            int continueOrder = order;

            // Walk dates starting from "today" (even though it is skipped) and
            // fill in any missing dates until we find either a completed date,
            // get out of order, or hit the start of the challenge.
            for ( var date = startDate; order >= 0 && date >= hardLimitDate; order--, date = date.AddDays( -1 ) )
            {
                var interaction = recentInteractions
                    .Where( i => i.InteractionDateTime.Date == date )
                    .FirstOrDefault();

                if ( interaction != null )
                {
                    int thisOrder = orderedItems.FindIndex( i => i.Id == interaction.EntityId.Value );

                    // Order has changed, that means they started a new sequence,
                    // so ignore this and all previous dates.
                    if ( thisOrder != order )
                    {
                        break;
                    }
                }

                var data = interaction?.InteractionData?.FromJsonOrNull<InteractionChallengeDayData>();

                if ( date >= minimumRecentDate && date < RockDateTime.Now.Date )
                {
                    if ( data == null || !data.IsComplete )
                    {
                        missedDays.Add( date.ToRockDateTimeOffset(), GetDailyChallenge( orderedItems[order], data ) );
                    }
                }

                // If they started on the first day but didn't finish then
                // give them a chance to continue.
                if ( data != null )
                {
                    keepMissedDateValues = data.IsComplete || date >= minimumRecentDate;

                    // If they fully completed this day then stop looking for missed
                    // dates. You can't jump over dates.
                    if ( data.IsComplete )
                    {
                        break;
                    }
                }

            }

            // If there was no day completed in our range then force them to
            // start over.
            if ( !keepMissedDateValues )
            {
                return (new Dictionary<DateTimeOffset, DailyChallenge>(), orderedItems[0].Guid);
            }

            return (missedDays, orderedItems[continueOrder].Guid);
        }

        /// <summary>
        /// Gets the <see cref="DailyChallenge"/> for the associated <see cref="ContentChannelItem"/>.
        /// </summary>
        /// <param name="dailyChallenge">The item that describes this daily challenge.</param>
        /// <param name="data">The data from a previous session.</param>
        /// <returns>A new <see cref="DailyChallenge"/> instance.</returns>
        internal static DailyChallenge GetDailyChallenge( CachedDailyChallenge dailyChallenge, InteractionChallengeDayData data )
        {
            // Load all the challenges for the current day.
            var challengeItems = dailyChallenge.ChallengeItems
                .OrderBy( c => c.Order )
                .ToList();

            // Check if we have any previously saved user values to load back
            // in and send to the client.
            var challengeItemValues = new Dictionary<Guid, ChallengeItemValue>();
            if ( data != null )
            {
                foreach ( var userValue in data.Items )
                {
                    // We need to translate from the Id number to the Guid value.
                    var challengeItem = challengeItems.FirstOrDefault( i => i.Id == userValue.Key );

                    if ( challengeItem != null )
                    {
                        challengeItemValues[challengeItem.Guid] = new ChallengeItemValue
                        {
                            Guid = challengeItem.Guid,
                            IsComplete = userValue.Value.IsComplete,
                            Value = userValue.Value.Value
                        };
                    }
                }
            }

            return new DailyChallenge
            {
                Guid = dailyChallenge.Guid,
                HeaderContent = dailyChallenge.Content,
                ChallengeItems = challengeItems
                    .Select( c => new ChallengeItem
                    {
                        Guid = c.Guid,
                        Title = c.Title,
                        Content = c.Content,
                        InputType = c.AttributeValues.GetValueOrDefault( "InputType", string.Empty ),
                        AttributeValues = c.AttributeValues
                            .Where( v => v.Key != "InputType" )
                            .ToDictionary( v => v.Key, v => v.Value )
                    } )
                    .ToList(),
                ChallengeItemValues = challengeItemValues,
                AttributeValues = dailyChallenge.AttributeValues
            };
        }

        /// <summary>
        /// Gets the daily challenge for the specified date.
        /// </summary>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <param name="recentInteractions">The recent interactions.</param>
        /// <param name="challengeDate">The challenge date.</param>
        /// <returns>The <see cref="DailyChallenge"/> for the date or <c>null</c> if it could not be fulfilled.</returns>
        internal static DailyChallenge GetDailyChallengeForDate( CachedChallenge challenge, List<Interaction> recentInteractions, DateTime challengeDate )
        {
            var firstInteraction = recentInteractions.OrderBy( i => i.InteractionDateTime ).FirstOrDefault();

            if ( firstInteraction == null )
            {
                return null;
            }

            // Take the earliest interaction and start counting forward until
            // we get to our target date.
            var orderedItems = challenge.DailyChallenges.OrderBy( i => i.Order ).ToList();
            int order = orderedItems.FindIndex( i => i.Id == firstInteraction.EntityId.Value );
            for ( var date = firstInteraction.InteractionDateTime.Date; order < orderedItems.Count; date = date.AddDays( 1 ), order++ )
            {
                if ( date == challengeDate.Date )
                {
                    var interaction = recentInteractions.FirstOrDefault( i => i.InteractionDateTime.Date == date );
                    var data = interaction?.InteractionData.FromJsonOrNull<InteractionChallengeDayData>();

                    return GetDailyChallenge( orderedItems[order], data );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the daily challenge for the specified unique identifier.
        /// </summary>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <param name="recentInteractions">The recent interactions.</param>
        /// <param name="dailyChallengeGuid">The daily challenge unique identifier.</param>
        /// <returns>The <see cref="DailyChallenge"/> for the unique identifier or <c>null</c> if it could not be fulfilled.</returns>
        internal static DailyChallenge GetDailyChallengeForGuid( CachedChallenge challenge, List<Interaction> recentInteractions, Guid dailyChallengeGuid )
        {
            var firstInteraction = recentInteractions.OrderBy( i => i.InteractionDateTime ).FirstOrDefault();

            if ( firstInteraction == null )
            {
                return null;
            }

            // Take the earliest interaction and start counting forward until
            // we get to our target date.
            var orderedItems = challenge.DailyChallenges.OrderBy( i => i.Order ).ToList();
            int order = orderedItems.FindIndex( i => i.Id == firstInteraction.EntityId.Value );

            // Determine the date that correlates with the first item.
            var date = firstInteraction.InteractionDateTime.Date.AddDays( -order );

            // Find the index of the daily challenge we are after.
            order = orderedItems.FindIndex( i => i.Guid == dailyChallengeGuid );
            if ( order == -1 )
            {
                return null;
            }

            // Adjust the date to match up to that item.
            date = date.AddDays( order );

            // Try to load any existing interaction data for this entry.
            var interaction = recentInteractions.FirstOrDefault( i => i.InteractionDateTime.Date == date );
            var data = interaction?.InteractionData.FromJsonOrNull<InteractionChallengeDayData>();

            return GetDailyChallenge( orderedItems[order], data );
        }

        /// <summary>
        /// Gets the current challenge day that should be worked on by default.
        /// </summary>
        /// <param name="challenge">The challenge being worked on.</param>
        /// <param name="recentInteractions">The recent interactions.</param>
        /// <param name="allowBackfillInDays">The number of days back to allow a backfill.</param>
        /// <returns>The <see cref="ContentChannelItem"/> that identifies the daily challenge to use by default.</returns>
        internal static DailyChallenge GetCurrentDailyChallenge( CachedChallenge challenge, List<Interaction> recentInteractions, int allowBackfillInDays )
        {
            var minimumRecentDate = RockDateTime.Now.AddDays( -allowBackfillInDays ).Date;
            var lastInteraction = recentInteractions.Where( i => i.InteractionDateTime >= minimumRecentDate )
                .OrderBy( i => i.InteractionDateTime )
                .LastOrDefault();

            // If there are no recent interactions, then just start a new challenge.
            if ( lastInteraction == null )
            {
                var item = challenge.DailyChallenges.OrderBy( i => i.Order ).First();

                return GetDailyChallenge( item, null );
            }

            // If the most recent interaction is for today then return todays
            // daily challenge so they can continue.
            if ( lastInteraction.InteractionDateTime.Date == RockDateTime.Now.Date )
            {
                var item = challenge.DailyChallenges.SingleOrDefault( i => i.Id == lastInteraction.EntityId );
                var data = lastInteraction.InteractionData.FromJsonOrNull<InteractionChallengeDayData>();

                return GetDailyChallenge( item, data );
            }

            var orderedItems = challenge.DailyChallenges.OrderBy( i => i.Order ).ToList();

            // If the most recent interaction is for yesterday, then check if
            // that day was completed and if so return today daily challenge
            // so they can continue.
            if ( lastInteraction.InteractionDateTime.Date == RockDateTime.Now.Date.AddDays( -1 ) )
            {
                int order = orderedItems.FindIndex( i => i.Id == lastInteraction.EntityId.Value );
                var data = lastInteraction.InteractionData?.FromJsonOrNull<InteractionChallengeDayData>();

                if ( data != null && data.IsComplete && order != -1 && order + 1 < challenge.DailyChallenges.Count )
                {
                    return GetDailyChallenge( orderedItems[order + 1], null );
                }
            }

            // If all else fails, they are starting a new challenge.
            return GetDailyChallenge( orderedItems[0], null );
        }

        /// <summary>
        /// Determines whether the challenge day is complete.
        /// </summary>
        /// <param name="data">The challenge day data.</param>
        /// <param name="challengeItems">The challenge items.</param>
        /// <returns>
        ///   <c>true</c> if all challenge items for the day have been completed; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsDayComplete( InteractionChallengeDayData data, IEnumerable<CachedChallengeItem> challengeItems )
        {
            foreach ( var item in challengeItems )
            {
                if ( !data.Items.ContainsKey( item.Id ) || !data.Items[item.Id].IsComplete )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the <see cref="ChallengeDataResponse"/> with progress information on
        /// which individual challenge days have been completed.
        /// </summary>
        /// <param name="challengeData">The challenge data.</param>
        /// <param name="challenge">The challenge channel that describes this challenge.</param>
        /// <param name="missedDayGuids">The missed day unique identifiers.</param>
        /// <param name="recentInteractions">The recent interactions from the database.</param>
        /// <param name="continueDayGuid">The day that marks which day will be considered today if they continue with their missed days.</param>
        internal static void UpdateChallengeDataWithProgress( ChallengeDataResponse challengeData, CachedChallenge challenge, IList<Guid> missedDayGuids, List<Interaction> recentInteractions, Guid continueDayGuid )
        {
            if ( missedDayGuids.Any() )
            {
                // Assume everything that isn't a missed day is complete.
                bool isAssumedComplete = true;
                var continueChallenge = GetDailyChallengeForGuid( challenge, recentInteractions, continueDayGuid );

                foreach ( var item in challenge.DailyChallenges.OrderBy( i => i.Order ) )
                {
                    if ( item.Guid == continueChallenge.Guid )
                    {
                        isAssumedComplete = continueChallenge.IsComplete;
                    }

                    var state = new ChallengeState
                    {
                        Guid = item.Guid,
                        IsComplete = missedDayGuids.Contains( item.Guid ) ? false : isAssumedComplete
                    };

                    challengeData.Progress.Add( state );

                    // Every item that comes after the current day is not complete.
                    if ( item.Guid == continueDayGuid )
                    {
                        isAssumedComplete = false;
                    }
                }
            }
            else
            {
                // Assume everything up until today is complete.
                bool isAssumedComplete = true;

                foreach ( var item in challenge.DailyChallenges.OrderBy( i => i.Order ) )
                {
                    if ( item.Guid == challengeData.CurrentChallenge.Guid )
                    {
                        isAssumedComplete = challengeData.CurrentChallenge.IsComplete;
                    }

                    var state = new ChallengeState
                    {
                        Guid = item.Guid,
                        IsComplete = isAssumedComplete
                    };

                    challengeData.Progress.Add( state );

                    // Every item that comes after this one is not complete.
                    if ( item.Guid == challengeData.CurrentChallenge.Guid )
                    {
                        isAssumedComplete = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the cached challenge or load it from the database.
        /// </summary>
        /// <param name="challengeGuid">The challenge unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="CachedChallenge"/> or <c>null</c> if it wasn't found.</returns>
        private CachedChallenge GetCachedChallengeOrLoad( Guid challengeGuid, RockContext rockContext )
        {
            var challenge = GetCachedChallenge( challengeGuid );

            if ( challenge != null )
            {
                return challenge;
            }

            var contentChannelService = new ContentChannelService( rockContext );

            var challengeChannel = contentChannelService.Get( challengeGuid );

            if ( challengeChannel == null )
            {
                return null;
            }

            challenge = new CachedChallenge( challengeChannel );

            if ( CacheDuration > 0 )
            {
                AddCachedContent( challengeGuid, challenge, CacheDuration );
            }

            return challenge;
        }

        #endregion

        #region Caching Methods

        /// <summary>
        /// Returns the cache key for a specific challenge
        /// </summary>
        /// <param name="challengeGuid">The challenge unique identifier.</param>
        /// <returns>The key that is used to access this cached item.</returns>
        private static string GetCacheKey( Guid challengeGuid )
        {
            return $"{typeof( DailyChallengeEntry ).FullName}: {challengeGuid}";
        }

        /// <summary>
        /// Returns the cached challenge details.
        /// </summary>
        /// <param name="challengeGuid">The challenge identifier.</param>
        /// <returns>The cached challenge details or <c>null</c> if not found.</returns>
        private static CachedChallenge GetCachedChallenge( Guid challengeGuid )
        {
            return RockCache.Get( GetCacheKey( challengeGuid ), true ) as CachedChallenge;
        }

        /// <summary>
        /// Adds the cached challenge details.
        /// </summary>
        /// <param name="challengeGuid">The challenge unique identifier.</param>
        /// <param name="challenge">The challenge details.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        private static void AddCachedContent( Guid challengeGuid, CachedChallenge challenge, int cacheDuration )
        {
            var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
            RockCache.AddOrUpdate( GetCacheKey( challengeGuid ), string.Empty, challenge, expiration );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the challenge data for the person.
        /// </summary>
        /// <returns>The <see cref="ChallengeDataResponse"/> object.</returns>
        [BlockAction]
        public BlockActionResult GetChallengeData( DateTimeOffset? challengeDate = null )
        {
            // We cannot operate without a logged in person.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                var cachedChallenge = GetCachedChallengeOrLoad( ContentChannelGuid ?? Guid.Empty, rockContext );

                if ( cachedChallenge == null || cachedChallenge.DailyChallenges.Count == 0 )
                {
                    throw new Exception( "Content Channel was not found or configured property." );
                }

                // Get all the recent interactions that correlate to the current
                // challenge sequence.
                var recentInteractions = GetRecentInteractions( rockContext, cachedChallenge );

                (var missedDays, var continueDayGuid) = GetMissedDays( cachedChallenge, recentInteractions, AllowBackfillInDays );

                DailyChallenge currentChallenge;

                // We have been requested to return the challenge for a specific
                // date.
                if ( challengeDate.HasValue )
                {
                    currentChallenge = GetDailyChallengeForDate( cachedChallenge, recentInteractions, challengeDate.Value.Date );

                    if ( currentChallenge == null )
                    {
                        return ActionBadRequest();
                    }
                }
                else
                {
                    currentChallenge = GetCurrentDailyChallenge( cachedChallenge, recentInteractions, AllowBackfillInDays );
                }

                var challengeData = new ChallengeDataResponse
                {
                    MissedDates = missedDays,
                    CurrentChallenge = currentChallenge,
                    Progress = new List<ChallengeState>(),
                    TodaysChallengeGuid = continueDayGuid
                };

                UpdateChallengeDataWithProgress( challengeData, cachedChallenge, missedDays.Select( d => d.Value.Guid ).ToList(), recentInteractions, continueDayGuid );

                return ActionOk( challengeData );
            }
        }

        /// <summary>
        /// Gets the challenge data for a specific day (content channel item).
        /// </summary>
        /// <param name="dailyChallengeGuid">The unique identifier of the day whose challenge data is to be retrieved.</param>
        /// <returns>The <see cref="ChallengeDataResponse"/> object.</returns>
        [BlockAction]
        public BlockActionResult GetChallengeDataForDay( Guid dailyChallengeGuid )
        {
            // We cannot operate without a logged in person.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                var cachedChallenge = GetCachedChallengeOrLoad( ContentChannelGuid ?? Guid.Empty, rockContext );

                if ( cachedChallenge == null || cachedChallenge.DailyChallenges.Count == 0 )
                {
                    throw new Exception( "Content Channel was not found or configured property." );
                }

                // Get all the recent interactions that correlate to the current
                // challenge sequence.
                var recentInteractions = GetRecentInteractions( rockContext, cachedChallenge );

                var currentChallenge = GetDailyChallengeForGuid( cachedChallenge, recentInteractions, dailyChallengeGuid );

                if ( currentChallenge == null )
                {
                    return ActionBadRequest();
                }

                var challengeData = new ChallengeDataResponse
                {
                    CurrentChallenge = currentChallenge,
                    TodaysChallengeGuid = currentChallenge.Guid
                };

                return ActionOk( challengeData );
            }
        }

        /// <summary>
        /// Updates the challenge by completing a single item.
        /// </summary>
        /// <param name="challengeDayGuid">The challenge day unique identifier.</param>
        /// <param name="challengeItemGuid">The challenge item unique identifier.</param>
        /// <param name="isComplete">if set to <c>true</c> then the item is marked as complete.</param>
        /// <param name="value">The value the user entered to associate with the item.</param>
        /// <param name="forDate">The date that this challenge is being completed for or <c>null</c> to use todays date.</param>
        /// <returns>The result of the request.</returns>
        [BlockAction]
        public BlockActionResult UpdateChallenge( Guid challengeDayGuid, Guid challengeItemGuid, bool isComplete, string value, DateTimeOffset? forDate = null )
        {
            // We cannot operate without a logged in person.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                var cachedChallenge = GetCachedChallengeOrLoad( ContentChannelGuid ?? Guid.Empty, rockContext );

                // Get the challenge day they are currently working on.
                var dayItem = cachedChallenge.DailyChallenges.FirstOrDefault( d => d.Guid == challengeDayGuid );

                // Get all the challenge items for that day.
                var challengeItems = dayItem.ChallengeItems;

                // Find the challenge item they are updating.
                var challengeItem = challengeItems.FirstOrDefault( i => i.Guid == challengeItemGuid );

                // If they specified any arguments that we can't find then let
                // them know.
                if ( dayItem == null || challengeItem == null )
                {
                    return ActionNotFound();
                }

                // Get or create the interaction for this day.
                var interaction = GetChallengeDayInteraction( rockContext, cachedChallenge, dayItem, forDate?.DateTime );

                var data = interaction.InteractionData?.FromJsonOrNull<InteractionChallengeDayData>() ?? new InteractionChallengeDayData();

                // Trying to mark a challenge item as not complete when they
                // day has already been completed is not allowed.
                if ( !isComplete && IsDayComplete( data, challengeItems ) )
                {
                    return ActionBadRequest();
                }

                // Update the challenge item.
                data.Items[challengeItem.Id] = new InteractionChallengeItemData
                {
                    IsComplete = isComplete,
                    Value = value
                };

                var isDayComplete = IsDayComplete( data, challengeItems );
                data.IsComplete = isDayComplete;

                interaction.InteractionData = data.ToJson();
                interaction.Operation = isDayComplete ? "COMPLETE" : "INCOMPLETE";

                // If they completed the entire day then create the interaction
                // to record that the entire day is complete. This must be done
                // in a different place so that streaks can pick it up.
                if ( isDayComplete )
                {
                    CreateDayCompleteInteraction( rockContext, cachedChallenge, dayItem, data, forDate?.DateTime );
                }

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The <see cref="Interaction.InteractionData"/> stored with interactions
        /// related to the Challenge system.
        /// </summary>
        internal class InteractionChallengeDayData
        {
            /// <summary>
            /// Gets or sets a value indicating whether this day is complete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this day is complete; otherwise, <c>false</c>.
            /// </value>
            public bool IsComplete { get; set; }

            /// <summary>
            /// Gets or sets the challenge items.
            /// </summary>
            /// <value>
            /// The challenge items.
            /// </value>
            public IDictionary<int, InteractionChallengeItemData> Items { get; set; } = new Dictionary<int, InteractionChallengeItemData>();
        }

        /// <summary>
        /// Information about a single challenge item in <see cref="InteractionChallengeDayData"/>.
        /// </summary>
        internal class InteractionChallengeItemData
        {
            /// <summary>
            /// Gets or sets a value indicating whether this item is complete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this item is complete; otherwise, <c>false</c>.
            /// </value>
            public bool IsComplete { get; set; }

            /// <summary>
            /// Gets or sets the value entered by the user.
            /// </summary>
            /// <value>
            /// The value entered by the user.
            /// </value>
            public string Value { get; set; }
        }

        #endregion

        #region Temporary REST POCOs

        // These can be replaced with the proper versions in Rock.Common.Mobile
        // assembly in v13. They are temporary for v12.5 until we can get to
        // the proper version of Rock.Common.Mobile. -dsh

        /// <summary>
        /// The response object to the GetChallengeData block action. This
        /// contains all the information required to display the challenge
        /// to the user for them to fill out.
        /// </summary>
        internal class ChallengeDataResponse
        {
            /// <summary>
            /// Gets or sets the missed dates. The keys are the dates that have
            /// been missed in the current challenge progress and the values
            /// contain the <see cref="DailyChallenge"/> that can be used to
            /// complete the challenge.
            /// </summary>
            /// <value>
            /// The missed dates.
            /// </value>
            public IDictionary<DateTimeOffset, DailyChallenge> MissedDates { get; set; }

            /// <summary>
            /// Gets or sets the progress of the challenge. This is a list of
            /// all daily challenges. Each item contains the identifier of the
            /// daily challenge and whether it is complete or not. This can be
            /// used to generate a progress report for the user showing how
            /// close they are to completing the entire challenge.
            /// </summary>
            /// <value>
            /// The progress of the challenge.
            /// </value>
            public IList<ChallengeState> Progress { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of the daily challenge that
            /// indicates todays challenge. This can be different than the
            /// value in <see cref="CurrentChallenge"/> as it takes into account
            /// an ongoing challenge where they might have skipped some days.
            /// </summary>
            /// <value>
            /// The continue challenge unique identifier.
            /// </value>
            public Guid TodaysChallengeGuid { get; set; }

            /// <summary>
            /// Gets or sets the current daily challenge. If there are any
            /// items in <see cref="MissedDates"/> then this will contain
            /// the daily challenge for starting a new challenge.
            /// </summary>
            /// <value>
            /// The current daily challenge.
            /// </value>
            public DailyChallenge CurrentChallenge { get; set; }

            public string Template { get; set; }
        }

        /// <summary>
        /// A single day challenge to be completed by the individual.
        /// </summary>
        internal class DailyChallenge
        {
            /// <summary>
            /// Gets or sets the unique identifier of this challenge.
            /// </summary>
            /// <value>
            /// The unique identifier of this challenge.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the content to be displayed in the header.
            /// </summary>
            /// <value>
            /// The content to be displayed in the header.
            /// </value>
            public string HeaderContent { get; set; }

            /// <summary>
            /// Gets a value indicating whether this challenge is complete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this challenge is complete; otherwise, <c>false</c>.
            /// </value>
            public bool IsComplete => IsDayComplete();

            /// <summary>
            /// Gets or sets the challenge items.
            /// </summary>
            /// <value>
            /// The challenge items.
            /// </value>
            public ICollection<ChallengeItem> ChallengeItems { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="ChallengeItem"/> values. These
            /// represent the values that have been filled in by the user.
            /// The key is the unique identifier of the <see cref="ChallengeItem"/>.
            /// </summary>
            /// <value>
            /// The <see cref="ChallengeItem"/> values.
            /// </value>
            public IDictionary<Guid, ChallengeItemValue> ChallengeItemValues { get; set; }

            /// <summary>
            /// Gets or sets the custom attribute values associated with this item.
            /// </summary>
            /// <value>
            /// The custom attribute values associated with this item.
            /// </value>
            public IDictionary<string, string> AttributeValues { get; set; }
            /// <summary>
            /// Determines whether this <see cref="DailyChallenge"/> is complete.
            /// </summary>
            /// <returns>
            ///   <c>true</c> if this <see cref="DailyChallenge"/> is complete; otherwise, <c>false</c>.
            /// </returns>
            private bool IsDayComplete()
            {
                if ( ChallengeItems == null || ChallengeItems.Count == 0 )
                {
                    return true;
                }

                if ( ChallengeItemValues == null )
                {
                    return false;
                }

                foreach ( var item in ChallengeItems )
                {
                    var challengeItemValue = ChallengeItemValues.GetValueOrNull( item.Guid );

                    if ( challengeItemValue == null || !challengeItemValue.IsComplete )
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// A single challenge item for the individual to complete.
        /// </summary>
        internal class ChallengeItem
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the title of this item.
            /// </summary>
            /// <value>
            /// The title of this item.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the content to be displayed with the item.
            /// </summary>
            /// <value>
            /// The content to be displayed with the item.
            /// </value>
            public string Content { get; set; }

            /// <summary>
            /// Gets or sets the type of the input.
            /// </summary>
            /// <value>
            /// The type of the input.
            /// </value>
            public string InputType { get; set; }

            /// <summary>
            /// Gets or sets the custom attribute values associated with this item.
            /// </summary>
            /// <value>
            /// The custom attribute values associated with this item.
            /// </value>
            public IDictionary<string, string> AttributeValues { get; set; }
        }

        /// <summary>
        /// The values entered by the individual for a <see cref="ChallengeItem"/>.
        /// </summary>
        internal class ChallengeItemValue
        {
            /// <summary>
            /// Gets or sets the unique identifier of the item.
            /// </summary>
            /// <value>
            /// The unique identifier of the item.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the item is complete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if the item is complete; otherwise, <c>false</c>.
            /// </value>
            public bool IsComplete { get; set; }

            /// <summary>
            /// Gets or sets the value entered by the individual.
            /// </summary>
            /// <value>
            /// The value entered by the individual.
            /// </value>
            public string Value { get; set; }
        }

        /// <summary>
        /// The completed state of a challenge.
        /// </summary>
        internal class ChallengeState
        {
            /// <summary>
            /// Gets or sets the unique identifier of the item.
            /// </summary>
            /// <value>
            /// The unique identifier of the item.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this challenge item is complete.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this challenge item is complete; otherwise, <c>false</c>.
            /// </value>
            public bool IsComplete { get; set; }
        }

        #endregion

        #region Cache POCOs

        /// <summary>
        /// Cached version of a challenge which is stored in the database as
        /// a <see cref="ContentChannel"/>.
        /// </summary>
        [Serializable]
        internal class CachedChallenge
        {
            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get;}

            /// <summary>
            /// Gets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; }

            /// <summary>
            /// Gets the name of the challenge.
            /// </summary>
            /// <value>
            /// The name of the challenge.
            /// </value>
            public string Name { get; }

            /// <summary>
            /// Gets the daily challenges that make up this challenge.
            /// </summary>
            /// <value>
            /// The daily challenges that make up this challenge.
            /// </value>
            public ICollection<CachedDailyChallenge> DailyChallenges { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedChallenge"/> class.
            /// </summary>
            /// <param name="channel">The channel.</param>
            public CachedChallenge( ContentChannel channel )
            {
                Id = channel.Id;
                Guid = channel.Guid;
                Name = channel.Name;
                DailyChallenges = channel.Items.Select( i => new CachedDailyChallenge( i ) ).ToList();
            }
        }

        /// <summary>
        /// Cached version of a daily challenge which is stored in the database
        /// as a <see cref="ContentChannelItem"/>.
        /// </summary>
        [Serializable]
        internal class CachedDailyChallenge
        {
            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; }

            /// <summary>
            /// Gets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; }

            /// <summary>
            /// Gets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; }

            /// <summary>
            /// Gets the additional header content.
            /// </summary>
            /// <value>
            /// The additional header content.
            /// </value>
            public string Content { get; }

            /// <summary>
            /// Gets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            public int Order { get; }

            /// <summary>
            /// Gets the individual challenge items that make up this daily challenge.
            /// </summary>
            /// <value>
            /// The individual challenge items that make up this daily challenge.
            /// </value>
            public ICollection<CachedChallengeItem> ChallengeItems { get; }

            /// <summary>
            /// Gets or sets the attribute values.
            /// </summary>
            /// <value>
            /// The attribute values.
            /// </value>
            public IDictionary<string, string> AttributeValues { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedDailyChallenge"/> class.
            /// </summary>
            /// <param name="item">The item.</param>
            public CachedDailyChallenge( ContentChannelItem item )
            {
                Id = item.Id;
                Guid = item.Guid;
                Title = item.Title;
                Content = item.Content;
                Order = item.Order;
                ChallengeItems = item.ChildItems
                    .OrderBy( i => i.Order )
                    .Select( i => new CachedChallengeItem( i.ChildContentChannelItem, i.Order ) )
                    .ToList();

                if ( item.Attributes == null )
                {
                    item.LoadAttributes();
                }

                AttributeValues = item.AttributeValues.ToDictionary( v => v.Key, v => item.GetAttributeValue( v.Key ) ?? string.Empty );
            }
        }

        /// <summary>
        /// Cached version of a challenge item which is stoed in the database
        /// as a <see cref="ContentChannelItem"/>.
        /// </summary>
        [Serializable]
        internal class CachedChallengeItem
        {
            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; }

            /// <summary>
            /// Gets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; }

            /// <summary>
            /// Gets the title of this challenge item.
            /// </summary>
            /// <value>
            /// The title of this challenge item.
            /// </value>
            public string Title { get; }

            /// <summary>
            /// Gets the additional content to display with this item.
            /// </summary>
            /// <value>
            /// The additional content to display with this item.
            /// </value>
            public string Content { get; }

            /// <summary>
            /// Gets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            public int Order { get; }

            /// <summary>
            /// Gets or sets the attribute values.
            /// </summary>
            /// <value>
            /// The attribute values.
            /// </value>
            public IDictionary<string, string> AttributeValues { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedChallengeItem"/> class.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="order">Overrides the order for this item.</param>
            public CachedChallengeItem( ContentChannelItem item, int? order = null )
            {
                Id = item.Id;
                Guid = item.Guid;
                Title = item.Title;
                Content = item.Content;
                Order = order ?? item.Order;

                if ( item.Attributes == null )
                {
                    item.LoadAttributes();
                }

                AttributeValues = item.AttributeValues.ToDictionary( v => v.Key, v => item.GetAttributeValue( v.Key ) ?? string.Empty );
            }
        }

        #endregion
    }
}
