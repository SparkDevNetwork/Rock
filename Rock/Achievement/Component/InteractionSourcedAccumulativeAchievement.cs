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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Achievement.Component
{
    /// <summary>
    /// Use to track achievements earned by accumulating interactions
    /// </summary>
    /// <seealso cref="AchievementComponent" />
    [Description( "Use to track achievements earned by accumulating interactions" )]
    [Export( typeof( AchievementComponent ) )]
    [ExportMetadata( "ComponentName", "Interactions: Accumulative" )]

    [InteractionChannelInteractionComponentField(
        "Interaction Channel and Component",
        Description = "The source interaction channel and component from which achievements are earned.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.InteractionChannelComponent )]

    [IntegerField(
        "Number to Accumulate",
        Description = "The number of interactions required to earn this achievement.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.NumberToAccumulate )]

    [DateField(
        "Start Date",
        Description = "The date that defines when the interactions must occur on or after.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.StartDateTime )]

    [DateField(
        "End Date",
        Description = "The date that defines when the interactions must occur on or before.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.EndDateTime )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT )]
    public class InteractionSourcedAccumulativeAchievement : AchievementComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementComponent" /> class.
        /// </summary>
        public InteractionSourcedAccumulativeAchievement() : base(
            new AchievementConfiguration( typeof( Interaction ), typeof( PersonAlias ) ),
            new HashSet<string> { AttributeKey.InteractionChannelComponent } )
        {
        }

        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The number to accumulate
            /// </summary>
            public const string NumberToAccumulate = "NumberToAccumulate";

            /// <summary>
            /// The Start Date Time
            /// </summary>
            public const string StartDateTime = "StartDateTime";

            /// <summary>
            /// The End Date Time
            /// </summary>
            public const string EndDateTime = "EndDateTime";

            /// <summary>
            /// The channel and component
            /// </summary>
            public const string InteractionChannelComponent = "InteractionChannelComponent";
        }

        #endregion Keys

        /// <summary>
        /// Should the achievement type process attempts if the given source entity has been modified in some way.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <returns></returns>
        public override bool ShouldProcess( AchievementTypeCache achievementTypeCache, IEntity sourceEntity )
        {
            var interaction = sourceEntity as Interaction;

            if ( interaction == null )
            {
                return false;
            }

            var channel = GetInteractionChannelCache( achievementTypeCache );

            if ( channel == null )
            {
                return true;
            }

            var component = GetInteractionComponentCache( achievementTypeCache );

            if ( component == null )
            {
                component = InteractionComponentCache.Get( interaction.InteractionComponentId );
                return component.InteractionChannelId == channel.Id;
            }

            return interaction.InteractionComponentId == component.Id;
        }

        /// <summary>
        /// Gets the source entities query. This is the set of source entities that should be passed to the process method
        /// when processing this achievement type.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IQueryable<IEntity> GetSourceEntitiesQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext )
        {
            var component = GetInteractionComponentCache( achievementTypeCache );
            var service = new InteractionService( rockContext );
            var query = service.Queryable();

            if ( component != null )
            {
                return query.Where( i => i.InteractionComponentId == component.Id );
            }

            var channel = GetInteractionChannelCache( achievementTypeCache );

            if ( channel != null )
            {
                return query.Where( i => i.InteractionComponent.InteractionChannelId == channel.Id );
            }

            return query;
        }

        /// <summary>
        /// Determines whether this achievement type applies given the set of filters. The filters could be the query string
        /// of a web request.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>
        ///   <c>true</c> if [is relevant to all filters] [the specified filters]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsRelevantToAllFilters( AchievementTypeCache achievementTypeCache, List<KeyValuePair<string, string>> filters )
        {
            if ( filters.Count == 0 )
            {
                return true;
            }

            if ( filters.Count > 2 )
            {
                return false;
            }

            return filters.All( f =>
            {
                if ( f.Key.Equals( "InteractionChannelId", StringComparison.OrdinalIgnoreCase ) )
                {
                    return f.Value.AsInteger() == GetInteractionChannelCache( achievementTypeCache )?.Id;
                }

                if ( f.Key.Equals( "InteractionComponentId", StringComparison.OrdinalIgnoreCase ) )
                {
                    return f.Value.AsInteger() == GetInteractionComponentCache( achievementTypeCache )?.Id;
                }

                return false;
            } );
        }

        /// <summary>
        /// Gets the achiever attempt query. This is the query (not enumerated) that joins attempts of this achievement type with the
        /// achiever entities, as well as the name (<see cref="AchieverAttemptItem.AchieverName"/> that could represent the achiever
        /// in a grid or other such display.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public override IQueryable<AchieverAttemptItem> GetAchieverAttemptQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext )
        {
            var attemptService = new AchievementAttemptService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            var attemptQuery = attemptService.Queryable().Where( aa => aa.AchievementTypeId == achievementTypeCache.Id );
            var personAliasQuery = personAliasService.Queryable();

            return attemptQuery.Join(
                    personAliasQuery,
                    aa => aa.AchieverEntityId,
                    pa => pa.Id,
                    ( aa, pa ) => new AchieverAttemptItem
                    {
                        AchievementAttempt = aa,
                        Achiever = pa,
                        AchieverName = pa.Person.NickName + " " + pa.Person.LastName
                    } );
        }

        /// <summary>
        /// Processes the specified achievement type cache for the source entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <returns>The set of attempts that were created or updated</returns>
        public override HashSet<AchievementAttempt> Process( RockContext rockContext, AchievementTypeCache achievementTypeCache, IEntity sourceEntity )
        {
            var interaction = sourceEntity as Interaction;
            var updatedAttempts = new HashSet<AchievementAttempt>();

            // If we cannot link the interaction to a person, then there is nothing to do
            if ( interaction?.PersonAliasId == null )
            {
                return updatedAttempts;
            }

            // If the achievement type is not active (or null) then there is nothing to do
            if ( achievementTypeCache?.IsActive != true )
            {
                return updatedAttempts;
            }

            // If there are unmet prerequisites, then there is nothing to do
            var achievementTypeService = new AchievementTypeService( rockContext );
            var unmetPrerequisites = achievementTypeService.GetUnmetPrerequisites( achievementTypeCache.Id, interaction.PersonAliasId.Value );

            if ( unmetPrerequisites.Any() )
            {
                return updatedAttempts;
            }

            // Get all of the attempts for this interaction and achievement combo, ordered by start date DESC so that
            // the most recent attempts can be found with FirstOrDefault
            var achievementAttemptService = new AchievementAttemptService( rockContext );
            var attempts = achievementAttemptService.GetOrderedAchieverAttempts( achievementAttemptService.Queryable(), achievementTypeCache, interaction.PersonAliasId.Value );

            var mostRecentSuccess = attempts.FirstOrDefault( saa => saa.AchievementAttemptEndDateTime.HasValue && saa.IsSuccessful );
            var overachievementPossible = achievementTypeCache.AllowOverAchievement;
            var successfulAttemptCount = attempts.Count( saa => saa.IsSuccessful );
            var maxSuccessesAllowed = achievementTypeCache.MaxAccomplishmentsAllowed ?? int.MaxValue;

            // If the most recent success is still open and overachievement is allowed, then update it
            if ( overachievementPossible && mostRecentSuccess != null && !mostRecentSuccess.IsClosed )
            {
                UpdateOpenAttempt( mostRecentSuccess, achievementTypeCache, interaction );
                updatedAttempts.Add( mostRecentSuccess );

                if ( !mostRecentSuccess.IsClosed )
                {
                    // New records can only be created once the open records are all closed
                    return updatedAttempts;
                }
            }

            // If the success count limit has been reached, then no more processing should be done
            if ( successfulAttemptCount >= maxSuccessesAllowed )
            {
                return updatedAttempts;
            }

            // Everything after the most recent success is on the table for deletion. Successes should not be
            // deleted. Everything after a success might be recalculated because of data changes.
            // Try to reuse these attempts if they match for continuity, but if the start date is changed, they
            // get deleted.
            var attemptsToDelete = attempts;

            if ( mostRecentSuccess != null )
            {
                attemptsToDelete = attemptsToDelete
                    .Where( saa => saa.AchievementAttemptStartDateTime > mostRecentSuccess.AchievementAttemptStartDateTime )
                    .ToList();
            }

            var newAttempts = CreateNewAttempts( achievementTypeCache, interaction, mostRecentSuccess );

            if ( newAttempts != null && newAttempts.Any() )
            {
                newAttempts = newAttempts.OrderBy( saa => saa.AchievementAttemptStartDateTime ).ToList();

                foreach ( var newAttempt in newAttempts )
                {
                    // Keep the old attempt if possible, otherwise add a new one
                    var existingAttempt = attemptsToDelete.FirstOrDefault( saa => saa.AchievementAttemptStartDateTime == newAttempt.AchievementAttemptStartDateTime );

                    if ( existingAttempt != null )
                    {
                        attemptsToDelete.Remove( existingAttempt );
                        CopyAttempt( newAttempt, existingAttempt );
                        updatedAttempts.Add( existingAttempt );
                    }
                    else
                    {
                        newAttempt.AchieverEntityId = interaction.PersonAliasId.Value;
                        newAttempt.AchievementTypeId = achievementTypeCache.Id;
                        achievementAttemptService.Add( newAttempt );
                        updatedAttempts.Add( newAttempt );
                    }

                    // If this attempt was successful then make re-check the max success limit
                    if ( newAttempt.IsSuccessful )
                    {
                        successfulAttemptCount++;

                        if ( successfulAttemptCount >= maxSuccessesAllowed &&
                            !overachievementPossible )
                        {
                            break;
                        }
                    }
                }
            }

            if ( attemptsToDelete.Any() )
            {
                updatedAttempts.RemoveAll( attemptsToDelete );
                achievementAttemptService.DeleteRange( attemptsToDelete );
            }

            return updatedAttempts;
        }

        /// <summary>
        /// Gets the name of the source that these achievements are measured from.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        public override string GetSourceName( AchievementTypeCache achievementTypeCache )
        {
            var channel = GetInteractionChannelCache( achievementTypeCache );
            var component = GetInteractionComponentCache( achievementTypeCache );

            if ( channel == null && component == null )
            {
                return "Any Interaction";
            }

            if ( component == null )
            {
                return channel.Name;
            }

            if ( channel == null )
            {
                return component.Name;
            }

            return $"{channel.Name}: {component.Name}";
        }

        /// <summary>
        /// Update the open attempt record if there are changes.
        /// </summary>
        /// <param name="openAttempt"></param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="interaction">The interaction.</param>
        private void UpdateOpenAttempt( AchievementAttempt openAttempt, AchievementTypeCache achievementTypeCache, Interaction interaction )
        {
            // Validate the attribute values
            var numberToAccumulate = GetAttributeValue( achievementTypeCache, AttributeKey.NumberToAccumulate ).AsInteger();

            if ( numberToAccumulate <= 0 )
            {
                ExceptionLogService.LogException( $"{GetType().Name}.UpdateOpenAttempt cannot process because the NumberToAccumulate attribute is less than 1" );
                return;
            }

            // Calculate the date range where the open attempt can be validly fulfilled
            var attributeMaxDate = GetAttributeValue( achievementTypeCache, AttributeKey.EndDateTime ).AsDateTime();
            var minDate = openAttempt.AchievementAttemptStartDateTime;
            var maxDate = CalculateMaxDateForAchievementAttempt( minDate, attributeMaxDate );

            // Get the interaction dates
            var interactionDates = GetInteractionDatesByPerson( achievementTypeCache, interaction.PersonAliasId.Value, minDate, maxDate );
            var newCount = interactionDates.Count();
            var lastInteractionDate = interactionDates.LastOrDefault();
            var progress = CalculateProgress( newCount, numberToAccumulate );
            var isSuccessful = progress >= 1m;

            openAttempt.AchievementAttemptEndDateTime = lastInteractionDate;
            openAttempt.Progress = progress;
            openAttempt.IsClosed = isSuccessful && !achievementTypeCache.AllowOverAchievement;
            openAttempt.IsSuccessful = isSuccessful;
        }

        /// <summary>
        /// Create new attempt records and return them in a list. All new attempts should be after the most recent successful attempt.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="interaction">The interaction.</param>
        /// <param name="mostRecentSuccess">The most recent successful attempt.</param>
        /// <returns></returns>
        private List<AchievementAttempt> CreateNewAttempts( AchievementTypeCache achievementTypeCache, Interaction interaction, AchievementAttempt mostRecentSuccess )
        {
            // Validate the attribute values
            var numberToAccumulate = GetAttributeValue( achievementTypeCache, AttributeKey.NumberToAccumulate ).AsInteger();

            if ( numberToAccumulate <= 0 )
            {
                ExceptionLogService.LogException( $"{GetType().Name}.CreateNewAttempts cannot process because the NumberToAccumulate attribute is less than 1" );
                return null;
            }

            // Calculate the date range where new achievements can be validly found
            var attributeMinDate = GetAttributeValue( achievementTypeCache, AttributeKey.StartDateTime ).AsDateTime();
            var attributeMaxDate = GetAttributeValue( achievementTypeCache, AttributeKey.EndDateTime ).AsDateTime();
            var minDate = CalculateMinDateForAchievementAttempt( DateTime.MinValue, mostRecentSuccess, attributeMinDate, numberToAccumulate );
            var maxDate = CalculateMaxDateForAchievementAttempt( minDate, attributeMaxDate );

            // Track the attempts in a list that will be returned
            var attempts = new List<AchievementAttempt>();
            ComputedStreak accumulation = null;

            // Get the interaction dates and begin calculating attempts
            var interactionDates = GetInteractionDatesByPerson( achievementTypeCache, interaction.PersonAliasId.Value, minDate, maxDate );

            foreach ( var interactionDate in interactionDates )
            {
                if ( accumulation == null )
                {
                    accumulation = new ComputedStreak( interactionDate );
                }

                // Increment the accumulation
                accumulation.Count++;
                accumulation.EndDate = interactionDate;

                // Check for a fulfilled attempt
                if ( accumulation.Count >= numberToAccumulate )
                {
                    attempts.Add( GetAttempt( accumulation, numberToAccumulate, true ) );

                    if ( !achievementTypeCache.AllowOverAchievement )
                    {
                        accumulation = null;
                    }
                }
            }

            // The leftover accumulation is an open attempt
            if ( accumulation != null )
            {
                var openAttempt = GetAttempt( accumulation, numberToAccumulate, false );
                var lastAttempt = attempts.LastOrDefault();

                if ( null == lastAttempt ||
                     openAttempt.Progress != lastAttempt.Progress ||
                     openAttempt.AchievementAttemptStartDateTime != lastAttempt.AchievementAttemptStartDateTime ||
                     openAttempt.AchievementAttemptEndDateTime != lastAttempt.AchievementAttemptEndDateTime )
                {
                    attempts.Add( openAttempt );
                }
            }

            return attempts;
        }

        /// <inheritdoc/>
        protected internal override int? GetTargetCount( AchievementType achievementType )
        {
            return achievementType.GetAttributeValue( AttributeKey.NumberToAccumulate ).AsIntegerOrNull();
        }

        #region Helpers

        /// <summary>
        /// Gets the interaction channel unique identifier.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private Guid? GetInteractionChannelGuid( AchievementTypeCache achievementTypeCache )
        {
            var delimited = GetAttributeValue( achievementTypeCache, AttributeKey.InteractionChannelComponent );
            var guids = delimited.SplitDelimitedValues().AsGuidOrNullList();
            return guids.FirstOrDefault();
        }

        /// <summary>
        /// Gets the interaction component unique identifier.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private Guid? GetInteractionComponentGuid( AchievementTypeCache achievementTypeCache )
        {
            var delimited = GetAttributeValue( achievementTypeCache, AttributeKey.InteractionChannelComponent );
            var guids = delimited.SplitDelimitedValues().AsGuidOrNullList();
            return guids.Count == 2 ? guids[1] : null;
        }

        /// <summary>
        /// Gets the interaction channel cache.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private InteractionChannelCache GetInteractionChannelCache(AchievementTypeCache achievementTypeCache)
        {
            var guid = GetInteractionChannelGuid( achievementTypeCache );
            return guid.HasValue ? InteractionChannelCache.Get( guid.Value ) : null;
        }

        /// <summary>
        /// Gets the interaction component cache.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private InteractionComponentCache GetInteractionComponentCache( AchievementTypeCache achievementTypeCache )
        {
            var guid = GetInteractionComponentGuid( achievementTypeCache );
            return guid.HasValue ? InteractionComponentCache.Get( guid.Value ) : null;
        }

        /// <summary>
        /// Gets the interaction dates.
        /// </summary>
        /// <param name="achievementTypeCache">The achievementTypeCache.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <returns></returns>
        private List<DateTime> GetInteractionDatesByPerson( AchievementTypeCache achievementTypeCache, int personAliasId, DateTime minDate, DateTime maxDate )
        {
            var rockContext = new RockContext();
            var query = GetSourceEntitiesQuery( achievementTypeCache, rockContext ) as IQueryable<Interaction>;
            var dayAfterMaxDate = maxDate.AddDays( 1 );

            var personAliasService = new PersonAliasService( rockContext );
            var personAliasQuery = personAliasService
                .Queryable()
                .AsNoTracking()
                .Where( pa => pa.Id == personAliasId )
                .SelectMany( pa => pa.Person.Aliases )
                .Select( pa => pa.Id );

            return query
                .AsNoTracking()
                .Where( i =>
                    i.PersonAliasId.HasValue &&
                    personAliasQuery.Contains(i.PersonAliasId.Value) &&
                    i.InteractionDateTime >= minDate &&
                    i.InteractionDateTime < dayAfterMaxDate )
                .Select( i => i.InteractionDateTime )
                .ToList()
                .OrderBy( d => d )
                .ToList();
        }

        /// <summary>
        /// Gets the attempt from the accumulation
        /// </summary>
        /// <param name="accumulation">The accumulation.</param>
        /// <param name="targetCount">The target count.</param>
        /// <param name="isClosed">if set to <c>true</c> [is closed].</param>
        /// <returns></returns>
        private static AchievementAttempt GetAttempt( ComputedStreak accumulation, int targetCount, bool isClosed )
        {
            var progress = CalculateProgress( accumulation.Count, targetCount );

            return new AchievementAttempt
            {
                AchievementAttemptStartDateTime = accumulation.StartDate,
                AchievementAttemptEndDateTime = accumulation.EndDate,
                Progress = progress,
                IsClosed = isClosed,
                IsSuccessful = progress >= 1m
            };
        }

        #endregion Helpers
    }
}