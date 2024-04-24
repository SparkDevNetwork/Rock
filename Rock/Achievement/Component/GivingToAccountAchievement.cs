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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Achievement.Component
{
    /// <summary>
    /// Use to track achievements earned by giving to an account.
    /// </summary>
    /// <seealso cref="AchievementComponent" />
    [Description( "Used to track achievements earned by giving to selected accounts." )]
    [Export( typeof( AchievementComponent ) )]
    [ExportMetadata( "ComponentName", "Giving: Giving to Account" )]

    [AccountField(
        "Account",
        Description = "The financial account from which the giving achievement is earned.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.FinancialAccount )]

    [BooleanField(
        "Include Child Accounts",
        Description = "Determines whether to include child accounts in this achievement.",
        IsRequired = false,
        Order = 1,
        DefaultBooleanValue = false,
        Key = AttributeKey.IncludeChildFinancialAccounts,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox )]

    [IntegerField(
        "Number to Accumulate",
        Description = "The number of giving transactions required to earn this achievement.",
        IsRequired = true,
        Order = 2,
        DefaultIntegerValue = 1,
        Key = AttributeKey.NumberToAccumulate )]

    [DateField(
        "Start Date",
        Description = "The date that defines when the giving must be completed on or after.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.StartDateTime )]

    [DateField(
        "End Date",
        Description = "The date that defines when the giving must be completed on or before.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.EndDateTime )]

    [Rock.SystemGuid.EntityTypeGuid( "5F8C7ECE-618D-429C-B459-782031F3C1C3")]
    public class GivingToAccountAchievement : AchievementComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementComponent" /> class.
        /// </summary>
        public GivingToAccountAchievement() : base(
            new AchievementConfiguration( typeof( FinancialTransaction ), typeof( PersonAlias ) ),
            new HashSet<string> { AttributeKey.GivingToAccountAchievement } )
        {
        }

        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The Start Date Time
            /// </summary>
            public const string StartDateTime = "StartDateTime";

            /// <summary>
            /// The End Date Time
            /// </summary>
            public const string EndDateTime = "EndDateTime";

            /// <summary>
            /// The financial account
            /// </summary>
            public const string FinancialAccount = "FinancialAccount";

            /// <summary>
            /// Whether to include child financial accounts
            /// </summary>
            public const string IncludeChildFinancialAccounts = "IncludeChildFinancialAccounts";

            /// <summary>
            /// The giving to account achievement type
            /// </summary>
            public const string GivingToAccountAchievement = "GivingToAccountAchievement";

            /// <summary>
            /// The number to accumulate
            /// </summary>
            public const string NumberToAccumulate = "NumberToAccumulate";
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
            if ( !( sourceEntity is FinancialTransaction financialTransaction ) )
            {
                return false;
            }

            var accountId = GetFinancialAccountId( achievementTypeCache );
            var details = financialTransaction.TransactionDetails;
            if ( details == null || !accountId.HasValue )
            {
                return false;
            }

            List<int> financialAccountIds = new List<int>
            {
                accountId.Value
            };

            var includeChildAccounts = GetAttributeValue( achievementTypeCache, AttributeKey.IncludeChildFinancialAccounts ).AsBoolean();
            if ( includeChildAccounts )
            {
                financialAccountIds.AddRange( FinancialAccountCache.Get( accountId.Value ).GetDescendentFinancialAccountIds() );
            }

            return details.Any( t => financialAccountIds.Contains( t.AccountId ) );
        }

        /// <summary>
        /// Gets the source entities query. This is the set of source entities that should be passed to the process method
        /// when processing this achievement type.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public override IQueryable<IEntity> GetSourceEntitiesQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext )
        {
            return GetFinancialTransactionQuery( achievementTypeCache, rockContext );
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
                if ( f.Key.Equals( "AccountId", StringComparison.OrdinalIgnoreCase ) )
                {
                    return f.Value.AsInteger() == GetFinancialAccount( achievementTypeCache )?.Id;
                }

                if ( f.Key.Equals( "FinancialAccountId", StringComparison.OrdinalIgnoreCase ) )
                {
                    return f.Value.AsInteger() == GetFinancialAccount( achievementTypeCache )?.Id;
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
            var updatedAttempts = new HashSet<AchievementAttempt>();

            // If we cannot link the transaction to a person, then there is nothing to do
            if ( !( sourceEntity is FinancialTransaction financialTransaction ) )
            {
                return updatedAttempts;
            }

            // If the achievement type is not active (or null) OR if there is no person associated to the financial transaction
            // then there is nothing to do
            if ( achievementTypeCache?.IsActive != true || financialTransaction.AuthorizedPersonAliasId == null )
            {
                return updatedAttempts;
            }

            // If there are unmet prerequisites, then there is nothing to do
            var achievementTypeService = new AchievementTypeService( rockContext );
            var unmetPrerequisites = achievementTypeService.GetUnmetPrerequisites( achievementTypeCache.Id, financialTransaction.AuthorizedPersonAliasId.Value );

            if ( unmetPrerequisites.Any() )
            {
                return updatedAttempts;
            }

            // If the transaction is a refund, the person is empty, or less than zero amount, then there is nothing to do.
            if ( null != financialTransaction.RefundDetails ||
                 !financialTransaction.AuthorizedPersonAliasId.HasValue ||
                 financialTransaction.AuthorizedPersonAliasId == 0 ||
                 financialTransaction.TotalAmount <= 0M )
            {
                return updatedAttempts;
            }

            // Get all of the attempts for this interaction and achievement combo, ordered by start date DESC so that
            // the most recent attempts can be found with FirstOrDefault
            var achievementAttemptService = new AchievementAttemptService( rockContext );
            var attempts = achievementAttemptService.GetOrderedAchieverAttempts( achievementAttemptService.Queryable(), achievementTypeCache, financialTransaction.AuthorizedPersonAliasId.Value );

            var mostRecentSuccess = attempts.FirstOrDefault( saa => saa.AchievementAttemptEndDateTime.HasValue && saa.IsSuccessful );
            var overachievementPossible = achievementTypeCache.AllowOverAchievement;
            var successfulAttemptCount = attempts.Count( saa => saa.IsSuccessful );
            var maxSuccessesAllowed = achievementTypeCache.MaxAccomplishmentsAllowed ?? int.MaxValue;

            // If the most recent success is still open and overachievement is allowed, then update it
            if ( overachievementPossible && mostRecentSuccess != null && !mostRecentSuccess.IsClosed )
            {
                UpdateOpenAttempt( mostRecentSuccess, achievementTypeCache, financialTransaction );
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

            var newAttempts = CreateNewAttempts( achievementTypeCache, financialTransaction, mostRecentSuccess );

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
                        newAttempt.AchieverEntityId = financialTransaction.AuthorizedPersonAliasId.Value;
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
            return GetFinancialAccountName( achievementTypeCache );
        }

        /// <inheritdoc/>
        protected internal override int? GetTargetCount( AchievementType achievementType )
        {
            return achievementType.GetAttributeValue( AttributeKey.NumberToAccumulate ).AsIntegerOrNull();
        }

        #region Helpers

        /// <summary>
        /// Update the open attempt record if there are changes.
        /// </summary>
        /// <param name="openAttempt"></param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="transaction">The financial transaction.</param>
        private void UpdateOpenAttempt( AchievementAttempt openAttempt, AchievementTypeCache achievementTypeCache, FinancialTransaction transaction )
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

            // Get the transaction dates
            var transactionDates = GetOrderedFinancialTransactionDatesByPerson( achievementTypeCache, transaction.AuthorizedPersonAlias.PersonId, minDate, maxDate );
            var newCount = transactionDates.Count();

            if ( newCount == 0 )
            {
                return;
            }

            var lastInteractionDate = transactionDates.LastOrDefault();
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
        /// <param name="transaction">The financial transaction.</param>
        /// <param name="mostRecentSuccess">The most recent successful attempt.</param>
        /// <returns></returns>
        private List<AchievementAttempt> CreateNewAttempts( AchievementTypeCache achievementTypeCache, FinancialTransaction transaction, AchievementAttempt mostRecentSuccess )
        {
            // Validate the attribute values
            var numberToAccumulate = GetAttributeValue( achievementTypeCache, AttributeKey.NumberToAccumulate ).AsInteger();

            if ( numberToAccumulate <= 0 )
            {
                ExceptionLogService.LogException( $"{GetType().Name}. CreateNewAttempts cannot process because the NumberToAccumulate attribute is less than 1" );
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

            // Get the transaction dates and begin calculating attempts
            if ( !transaction.AuthorizedPersonAliasId.HasValue )
            {
                ExceptionLogService.LogException( $"{GetType().Name}. CreateNewAttempts cannot process because the transaction does not have an AuthorizedPersonAliasId." );
                return null;
            }

            var personId = new PersonAliasService( new RockContext() ).Get( transaction.AuthorizedPersonAliasId.Value ).PersonId;
            var transactionDates = GetOrderedFinancialTransactionDatesByPerson( achievementTypeCache, personId, minDate, maxDate );

            foreach ( var transactionDate in transactionDates )
            {
                if ( !transactionDate.HasValue )
                {
                    // Nothing we can do without a date
                    continue;
                }

                if ( accumulation == null )
                {
                    accumulation = new ComputedStreak( transactionDate.Value );
                }

                // Increment the accumulation
                accumulation.Count++;
                accumulation.EndDate = transactionDate;

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

        /// <summary>
        /// Gets the financial account unique identifier.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private Guid? GetFinancialAccountGuid( AchievementTypeCache achievementTypeCache )
        {
            var delimited = GetAttributeValue( achievementTypeCache, AttributeKey.FinancialAccount );
            var guids = delimited.SplitDelimitedValues().AsGuidOrNullList();
            return guids.FirstOrDefault();
        }

        /// <summary>
        /// Gets the financial account.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private FinancialAccountCache GetFinancialAccount( AchievementTypeCache achievementTypeCache, RockContext rockContext = null )
        {
            if ( null == rockContext )
            {
                rockContext = new RockContext();
            }

            var guid = GetFinancialAccountGuid( achievementTypeCache );

            if ( !guid.HasValue )
            {
                return null;
            }

            return FinancialAccountCache.Get( guid.Value );
        }

        /// <summary>
        /// Gets the financial account identifier.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private int? GetFinancialAccountId( AchievementTypeCache achievementTypeCache, RockContext rockContext = null )
        {
            if ( null == rockContext )
            {
                rockContext = new RockContext();
            }

            var guid = GetFinancialAccountGuid( achievementTypeCache );

            if ( !guid.HasValue )
            {
                return null;
            }


            return FinancialAccountCache.GetId( guid.Value );
        }

        /// <summary>
        /// Gets the financial account name.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private string GetFinancialAccountName( AchievementTypeCache achievementTypeCache, RockContext rockContext = null )
        {
            if ( null == rockContext )
            {
                rockContext = new RockContext();
            }

            var guid = GetFinancialAccountGuid( achievementTypeCache );

            if ( !guid.HasValue )
            {
                return null;
            }

            return FinancialAccountCache.Get( guid.Value )?.Name;
        }

        /// <summary>
        /// Gets the financial transactions query.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<FinancialTransaction> GetFinancialTransactionQuery( AchievementTypeCache achievementTypeCache, RockContext rockContext = null )
        {
            if ( null == rockContext )
            {
                rockContext = new RockContext();
            }

            var includeChildAccounts = GetAttributeValue( achievementTypeCache, AttributeKey.IncludeChildFinancialAccounts ).AsBoolean();
            var attributeMinDate = GetAttributeValue( achievementTypeCache, AttributeKey.StartDateTime ).AsDateTime();
            var attributeMaxDate = GetAttributeValue( achievementTypeCache, AttributeKey.EndDateTime ).AsDateTime();

            // Get the account identifier for this achievement type
            var accountId = GetFinancialAccountId( achievementTypeCache, rockContext );
            if ( !accountId.HasValue )
            {
                return null;
            }

            // For the include child accounts option to work we need to get the descendents for this Type
            var accountDescendentIds = includeChildAccounts ? FinancialAccountCache.Get( accountId.Value ).GetDescendentFinancialAccounts().Select( a => a.Id ).ToList() : new List<int>();

            var transactionService = new FinancialTransactionService( rockContext );

            var query = transactionService.Queryable()
                .AsNoTracking()
                .Where( t =>
                    t.AuthorizedPersonAliasId.HasValue &&
                    t.RefundDetails == null &&
                    t.TransactionDetails.Any( a =>
                        a.Amount > 0 &&
                        ( a.AccountId == accountId ||
                        accountDescendentIds.Contains( a.AccountId ) ) ) );

            if ( attributeMinDate.HasValue )
            {
                query = query.Where( t => t.TransactionDateTime >= attributeMinDate.Value );
            }

            if ( attributeMaxDate.HasValue )
            {
                var dayAfterMaxDate = attributeMaxDate.Value.AddDays( 1 );
                query = query.Where( t => t.TransactionDateTime < dayAfterMaxDate );
            }

            return query;
        }

        /// <summary>
        /// Gets the transaction dates for the specified person.
        /// </summary>
        /// <param name="achievementTypeCache">The achievementTypeCache.</param>
        /// <param name="personId">The person alias identifier.</param>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <returns></returns>
        private List<DateTime?> GetOrderedFinancialTransactionDatesByPerson( AchievementTypeCache achievementTypeCache, int personId, DateTime minDate, DateTime maxDate )
        {
            var rockContext = new RockContext();
            var query = GetSourceEntitiesQuery( achievementTypeCache, rockContext ) as IQueryable<FinancialTransaction>;
            var dayAfterMaxDate = maxDate.AddDays( 1 );

            // Get the Person's Giving ID
            var givingId = new PersonService( rockContext ).GetSelect( personId, p => p.GivingId );

            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
            var personAliasQuery = new PersonAliasService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( pa => pa.Person.GivingId == givingId )
                .Select( pa => pa.Id );

            return query
                .AsNoTracking()
                .Where( i =>
                    i.AuthorizedPersonAliasId.HasValue &&
                    personAliasQuery.Contains( i.AuthorizedPersonAliasId.Value ) &&
                    i.TransactionDateTime >= minDate &&
                    i.TransactionDateTime < dayAfterMaxDate )
                .Select( i => i.TransactionDateTime )
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

        #endregion
    }
}
