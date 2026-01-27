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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that updates giving classifications, journey stages, and giving alerts.
    /// </summary>
    /*
        Giving Automation Job (refactored 01/2026)

        Summary:
            - Updates a total of 17 person attributes as part of Giving Automation.
            - Sends giving alerts based on recent transactions and late/expected gifts.

        Responsibilities:
            1) Update giving classification attributes (12 + 2) for giving units (GivingId).
               - Calculates per-giving-unit values and persists them to all people that share the same GivingId.
               - Statistical attributes are only updated when there is sufficient transaction history, and only
                 for GivingIds who have new transactions since the last classification job run.
               - Note: Bin and Percentile attributes are now updated through a centralized stored procedure,
                 with a full explanation outlining this change provided in the procedure’s comments.

            2) Update Giving Journey Stage attributes (3).
               - Refactored out of a per-GivingId loop and updated efficiently (via stored procedure),
                 since journey stages are evaluated for the full population of giving units.

            3) Generate Giving Alerts.
               - Processes "recent transaction" alerts for giving units with qualifying transactions in the last week.
               - Processes "late transaction" alerts (expected gifts that did not occur) for configured follow-up alert types.
               - Repeat prevention (global + per-alert-type) is respected to avoid generating excessive alerts.
    */
    [DisplayName( "Giving Automation" )]
    [Description( "Job that updates giving classifications and journey stages, and send any giving alerts." )]

    #region Block Attributes

    [IntegerField( "Max Days Since Last Gift for Alerts",
        Description = "The maximum number of days since a giving group last gave where alerts can be made. If the last gift was earlier than this maximum, then alerts are not relevant.",
        DefaultIntegerValue = AttributeDefaultValue.MaxDaysSinceLastGift,
        Key = AttributeKey.MaxDaysSinceLastGift,
        Order = 1 )]
    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout,
        Category = "General",
        Order = 7 )]

    #endregion Block Attributes

    public class GivingAutomation : RockJob
    {
        #region Keys

        private static class AttributeKey
        {
            public const string MaxDaysSinceLastGift = "MaxDaysSinceLastGift";
            public const string CommandTimeout = "CommandTimeout";
        }

        private static class AttributeDefaultValue
        {
            public const int MaxDaysSinceLastGift = 548;
            public const int CommandTimeout = 180;
        }

        private static class Icons
        {
            public const string SUCCESS_ICON = "<i class='ti ti-circle-filled text-success'></i> ";
            public const string ERROR_ICON = "<i class='ti ti-circle-filled text-danger'></i> ";
        }

        #endregion Keys

        #region Execute

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var context = new GivingAutomationContext
            {
                DebugModeEnabled = false,
            };

            if ( context.Settings?.GivingAutomationJobSettings?.IsEnabled != true )
            {
                this.UpdateLastStatusMessage( "Giving Automation is not enabled." );
                return;
            }

            try
            {
                if ( context.DebugModeEnabled )
                {
                    DebugHelper.Enable();
                }

                var parallelOptions = new ParallelOptions
                {
                    // Set to half of the Processor Count. This seems to be the sweet spot for best performance without overwhelming the machine and slowing down IIS.
                    MaxDegreeOfParallelism = Environment.ProcessorCount > 4 ? Environment.ProcessorCount / 2 : 1
                };

                context.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaultValue.CommandTimeout;

                var fallback = context.Now.AddDays( -( GetAttributeValue( AttributeKey.MaxDaysSinceLastGift ).AsIntegerOrNull() ?? AttributeDefaultValue.MaxDaysSinceLastGift ) );
                var lastClassificationRunDateTime = context.Settings.GivingClassificationSettings.LastRunDateTime ?? fallback;
                var today = context.Now.DayOfWeek;

                // Determine whether to update attribute values for classification attributes today.
                var givingClassificationSettings = context.Settings?.GivingClassificationSettings;
                var isClassificationUpdateDay = givingClassificationSettings?.RunDays == null || givingClassificationSettings.RunDays.Contains( today );
                var isJourneyUpdateDay = context.Settings.GivingJourneySettings?.DaysToUpdateGivingJourneys?.Contains( today ) == true;

                /*
                     1/11/2026 - MSE

                     When the transaction-related filters on the GivingAutomationConfiguration block change
                     (Transaction Types, Accounts, or Include Child Accounts), any previously computed
                     classification attribute values are considered invalid for all people.

                     To prevent retaining invalid data, all Giving Ids are reprocessed using the updated
                     transaction filter criteria. Without this reset, people would retain the values
                     calculated using the old filters, while only new givers would receive values based on
                     the updated filters, resulting in inconsistent data.

                     We would be comparing apples to oranges...

                     There are warnings set in the UI notifying administrators of this behavior.

                     Reason: Changes to which transactions qualify for this job invalidate all previously
                     computed classification attribute values.
                */
                var filtersChanged = context.Settings?.GivingClassificationSettings?.FiltersChanged == true;
                if ( filtersChanged )
                {
                    isClassificationUpdateDay = true;
                    isJourneyUpdateDay = true;
                }

                // Identify GivingIds with new or modified transactions since the last time classifications were run
                var dirtyGivingIds = GetDirtyGivingIds( context, lastClassificationRunDateTime, filtersChanged );
                var dirtyGivingIdsCount = dirtyGivingIds.Count;

                // Get the alert types that are scheduled to run today.
                context.AlertTypes = GetAlertTypes( context );
                var hasAlertTypes = context.AlertTypes.Any();

                // Determine if there are any alert types that don't require sensitivity
                // These can be processed even for givers without enough transaction history over the past year.
                bool hasAlertTypesWithoutSensitivity = hasAlertTypes && context.AlertTypes.Any( a => !a.FrequencySensitivityScale.HasValue && !a.AmountSensitivityScale.HasValue );

                if ( isClassificationUpdateDay )
                {
                    /*
                         1/11/2026 - MSE

                         Giving bins and percentiles are recalculated for all people, not just those with
                         new giving activity.

                         Percentiles are relative to the entire giving distribution. When any person’s
                         giving changes, the distribution shifts, which affects the percentile placement
                         of everyone else. Updating only people with new transactions would allow others to
                         retain stale and inflated values.

                         Reason: Percentile and bin values must be recalculated globally to remain accurate
                         as the overall giving amount distribution changes.
                    */
                    this.UpdateLastStatusMessage( "Updating Giving Bins and Percentiles..." );
                    try
                    {
                        GivingAutomationHelper.UpdateGivingBinsAndPercentiles();
                    }
                    catch ( Exception ex )
                    {
                        AddError( context, "Error updating giving bins and percentiles.", ex );
                    }
                }

                int? numberOfJourneysChanged = null;
                if ( isJourneyUpdateDay )
                {
                    this.UpdateLastStatusMessage( "Updating Giving Journey Stages..." );
                    try
                    {
                        numberOfJourneysChanged = GivingAutomationHelper.UpdateJourneyStages( context.Settings );
                    }
                    catch ( Exception ex )
                    {
                        AddError( context, "Error updating giving journey stages.", ex );
                    }
                }

                if ( dirtyGivingIdsCount > 0 )
                {
                    int batchSize = dirtyGivingIdsCount > 500 ? 500 : dirtyGivingIdsCount;

                    for ( int i = 0; i < dirtyGivingIdsCount; i += batchSize )
                    {
                        int currentBatchSize = Math.Min( batchSize, dirtyGivingIdsCount - i );
                        var batchIds = dirtyGivingIds.GetRange( i, currentBatchSize );

                        this.UpdateLastStatusMessage( $"Processing Giving Classifications and Alerts...{( dirtyGivingIdsCount > 500 ? $" {i + currentBatchSize}/{dirtyGivingIdsCount}" : "" )}" );
                        var batchSw = Stopwatch.StartNew();

                        var twelveMonthsTransactionsByGivingId = GetTransactionsForBatch( context, batchIds );
                        var twelveMonthsAlertsByGivingId = GetAlertsForBatch( context, batchIds );
                        var eligibleGivingIdsByDataViewId = GetEligibleGivingIdsByDataViewIdForBatch( context, batchIds );
                        var personsByGivingId = GetPersonsForBatch( context, batchIds );

                        var batchAttributeValueUpdates = new ConcurrentDictionary<int, List<AttributeValueUpdate>>();

                        Parallel.ForEach( batchIds, parallelOptions, givingId =>
                        {
                            Stopwatch threadSw = null;
                            if ( context.DebugModeEnabled )
                            {
                                threadSw = Stopwatch.StartNew();
                            }

                            try
                            {
                                var twelveMonthsTransactions = twelveMonthsTransactionsByGivingId.GetValueOrNull( givingId ) ?? new List<FinancialTransactionView>();
                                var twelveMonthsAlerts = twelveMonthsAlertsByGivingId.GetValueOrNull( givingId ) ?? new List<AlertView>();

                                // If there are no transactions in the last 12 months, there is no basis to classify or alert.
                                if ( !twelveMonthsTransactions.Any() )
                                {
                                    // If the transaction filters have changed and therefore this giving unit doesn't have
                                    // any qualifying transactions in the last 12 months, clear their invalidated attributes.
                                    if ( filtersChanged && isClassificationUpdateDay )
                                    {
                                        var personsForGivingId = personsByGivingId.GetValueOrNull( givingId ) ?? new List<Person>();
                                        var attributeValueUpdates = CreateClearedAttributeValueUpdates( context.Now );

                                        if ( personsForGivingId.Any() )
                                        {
                                            foreach ( var person in personsForGivingId )
                                            {
                                                batchAttributeValueUpdates.TryAdd( person.Id, attributeValueUpdates );
                                            }
                                        }
                                    }

                                    return;
                                }

                                var persons = personsByGivingId.GetValueOrNull( givingId );
                                if ( persons == null || !persons.Any() )
                                {
                                    return;
                                }

                                var firstGiftDate = GetGivingUnitAttributeValue( persons, SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE ).AsDateTime();
                                var lastClassifiedDate = GetGivingUnitAttributeValue( persons, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime() ?? DateTime.MinValue;

                                // If first gift date is missing, try to determine it.
                                if ( !firstGiftDate.HasValue )
                                {
                                    using ( var rockContext = new RockContext() )
                                    {
                                        firstGiftDate = new FinancialTransactionService( rockContext )
                                            .GetGivingAutomationSourceTransactionQueryByGivingId( givingId )
                                            .Where( t => t.TransactionDateTime.HasValue )
                                            .Min( t => t.TransactionDateTime );

                                        // If first gift date was missing but now identified, persist it.
                                        if ( firstGiftDate.HasValue )
                                        {
                                            var attributeValueUpdates = new List<AttributeValueUpdate>
                                            {
                                                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE.AsGuid(), firstGiftDate )
                                            };

                                            foreach ( var person in persons )
                                            {
                                                var updates = batchAttributeValueUpdates.GetOrAdd( person.Id, _ => new List<AttributeValueUpdate>( 16 ) );
                                                updates.AddRange( attributeValueUpdates );
                                            }
                                        }
                                    }
                                }

                                // If the first gift date was less than 12 months ago and there are less than 5 gifts,
                                // there is not enough data to develop any meaningful insights.
                                var hasEnoughDataForClassification =
                                    !( ( !firstGiftDate.HasValue || firstGiftDate.Value > context.OneYearAgo )
                                        && twelveMonthsTransactions.Count < context.MinimumTransactionCountForClassifications );

                                // ========================================
                                // CALCULATE GIVING ATTRIBUTE VALUES
                                // ========================================
                                if ( isClassificationUpdateDay )
                                {
                                    var attributeValueUpdates = CalculateAttributesPerGivingId( context, twelveMonthsTransactions, hasEnoughDataForClassification );

                                    foreach ( var person in persons )
                                    {
                                        var updates = batchAttributeValueUpdates.GetOrAdd( person.Id, _ => new List<AttributeValueUpdate>( 16 ) );
                                        updates.AddRange( attributeValueUpdates );
                                    }
                                }

                                // ========================================
                                // GENERATE RECENT GIFT ALERTS
                                // ========================================
                                if ( hasAlertTypes && ( hasEnoughDataForClassification || hasAlertTypesWithoutSensitivity ) )
                                {
                                    if ( !hasAlertTypesWithoutSensitivity )
                                    {
                                        if ( lastClassifiedDate < context.OneWeekAgo )
                                        {
                                            return;
                                        }
                                    }

                                    ProcessRecentTxnAlertsPerGivingId(
                                        context,
                                        givingId,
                                        twelveMonthsTransactions,
                                        twelveMonthsAlerts,
                                        eligibleGivingIdsByDataViewId );
                                }
                            }
                            catch ( Exception ex )
                            {
                                Interlocked.Increment( ref context.GivingUnitsFailed );
                                AddError( context, $"Error processing GivingId '{givingId}'.", ex );
                            }
                            finally
                            {
                                Interlocked.Increment( ref context.GivingUnitsProcessed );

                                if ( threadSw != null )
                                {
                                    threadSw.Stop();
                                    DebugHelper.RecordProcessed( threadSw.ElapsedMilliseconds );
                                }
                            }
                        } );

                        batchSw.Stop();
                        if ( context.DebugModeEnabled )
                        {
                            DebugHelper.Write( $"Processed batch {( i / batchSize ) + 1} of {Math.Ceiling( ( double ) dirtyGivingIdsCount / batchSize )} in {batchSw.ElapsedMilliseconds} ms" );
                        }

                        // ========================================
                        // SAVE ATTRIBUTE VALUES
                        // ========================================
                        var batchUpdatesToSave = batchAttributeValueUpdates.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Where( u => u.AttributeId > 0 ).ToList() );

                        if ( batchUpdatesToSave.Any() )
                        {
                            if ( SaveAttributeValuesForBatch( context, batchUpdatesToSave ) )
                            {
                                GivingUnitWasClassifiedMessage.Publish( batchUpdatesToSave.Keys.ToList() );
                            }
                        }
                    }
                }

                // ========================================
                // GENERATE LATE GIFT ALERTS
                // ========================================
                if ( hasAlertTypes )
                {
                    this.UpdateLastStatusMessage( "Processing late gift alerts..." );
                    try
                    {
                        ProcessLateTxnAlerts( context );
                    }
                    catch ( Exception ex )
                    {
                        AddError( context, "Error processing late transaction alerts.", ex );
                    }
                }

                if ( isClassificationUpdateDay )
                {
                    SaveLastClassificationsRunDateTime( context, context.Now );
                }

                if ( filtersChanged && context?.Settings?.GivingClassificationSettings != null )
                {
                    context.Settings.GivingClassificationSettings.FiltersChanged = false;
                    GivingAutomationSettings.SaveGivingAutomationSettings( context.Settings );
                }

                DebugHelper.WriteFinalSummary( dirtyGivingIdsCount, context.GivingUnitsProcessed, context.GivingUnitsFailed, context.Errors.Count );

                var sb = new StringBuilder();
                sb.AppendLine( $"{Icons.SUCCESS_ICON}Processed {dirtyGivingIdsCount} giving units." );
                sb.AppendLine( $"{Icons.SUCCESS_ICON}Created {context.AlertsCreated} alerts." );
                if ( numberOfJourneysChanged.HasValue )
                {
                    sb.AppendLine( $"{Icons.SUCCESS_ICON}Updated {numberOfJourneysChanged.Value} journey stages." );
                }

                if ( context.Errors.Any() )
                {
                    sb.AppendLine();
                    sb.AppendLine( "Errors:" );
                    foreach ( var error in context.Errors )
                    {
                        sb.AppendLine( $"{Icons.ERROR_ICON}{error}" );
                    }
                }

                this.Result = sb.ToString().Trim().ConvertCrLfToHtmlBr();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                AddError( context, "Giving Automation failed with an exception.", ex );
                this.Result = $"{Icons.ERROR_ICON}Giving Automation failed with an exception: {ex.Message}";
                throw;
            }
        }

        #endregion Execute

        #region Methods

        /// <summary>
        /// Gets the list of giving ids that should be processed for this run.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="lastClassificationRunDateTime">The last time classification processing completed successfully.</param>
        /// <param name="filtersChanged">Whether the Giving Automation filters changed, requiring the job to refresh all attributes for all giving units.</param>
        private static List<string> GetDirtyGivingIds( GivingAutomationContext context, DateTime lastClassificationRunDateTime, bool filtersChanged )
        {
            // Classification attributes need to be written for all adults with the same giver id in Rock. So Ted &
            // Cindy should have the same attribute values if they are set to contribute as a family even if Cindy
            // is always the one giving the gift.

            // We will reclassify anyone who has given since the last run of this job. This also covers all alerts except
            // the "late txn" alert, which needs to find people based on the absence of a gift.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                if ( filtersChanged )
                {
                    var personQueryOptions = new PersonService.PersonQueryOptions
                    {
                        IncludeDeceased = true,
                        IncludeBusinesses = true,
                        IncludePersons = true,
                        IncludeNameless = false,
                        IncludeRestUsers = false
                    };

                    return new PersonService( rockContext ).Queryable( personQueryOptions )
                        .Where( p => p.GivingId != null && p.GivingId != string.Empty )
                        .Select( p => p.GivingId )
                        .Distinct()
                        .ToList();
                }

                var query = new FinancialTransactionService( rockContext )
                    .GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        t.AuthorizedPersonAliasId != null &&
                        t.AuthorizedPersonAlias.Person.GivingId != null &&
                        t.AuthorizedPersonAlias.Person.GivingId != string.Empty );

                query = query.Where( t => t.TransactionDateTime >= lastClassificationRunDateTime || t.ModifiedDateTime >= lastClassificationRunDateTime );

                var dirtyGivingIds = query
                    .Select( ft => ft.AuthorizedPersonAlias.Person.GivingId )
                    .Distinct()
                    .ToList();

                return dirtyGivingIds;
            }
        }

        /// <summary>
        /// Gets the people for a batch of GivingIds, with attributes pre-loaded.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="givingIds">The giving ids to query.</param>
        private static Dictionary<string, List<Person>> GetPersonsForBatch( GivingAutomationContext context, List<string> givingIds )
        {
            if ( givingIds == null || givingIds.Count == 0 )
            {
                return new Dictionary<string, List<Person>>();
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                var personQueryOptions = new PersonService.PersonQueryOptions
                {
                    IncludeDeceased = true,
                    IncludeBusinesses = true,
                    IncludePersons = true,
                    IncludeNameless = false,
                    IncludeRestUsers = false
                };

                var persons = new PersonService( rockContext ).Queryable( personQueryOptions )
                    .Where( p => givingIds.Contains( p.GivingId ) )
                    .AsNoTracking()
                    .ToList();

                persons.LoadAttributes( rockContext );

                return persons
                    .GroupBy( p => p.GivingId )
                    .ToDictionary( g => g.Key, g => g.ToList() );
            }
        }

        /// <summary>
        /// Gets the alert types that should be considered today (according to the alert type RunDays).
        /// </summary>
        /// <param name="context">The current job context.</param>
        private static List<FinancialTransactionAlertType> GetAlertTypes( GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                // Get the alert types
                var alertTypeService = new FinancialTransactionAlertTypeService( rockContext );
                var alertTypes = alertTypeService.Queryable()
                    .AsNoTracking()
                    .Include( a => a.FinancialAccount.ChildAccounts )
                    .OrderBy( at => at.Order )
                    .ToList();

                // Filter out alert types that are not supposed to run today
                var currentDayOfWeekFlag = context.Now.DayOfWeek.AsFlags();

                alertTypes = alertTypes
                    .Where( at =>
                        !at.RunDaysOfWeek.HasValue ||
                        ( at.RunDaysOfWeek.Value & currentDayOfWeekFlag ) == currentDayOfWeekFlag )
                    .ToList();

                return alertTypes;
            }
        }

        /// <summary>
        /// Gets transaction data from the last 12 months for a batch of GivingIds.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="givingIds">The giving ids to query.</param>
        private static Dictionary<string, List<FinancialTransactionView>> GetTransactionsForBatch( GivingAutomationContext context, List<string> givingIds )
        {
            if ( givingIds == null || givingIds.Count == 0 )
            {
                return new Dictionary<string, List<FinancialTransactionView>>();
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                var query = new FinancialTransactionService( rockContext ).GetGivingAutomationSourceTransactionQueryByGivingIds( givingIds );
                query = query.Where( t => t.TransactionDateTime >= context.OneYearAgo );

                var transactions = query
                    .Select( t => new FinancialTransactionView
                    {
                        Id = t.Id,
                        AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                        AuthorizedPersonGivingId = t.AuthorizedPersonAlias.Person.GivingId,
                        AuthorizedPersonCampusId = t.AuthorizedPersonAlias.Person.PrimaryCampusId,
                        TransactionDateTime = t.TransactionDateTime.Value,
                        CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                        SourceTypeValueId = t.SourceTypeValueId,
                        IsScheduled = t.ScheduledTransactionId.HasValue,
                        TransactionDetails = t.TransactionDetails.Select( d => new FinancialTransactionDetailView { AccountId = d.AccountId, Amount = d.Amount } ).ToList(),
                        RefundDetails = t.Refunds.SelectMany( r => r.FinancialTransaction.TransactionDetails ).Select( d => new FinancialTransactionDetailView { AccountId = d.AccountId, Amount = d.Amount } ).ToList()
                    } )
                    .ToList();

                return transactions
                    .GroupBy( t => t.AuthorizedPersonGivingId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList() );
            }
        }

        /// <summary>
        /// Gets the last 12 months of alerts for a batch of GivingIds.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="givingIds">The giving ids to query.</param>
        private static Dictionary<string, List<AlertView>> GetAlertsForBatch( GivingAutomationContext context, List<string> givingIds )
        {
            if ( givingIds == null || givingIds.Count == 0 )
            {
                return new Dictionary<string, List<AlertView>>();
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                var alerts = new FinancialTransactionAlertService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.AlertDateTime > context.OneYearAgo )
                    .Where( a => a.PersonAlias != null
                        && a.PersonAlias.Person.GivingId != null
                        && givingIds.Contains( a.PersonAlias.Person.GivingId ) )
                    .Select( a => new
                    {
                        GivingId = a.PersonAlias.Person.GivingId,
                        Alert = new AlertView
                        {
                            AlertTypeId = a.AlertTypeId,
                            AlertDateTime = a.AlertDateTime,
                            AlertType = a.FinancialTransactionAlertType.AlertType,
                            TransactionId = a.TransactionId
                        }
                    } )
                    .ToList();

                return alerts
                    .GroupBy( a => a.GivingId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select( x => x.Alert ).OrderBy( x => x.AlertDateTime ).ToList() );
            }
        }

        /// <summary>
        /// Precomputes DataView eligibility for the provided batch of GivingIds.
        /// Key is DataViewId, value is a set of GivingIds that match the DataView.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="givingIds">The giving ids in the current batch.</param>
        private static Dictionary<int, HashSet<string>> GetEligibleGivingIdsByDataViewIdForBatch( GivingAutomationContext context, List<string> givingIds )
        {
            var result = new Dictionary<int, HashSet<string>>();

            if ( givingIds == null || givingIds.Count == 0 || context.AlertTypes == null )
            {
                return result;
            }

            var dataViewIds = context.AlertTypes
                .Where( at => at.DataViewId.HasValue )
                .Select( at => at.DataViewId.Value )
                .Distinct()
                .ToList();

            if ( !dataViewIds.Any() )
            {
                return result;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                foreach ( var dataViewId in dataViewIds )
                {
                    var dataViewQuery = GetDataViewGivingIdQuery( context, dataViewId, rockContext );
                    if ( dataViewQuery != null )
                    {
                        var eligibleGivingIds = dataViewQuery
                            // Don't have to worry about SQL parameter limits here since we're at most
                            // using batches of 500.
                            .Where( gId => givingIds.Contains( gId ) )
                            .Distinct()
                            .ToList();

                        if ( eligibleGivingIds.Any() )
                        {
                            result[dataViewId] = new HashSet<string>( eligibleGivingIds );
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a query that returns giving ids for people in the specified DataView.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="dataViewId">The DataView id.</param>
        /// <param name="rockContext">The Rock context to use for query generation.</param>
        private static IQueryable<string> GetDataViewGivingIdQuery( GivingAutomationContext context, int dataViewId, RockContext rockContext )
        {
            var dataview = DataViewCache.Get( dataViewId );
            if ( dataview == null )
            {
                AddError( context, $"The dataview {dataViewId} did not resolve." );
                return null;
            }

            if ( rockContext == null )
            {
                return null;
            }

            IQueryable<Person> personQuery;
            try
            {
                var dataViewGetQueryArgs = new Reporting.GetQueryableOptions
                {
                    DbContext = rockContext
                };

                personQuery = dataview.GetQuery( dataViewGetQueryArgs ) as IQueryable<Person>;
            }
            catch ( Exception ex )
            {
                AddError( context, $"Error generating query for dataview {dataViewId}.", ex );
                return null;
            }

            if ( personQuery == null )
            {
                AddError( context, $"Generating a query for dataview {dataViewId} was not successful." );
                return null;
            }

            return personQuery.Select( p => p.GivingId );
        }

        #endregion Methods

        #region Giving Classification Attributes

        /// <summary>
        /// Calculates the 14 Giving Automation attributes for the specified GivingId based on the provided transactions.
        /// </summary>
        private static List<AttributeValueUpdate> CalculateAttributesPerGivingId( GivingAutomationContext context, List<FinancialTransactionView> transactions, bool calculateStatistics )
        {
            if ( transactions == null || transactions.Count == 0 )
            {
                // Should not happen due to prior safeguards.
                return new List<AttributeValueUpdate>();
            }

            var attributeValueUpdates = new List<AttributeValueUpdate>();

            // PERSON_ERA_FIRST_GAVE
            // ^^^ This attribute is already handled within Execute(), since it must consider all transactions
            // (not just the last 12 months) and is used to potentially prevent updating all of these other attributes.

            // PERSON_ERA_LAST_GAVE
            var lastGiftDate = transactions.Max( t => t.TransactionDateTime );
            attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_ERA_LAST_GAVE.AsGuid(), lastGiftDate ) );

            // PERSON_GIVING_PERCENT_SCHEDULED
            var percentScheduled = GivingAutomationHelper.GetPercentScheduled( transactions );
            attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED.AsGuid(), percentScheduled ) );

            // PERSON_GIVING_PREFERRED_SOURCE
            var preferredSourceGuid = GivingAutomationHelper.GetPreferredSourceGuid( transactions );
            attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE.AsGuid(), preferredSourceGuid.ToStringSafe() ) );

            // PERSON_GIVING_PREFERRED_CURRENCY
            var preferredCurrencyGuid = GivingAutomationHelper.GetPreferredCurrencyGuid( transactions );
            attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY.AsGuid(), preferredCurrencyGuid.ToStringSafe() ) );

            // PERSON_GIVING_AMOUNT_MEDIAN
            // PERSON_GIVING_AMOUNT_IQR
            // PERSON_GIVING_FREQUENCY_MEAN_DAYS
            // PERSON_GIVING_FREQUENCY_STD_DEV_DAYS
            // PERSON_GIVING_FREQUENCY_LABEL
            // PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE
            if ( calculateStatistics )
            {
                var orderedTransactionDateTimes = transactions.Select( t => t.TransactionDateTime ).OrderBy( d => d ).ToList();
                var frequencyStats = GivingAutomationHelper.GetFrequencyStats( orderedTransactionDateTimes, context.TransactionWindowDurationHours );
                var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactions.Select( t => t.TotalAmount ) );

                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid(), frequencyStats.MeanDays ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid(), frequencyStats.StdDevDays ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL.AsGuid(), ( int ) frequencyStats.FrequencyLabel ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE.AsGuid(), frequencyStats.NextExpectedGiftDate ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN.AsGuid(), quartileRanges.MedianAmount ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR.AsGuid(), quartileRanges.IQRAmount ) );
            }
            else
            {
                // If we aren't calculating statistics (not enough transactions), we need to explicitly clear them
                // to ensure we don't have stale data remaining on the person record.
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid(), string.Empty ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid(), string.Empty ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL.AsGuid(), string.Empty ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE.AsGuid(), string.Empty ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN.AsGuid(), string.Empty ) );
                attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR.AsGuid(), string.Empty ) );
            }

            // PERSON_GIVING_LAST_CLASSIFICATION_DATE
            attributeValueUpdates.Add( AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE.AsGuid(), context.Now ) );

            // PERSON_GIVING_BIN
            // PERSON_GIVING_PERCENTILE
            // PERSON_GIVING_CURRENT_GIVING_JOURNEY_STAGE
            // PERSON_GIVING_PREVIOUS_GIVING_JOURNEY_STAGE
            // PERSON_GIVING_GIVING_JOURNEY_STAGE_CHANGE_DATE
            // ^^^ These 5 attributes are updated via stored procedures at the beginning of the job.
            // They're handled separately due to their nature, because we need to consider
            // ALL GivingIds in the database for changes, not just the people who've given since last job run.

            return attributeValueUpdates;
        }

        /// <summary>
        /// Processes all attribute value updates by merging them into the database in a single transaction.
        /// </summary>
        /// <param name="context">The job context.</param>
        /// <param name="updates">The dictionary of updates (Key: PersonId, Value: List of AttributeUpdate objects).</param>
        private static bool SaveAttributeValuesForBatch( GivingAutomationContext context, Dictionary<int, List<AttributeValueUpdate>> updates )
        {
            if ( updates == null || !updates.Any() )
            {
                return false;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                    var parameters = updates.ConvertToAttributeValueUpdateListParameter( "@GivingAttributeValueUpdates" );

                    rockContext.Database.ExecuteSqlCommand( @"
MERGE INTO [AttributeValue] AS [Target]
USING @GivingAttributeValueUpdates AS [Source]
ON [Target].[AttributeId] = [Source].[AttributeId] AND [Target].[EntityId] = [Source].[EntityId]
WHEN MATCHED THEN
    UPDATE SET 
        [Value] = [Source].[Value],
        [ModifiedDateTime] = SYSDATETIME()
WHEN NOT MATCHED THEN
    INSERT ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime])
    VALUES (0, [Source].[AttributeId], [Source].[EntityId], [Source].[Value], NEWID(), SYSDATETIME(), SYSDATETIME());", parameters );
                }

                return true;
            }
            catch ( Exception ex )
            {
                AddError( context, "Error when persisting attribute values.", ex );
                return false;
            }
        }

        #endregion Giving Classification Attributes

        #region Recent Transaction Alerts

        /// <summary>
        /// Evaluates alert rules for recent transactions for a single giving unit and persists any new alerts.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="givingId">The giving unit id.</param>
        /// <param name="twelveMonthsTransactions">The giving unit's transactions in the last 12 months.</param>
        /// <param name="twelveMonthsAlerts">The giving unit's existing alerts in the last 12 months.</param>
        /// <param name="eligibleGivingIdsByDataViewId">Precomputed DataView eligibility for the current batch.</param>
        private static void ProcessRecentTxnAlertsPerGivingId(
            GivingAutomationContext context,
            string givingId,
            List<FinancialTransactionView> twelveMonthsTransactions,
            List<AlertView> twelveMonthsAlerts,
            Dictionary<int, HashSet<string>> eligibleGivingIdsByDataViewId )
        {
            var orderedTwelveMonthsTransactions = twelveMonthsTransactions
                .OrderBy( t => t.TransactionDateTime )
                .ToList();

            // Alerts can be generated for transactions given in the last week. One week was chosen since alert types
            // can run at a minimum of 1 day per week since the control is a day of the week picker without being
            // all the way turned off.
            var transactionsToCheckAlerts = orderedTwelveMonthsTransactions
                .Where( t => t.TransactionDateTime >= context.OneWeekAgo )
                .ToList();

            if ( !transactionsToCheckAlerts.Any() )
            {
                return;
            }

            var computedMetricsByAlertTypeId = GivingAutomationHelper.ComputeMetricsForAlertTypes(
                context.AlertTypes,
                orderedTwelveMonthsTransactions,
                context.TransactionWindowDurationHours );

            if ( !computedMetricsByAlertTypeId.Any() )
            {
                return;
            }

            var globalRepeatPreventionDays = context.Settings?.GivingAlertingSettings?.GlobalRepeatPreventionDurationDays;
            var followUpRepeatPreventionDays = context.Settings?.GivingAlertingSettings?.FollowupRepeatPreventionDurationDays;
            var gratitudeRepeatPreventionDays = context.Settings?.GivingAlertingSettings?.GratitudeRepeatPreventionDurationDays;

            bool allowFollowUp = GivingAutomationHelper.AllowFollowUpAlerts( twelveMonthsAlerts, context.Now, globalRepeatPreventionDays, followUpRepeatPreventionDays );
            bool allowGratitude = GivingAutomationHelper.AllowGratitudeAlerts( twelveMonthsAlerts, context.Now, globalRepeatPreventionDays, gratitudeRepeatPreventionDays );

            var alertsToAddToDb = new List<FinancialTransactionAlert>();

            if ( allowFollowUp || allowGratitude )
            {
                var builder = new RecentTxnAlertBuilder(
                    context,
                    givingId,
                    orderedTwelveMonthsTransactions,
                    twelveMonthsAlerts,
                    eligibleGivingIdsByDataViewId,
                    computedMetricsByAlertTypeId );

                foreach ( var transaction in transactionsToCheckAlerts )
                {
                    var alertsForTransaction = builder.BuildAlertsForTransaction( transaction, allowFollowUp, allowGratitude );

                    if ( alertsForTransaction.Any() )
                    {
                        alertsToAddToDb.AddRange( alertsForTransaction );

                        foreach ( var alert in alertsForTransaction )
                        {
                            twelveMonthsAlerts.Add( new AlertView
                            {
                                AlertDateTime = alert.AlertDateTime,
                                AlertTypeId = alert.AlertTypeId,
                                AlertType = context.AlertTypes.First( at => at.Id == alert.AlertTypeId ).AlertType,
                                TransactionId = alert.TransactionId
                            } );
                        }

                        allowFollowUp = GivingAutomationHelper.AllowFollowUpAlerts( twelveMonthsAlerts, context.Now, globalRepeatPreventionDays, followUpRepeatPreventionDays );
                        allowGratitude = GivingAutomationHelper.AllowGratitudeAlerts( twelveMonthsAlerts, context.Now, globalRepeatPreventionDays, gratitudeRepeatPreventionDays );

                        if ( !allowFollowUp && !allowGratitude )
                        {
                            // Repeat-prevention prevents further alert creation for this giving unit,
                            // so break out of the transaction loop.
                            break;
                        }
                    }
                }
            }

            if ( alertsToAddToDb.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.SetCommandTimeout( context.CommandTimeout );
                    new FinancialTransactionAlertService( rockContext ).AddRange( alertsToAddToDb );
                    rockContext.SaveChanges();
                }

                Interlocked.Add( ref context.AlertsCreated, alertsToAddToDb.Count );
                HandlePostAlertsAddedLogic( alertsToAddToDb );
            }
        }

        internal sealed class RecentTxnAlertBuilder
        {
            private readonly GivingAutomationContext _context;
            private readonly string _givingId;
            private readonly List<FinancialTransactionView> _orderedTransactions;
            private readonly List<AlertView> _recentAlerts;
            private readonly Dictionary<int, HashSet<string>> _eligibleGivingIdsByDataViewId;
            private readonly Dictionary<int, AlertTypeComputedMetrics> _computedMetricsByAlertTypeId;

            /// <summary>
            /// Initializes a new instance of the <see cref="RecentTxnAlertBuilder"/> class.
            /// </summary>
            internal RecentTxnAlertBuilder(
                GivingAutomationContext context,
                string givingId,
                List<FinancialTransactionView> orderedTransactions,
                List<AlertView> recentAlerts,
                Dictionary<int, HashSet<string>> eligibleGivingIdsByDataViewId,
                Dictionary<int, AlertTypeComputedMetrics> computedMetricsByAlertTypeId )
            {
                _context = context;
                _givingId = givingId;
                _orderedTransactions = orderedTransactions;
                _recentAlerts = recentAlerts;
                _eligibleGivingIdsByDataViewId = eligibleGivingIdsByDataViewId;
                _computedMetricsByAlertTypeId = computedMetricsByAlertTypeId;
            }

            /// <summary>
            /// Evaluates alert types for a single recent transaction and returns any alerts to create.
            /// </summary>
            internal List<FinancialTransactionAlert> BuildAlertsForTransaction( FinancialTransactionView transaction, bool allowFollowUp, bool allowGratitude )
            {
                var alerts = new List<FinancialTransactionAlert>();

                var index = _orderedTransactions.IndexOf( transaction );
                DateTime previousTransactionDate;

                if ( index > 0 )
                {
                    previousTransactionDate = _orderedTransactions[index - 1].TransactionDateTime;
                }
                else
                {
                    previousTransactionDate = transaction.TransactionDateTime;
                }

                var daysSinceLastTransaction = ( transaction.TransactionDateTime - previousTransactionDate ).TotalDays;

                foreach ( var alertType in _context.AlertTypes )
                {
                    if ( !_computedMetricsByAlertTypeId.TryGetValue( alertType.Id, out var metrics ) )
                    {
                        continue;
                    }

                    var newAlert = BuildAlert( alertType, transaction, daysSinceLastTransaction, metrics, allowFollowUp, allowGratitude );
                    if ( newAlert == null )
                    {
                        continue;
                    }

                    alerts.Add( newAlert );

                    if ( !alertType.ContinueIfMatched )
                    {
                        break;
                    }
                }

                return alerts;
            }

            private FinancialTransactionAlert BuildAlert(
                FinancialTransactionAlertType alertType,
                FinancialTransactionView transaction,
                double daysSinceLastTransaction,
                AlertTypeComputedMetrics metrics,
                bool allowFollowUp,
                bool allowGratitude )
            {
                // Make sure this transaction / alert type combo doesn't already exist.
                if ( _recentAlerts.Any( a => a.AlertTypeId == alertType.Id && a.TransactionId == transaction.Id ) )
                {
                    return null;
                }

                // Ensure that this alert type is allowed (might be disallowed because of repeat prevention durations).
                if ( !allowFollowUp && alertType.AlertType == AlertType.FollowUp )
                {
                    return null;
                }

                if ( !allowGratitude && alertType.AlertType == AlertType.Gratitude )
                {
                    return null;
                }

                // Check the days since the last transaction are within allowed range.
                if ( alertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > alertType.MaximumDaysSinceLastGift.Value )
                {
                    return null;
                }

                // Check if this alert type has already been alerted too recently.
                if ( alertType.RepeatPreventionDuration.HasValue && _recentAlerts.Any( a => a.AlertTypeId == alertType.Id ) )
                {
                    var lastAlertOfTypeDate = _recentAlerts
                        .Where( a => a.AlertTypeId == alertType.Id )
                        .Max( a => a.AlertDateTime );
                    var daysSinceLastAlert = ( _context.Now - lastAlertOfTypeDate ).TotalDays;

                    if ( daysSinceLastAlert <= alertType.RepeatPreventionDuration.Value )
                    {
                        // Alert would be too soon after the last alert was generated
                        return null;
                    }
                }

                // Check if the campus is a match.
                if ( alertType.CampusId.HasValue && alertType.CampusId != transaction.AuthorizedPersonCampusId )
                {
                    return null;
                }

                decimal transactionAmount;

                // Account restrictions (if any) and the transaction amount for this alert.
                if ( metrics.AlertTypeAccountIds != null )
                {
                    var detailsForThisTransaction = transaction.GetTransactionDetails();
                    if ( !detailsForThisTransaction.Any( d => metrics.AlertTypeAccountIds.Contains( d.AccountId ) ) )
                    {
                        // Transaction isn't tied to the relevant accounts for this alert type.
                        return null;
                    }

                    transactionAmount = detailsForThisTransaction
                        .Where( d => metrics.AlertTypeAccountIds.Contains( d.AccountId ) )
                        .Sum( d => d.Amount );
                }
                else
                {
                    transactionAmount = transaction.TotalAmount;
                }

                // If there is either FrequencySensitivity or AmountSensitivity rule, we need to have enough transaction history.
                if ( alertType.FrequencySensitivityScale.HasValue || alertType.AmountSensitivityScale.HasValue )
                {
                    if ( metrics.TransactionCount < _context.MinimumTransactionCountForSensitivityAlertTypes )
                    {
                        return null;
                    }
                }

                if ( alertType.MinimumGiftAmount.HasValue && transactionAmount < alertType.MinimumGiftAmount.Value )
                {
                    // Gift is less than this rule allows
                    return null;
                }

                if ( alertType.MaximumGiftAmount.HasValue && transactionAmount > alertType.MaximumGiftAmount.Value )
                {
                    // Gift is more than this rule allows
                    return null;
                }

                if ( alertType.MinimumMedianGiftAmount.HasValue && metrics.QuartileRanges.MedianAmount < alertType.MinimumMedianGiftAmount.Value )
                {
                    // Median gift amount is too small for this rule
                    return null;
                }

                if ( alertType.MaximumMedianGiftAmount.HasValue && metrics.QuartileRanges.MedianAmount > alertType.MaximumMedianGiftAmount.Value )
                {
                    // Median gift amount is too large for this rule
                    return null;
                }

                // Check the number of IQRs that the amount varies.
                var numberOfAmountIqrs = GivingAutomationHelper.GetAmountIqrCount( metrics.QuartileRanges, transactionAmount );

                var meanFrequencyDays = metrics.FrequencyStats.MeanDays;
                var frequencyStdDevDays = metrics.FrequencyStats.StdDevDays;
                var numberOfFrequencyStdDevs = GivingAutomationHelper.GetFrequencyDeviationCount( frequencyStdDevDays, meanFrequencyDays, Convert.ToDecimal( daysSinceLastTransaction ) );

                // Detect which thing, amount or frequency, is exceeding the rule's sensitivity scale.
                var reasons = new List<string>();

                /*
                    11-18-2021 MDP

                    The Sensitivity scale logic is dependant on whether Gratitude or Follow-up is selected:
                        - Gratitude looks at 'better than usual' transactions (higher amount than usual, or earlier than usual)
                        - Follow-up looks at 'worse than usual'  transactions (lower amount than usual, or later than usual)

                    Example:
                        - Normal Range $500 +/- $30
                        - Gratitude (with sensitivity of 3), would alert if $590 or more
                */

                if ( alertType.AlertType == AlertType.Gratitude )
                {
                    // For example, if Follow-up with a Sensitivity of 3
                    // Normal is $500 +/- 50
                    // Gratitude with look at values $650 or more
                    if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs >= alertType.AmountSensitivityScale.Value )
                    {
                        // Gift is larger amount than they usually give (Larger than Usual alert)
                        // Note that this is only for people have established a normal range of giving, so it would only
                        // people with some transaction history. ('Large Gift alert' is different than 'Larger than Usual alert')
                        reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                    }

                    if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs >= alertType.FrequencySensitivityScale.Value )
                    {
                        // Gift is earlier than when they usually give( Early Gift Alert)
                        reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                    }
                }
                else if ( alertType.AlertType == AlertType.FollowUp )
                {
                    // Follow up 'Flips the Sign' of what was specified.
                    // For example, if Followup with a Sensitivity of 3
                    // Normal is $500 +/- 50
                    // Follow-up with look at values $350 or less
                    if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs <= ( alertType.AmountSensitivityScale.Value * -1 ) )
                    {
                        // Gift is outside the amount sensitivity scale (Smaller Amount than Usual alert)
                        reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                    }

                    if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs <= ( alertType.FrequencySensitivityScale.Value * -1 ) )
                    {
                        // Gift is outside the frequency sensitivity scale (Later than Usual)
                        reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                    }
                }

                if ( !reasons.Any() )
                {
                    bool hasSensitivityRules = alertType.AmountSensitivityScale.HasValue || alertType.FrequencySensitivityScale.HasValue;

                    if ( hasSensitivityRules )
                    {
                        // this alerts has Sensitivity rules, but neither triggered an alert, so we continue without generating alert 
                        return null;
                    }
                    else
                    {
                        // If the case of no sensitivity rules and no sensitivity alerts,
                        // Check for a simple 'Transaction Amount Over $x' type of alert (no other criteria other than Minimum Amount)
                        if ( alertType.MinimumGiftAmount.HasValue && transactionAmount >= alertType.MinimumGiftAmount.Value )
                        {
                            // this is the 'Large Gift Amount' use case
                            reasons.Add( nameof( alertType.MinimumGiftAmount ) );
                        }
                        else
                        {
                            // No alert criteria is met, so continue without an alert
                            return null;
                        }
                    }
                }

                // DataView restriction.
                if ( alertType.DataViewId.HasValue )
                {
                    if ( _eligibleGivingIdsByDataViewId == null
                        || !_eligibleGivingIdsByDataViewId.TryGetValue( alertType.DataViewId.Value, out var eligibleGivingIds )
                        || !eligibleGivingIds.Contains( _givingId ) )
                    {
                        return null;
                    }
                }

                var frequencyDeviation = meanFrequencyDays - Convert.ToDecimal( daysSinceLastTransaction );

                return new FinancialTransactionAlert
                {
                    TransactionId = transaction.Id,
                    PersonAliasId = transaction.AuthorizedPersonAliasId,
                    GivingId = _givingId,
                    AlertTypeId = alertType.Id,
                    Amount = transactionAmount,
                    AmountCurrentMedian = metrics.QuartileRanges.MedianAmount,
                    AmountCurrentIqr = metrics.QuartileRanges.IQRAmount,
                    AmountIqrMultiplier = numberOfAmountIqrs,
                    FrequencyCurrentMean = meanFrequencyDays,
                    FrequencyCurrentStandardDeviation = frequencyStdDevDays,
                    FrequencyDifferenceFromMean = frequencyDeviation,
                    FrequencyZScore = numberOfFrequencyStdDevs,
                    ReasonsKey = reasons.ToJson(),
                    AlertDateTime = _context.Now,
                    AlertDateKey = _context.Now.ToDateKey()
                };
            }

        }

        #endregion Recent Transaction Alerts

        #region Late Transaction Alerts

        /// <summary>
        /// Processes "late transaction" alerts (expected gifts that did not occur) for configured alert types.
        /// </summary>
        /// <param name="context">The current job context.</param>
        private static void ProcessLateTxnAlerts( GivingAutomationContext context )
        {
            var lateTxnAlertTypes = context.AlertTypes
                .Where( at => at.AlertType == AlertType.FollowUp && at.FrequencySensitivityScale.HasValue )
                .OrderBy( at => at.Order )
                .ToList();

            if ( !lateTxnAlertTypes.Any() )
            {
                return;
            }

            /*
                 1/7/2026 - MSE

                 This HashSet tracks GivingIds that have already triggered a Late Txn Alert Type
                 where ContinueIfMatched is set to false.

                 Late Alert Types are processed in order based on their Order value. When a
                 person matches an alert that is marked as exclusive (ContinueIfMatched == false),
                 their GivingId is added to this collection. As lower-priority ( higher Order ) alert types are
                 evaluated, this list is checked to determine whether the person should be skipped.

                 This enforces a hierarchy of Late Txn Alert Types and prevents a single person
                 from receiving multiple alerts for the same behavior.

                 Reason: Prevent lower-priority alerts from being sent after
                 an exclusive alert has already been triggered.
            */
            var givingIdsToExcludeFromSubsequentAlerts = new HashSet<string>();


            /*
                1/11/2026 - MSE

                The late-alert path used to apply GlobalRepeatPreventionDurationDays using alerts filtered only
                to the current AlertTypeId, which meant "global" repeat prevention was not actually global.

                Fix: build the all-alert-types dictionary once (outside the alert-type loop) and use it only for the global cooldown check.
                We still build the per-alert-type list here for the per-alert-type repeat prevention check.

                Reason: Global repeat prevention must consider all alert types for the giving unit.
            */
            var recentAlertsByGivingId = GetRecentAlertsByGivingIdForLateAlerts( context );

            var alertsAdded = new List<FinancialTransactionAlert>();

            foreach ( var lateTxnAlertType in lateTxnAlertTypes )
            {
                var alerts = ProcessLateTxnAlertType( context, lateTxnAlertType, givingIdsToExcludeFromSubsequentAlerts, recentAlertsByGivingId );
                if ( alerts == null || alerts.Count == 0 )
                {
                    continue;
                }

                if ( !lateTxnAlertType.ContinueIfMatched )
                {
                    foreach ( var alert in alerts )
                    {
                        if ( alert.GivingId != null )
                        {
                            givingIdsToExcludeFromSubsequentAlerts.Add( alert.GivingId );
                        }
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.SetCommandTimeout( context.CommandTimeout );
                    new FinancialTransactionAlertService( rockContext ).AddRange( alerts );
                    rockContext.SaveChanges();
                }

                Interlocked.Add( ref context.AlertsCreated, alerts.Count );
                alertsAdded.AddRange( alerts );
            }

            if ( alertsAdded.Any() )
            {
                HandlePostAlertsAddedLogic( alertsAdded );
            }
        }

        /// <summary>
        /// Processes a single late-transaction alert type and returns alerts to persist.
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="lateGiftAlertType">The late-transaction alert type to evaluate.</param>
        /// <param name="givingIdsToExcludeFromSubsequentAlerts">Giving ids excluded due to exclusive alerts.</param>
        /// <param name="recentAlertsByGivingId">Recent alerts (all alert types) for use in the global repeat prevention check.</param>
        private static List<FinancialTransactionAlert> ProcessLateTxnAlertType( GivingAutomationContext context, FinancialTransactionAlertType lateGiftAlertType, HashSet<string> givingIdsToExcludeFromSubsequentAlerts, Dictionary<string, List<AlertView>> recentAlertsByGivingId )
        {
            var alertsForThisAlertType = new List<FinancialTransactionAlert>();

            if ( lateGiftAlertType == null )
            {
                return alertsForThisAlertType;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                var accountIds = GivingAutomationHelper.GetAlertTypeAccountIds( lateGiftAlertType );

                var transactionsQuery = new FinancialTransactionService( rockContext )
                    .GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        t.TransactionDateTime.HasValue &&
                        t.TransactionDateTime >= context.OneYearAgo &&
                        t.AuthorizedPersonAliasId.HasValue &&
                        t.AuthorizedPersonAlias.Person.GivingId != null &&
                        t.AuthorizedPersonAlias.Person.GivingId != string.Empty
                    );

                if ( lateGiftAlertType.DataViewId.HasValue )
                {
                    var dataViewId = lateGiftAlertType.DataViewId.Value;

                    var dataViewQuery = GetDataViewGivingIdQuery( context, dataViewId, rockContext );
                    if ( dataViewQuery != null )
                    {
                        transactionsQuery = transactionsQuery.Where( t => dataViewQuery.Contains( t.AuthorizedPersonAlias.Person.GivingId ) );
                    }
                }

                if ( accountIds != null )
                {
                    transactionsQuery = transactionsQuery.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
                }

                var twelveMonthsTransactions = transactionsQuery
                    .Select( t => new FinancialTransactionView
                    {
                        Id = t.Id,
                        AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                        AuthorizedPersonGivingId = t.AuthorizedPersonAlias.Person.GivingId,
                        AuthorizedPersonCampusId = t.AuthorizedPersonAlias.Person.PrimaryCampusId,
                        TransactionDateTime = t.TransactionDateTime.Value,
                        CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                        SourceTypeValueId = t.SourceTypeValueId,
                        IsScheduled = t.ScheduledTransactionId.HasValue,
                        TransactionDetails = t.TransactionDetails.Select( d => new FinancialTransactionDetailView { AccountId = d.AccountId, Amount = d.Amount } ).ToList(),
                        RefundDetails = t.Refunds.SelectMany( r => r.FinancialTransaction.TransactionDetails ).Select( d => new FinancialTransactionDetailView { AccountId = d.AccountId, Amount = d.Amount } ).ToList()
                    } )
                    .ToList();

                if ( !twelveMonthsTransactions.Any() )
                {
                    return alertsForThisAlertType;
                }

                /* 
                    08/16/2022 MP
              
                    Select the data from the database into a list before doing the GroupBy. This improves performance
                    significantly in this case since we'll be doing the GroupBy in memory instead of having SQL
                    figure it out. 
                */
                var transactionsByGivingId = twelveMonthsTransactions
                    .GroupBy( t => t.AuthorizedPersonGivingId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy( t => t.TransactionDateTime ).ToList() );

                var recentAlertsOfType = new FinancialTransactionAlertService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.AlertTypeId == lateGiftAlertType.Id && a.AlertDateTime > context.OneYearAgo && a.GivingId != null )
                    .Select( a => new
                    {
                        a.GivingId,
                        a.AlertDateTime,
                        a.FinancialTransactionAlertType.AlertType,
                        a.AlertTypeId
                    } )
                    .ToList();

                var givingIdSetForAlertType = new HashSet<string>( transactionsByGivingId.Keys );
                var recentAlertsOfAlertTypeByGivingId = new Dictionary<string, List<AlertView>>();

                recentAlertsOfAlertTypeByGivingId = recentAlertsOfType
                    .Where( a => givingIdSetForAlertType.Contains( a.GivingId ) )
                    .GroupBy( a => a.GivingId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select( a => new AlertView
                        {
                            AlertDateTime = a.AlertDateTime,
                            AlertType = a.AlertType,
                            AlertTypeId = a.AlertTypeId,
                            TransactionId = null
                        } ).OrderByDescending( x => x.AlertDateTime ).ToList() );

                var globalRepeatPreventionDays = context.Settings?.GivingAlertingSettings?.GlobalRepeatPreventionDurationDays;
                var followUpRepeatPreventionDays = context.Settings?.GivingAlertingSettings?.FollowupRepeatPreventionDurationDays;

                var builder = new LateTxnAlertBuilder( context, lateGiftAlertType, givingIdsToExcludeFromSubsequentAlerts );

                foreach ( var kvp in transactionsByGivingId )
                {
                    var givingId = kvp.Key;
                    var orderedTransactions = kvp.Value;

                    // Late alerts are always follow-up type.
                    var allowFollowUp = GivingAutomationHelper.AllowFollowUpAlerts(
                        // this now correctly respects GlobalRepeatPreventionDurationDays by checking alerts for all alert types
                        recentAlertsByGivingId.GetValueOrNull( givingId ),
                        context.Now,
                        globalRepeatPreventionDays,
                        followUpRepeatPreventionDays );

                    if ( !allowFollowUp )
                    {
                        continue;
                    }

                    var alert = builder.BuildAlert(
                        givingId,
                        orderedTransactions,
                        recentAlertsOfAlertTypeByGivingId.GetValueOrNull( givingId ) );

                    if ( alert == null )
                    {
                        continue;
                    }

                    alertsForThisAlertType.Add( alert );
                }

                return alertsForThisAlertType;
            }
        }

        /// <summary>
        /// Loads recent alerts (all alert types) from the last 12 months, grouped by GivingId.
        /// Used by the late-alert path for accurate global repeat prevention check.
        /// </summary>
        private static Dictionary<string, List<AlertView>> GetRecentAlertsByGivingIdForLateAlerts( GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.SetCommandTimeout( context.CommandTimeout );

                var alerts = new FinancialTransactionAlertService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.AlertDateTime > context.OneYearAgo && a.GivingId != null )
                    .Select( a => new
                    {
                        a.GivingId,
                        a.AlertDateTime,
                        a.FinancialTransactionAlertType.AlertType,
                        a.AlertTypeId
                    } )
                    .ToList();

                return alerts
                    .GroupBy( a => a.GivingId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select( a => new AlertView
                        {
                            AlertDateTime = a.AlertDateTime,
                            AlertType = a.AlertType,
                            AlertTypeId = a.AlertTypeId,
                            TransactionId = null
                        } ).ToList() );
            }
        }

        internal sealed class LateTxnAlertBuilder
        {
            private readonly GivingAutomationContext _context;
            private readonly FinancialTransactionAlertType _alertType;
            private readonly HashSet<string> _givingIdsToExcludeFromSubsequentAlerts;

            /// <summary>
            /// Initializes a new instance of the <see cref="LateTxnAlertBuilder"/> class.
            /// </summary>
            internal LateTxnAlertBuilder( GivingAutomationContext context, FinancialTransactionAlertType alertType, HashSet<string> givingIdsToExcludeFromSubsequentAlerts )
            {
                _context = context;
                _alertType = alertType;
                _givingIdsToExcludeFromSubsequentAlerts = givingIdsToExcludeFromSubsequentAlerts;
            }

            /// <summary>
            /// Builds a late-transaction alert for a single giving unit, if criteria are met.
            /// </summary>
            internal FinancialTransactionAlert BuildAlert(
                string givingId,
                List<FinancialTransactionView> orderedTransactions,
                List<AlertView> recentAlertsOfThisAlertType )
            {
                if ( _givingIdsToExcludeFromSubsequentAlerts.Contains( givingId ) )
                {
                    // if an alert was already generated for this GivingId, don't add any more new ones if ContinueIfMatched is false;
                    return null;
                }

                if ( orderedTransactions == null || orderedTransactions.Count == 0 )
                {
                    return null;
                }

                var metrics = GivingAutomationHelper.ComputeMetricsForAlertType(
                    _alertType,
                    orderedTransactions,
                    _context.TransactionWindowDurationHours );

                if ( metrics.TransactionCount == 0 )
                {
                    return null;
                }

                if ( metrics.TransactionCount < _context.MinimumTransactionCountForClassifications )
                {
                    // A Late Transaction Alert requires Frequency Sensitivity Rules
                    return null;
                }

                var mostRecentTransaction = metrics.MostRecentTransaction;
                var quartileRanges = metrics.QuartileRanges;
                var frequencyStats = metrics.FrequencyStats;

                if ( mostRecentTransaction == null )
                {
                    // no transaction in last 12 months for this AlertType's criteria
                    return null;
                }

                if ( !frequencyStats.NextExpectedGiftDate.HasValue )
                {
                    // Since there is NextExpectedGiftDate based on giving patterns, there wouldn't be a late alert
                    return null;
                }

                if ( !frequencyStats.LastTransactionDateTime.HasValue )
                {
                    // No previous transaction
                    return null;
                }

                // Don't generate late alerts more than once for a giving unit since the last time they gave.
                DateTime? mostRecentAlertOfThisTypeDateTime = recentAlertsOfThisAlertType?.FirstOrDefault()?.AlertDateTime;
                if ( mostRecentAlertOfThisTypeDateTime.HasValue )
                {
                    if ( mostRecentAlertOfThisTypeDateTime.Value > frequencyStats.LastTransactionDateTime.Value )
                    {
                        // Don't generate late alerts more than once for a giving group since the last time they gave
                        return null;
                    }

                    // Check if this alert type has already been alerted too recently
                    if ( _alertType.RepeatPreventionDuration.HasValue )
                    {
                        var daysSinceLastAlert = ( _context.Now - mostRecentAlertOfThisTypeDateTime.Value ).TotalDays;
                        if ( daysSinceLastAlert <= _alertType.RepeatPreventionDuration.Value )
                        {
                            // Alert would be too soon after the last alert was generated
                            return null;
                        }
                    }
                }

                var daysSinceLastTransaction = ( _context.Now - frequencyStats.LastTransactionDateTime.Value ).TotalDays;

                // Check the maximum days since the last alert
                if ( _alertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > _alertType.MaximumDaysSinceLastGift.Value )
                {
                    return null;
                }

                // Check if the campus is a match
                if ( _alertType.CampusId.HasValue && _alertType.CampusId != mostRecentTransaction.AuthorizedPersonCampusId )
                {
                    return null;
                }

                if ( _alertType.MinimumMedianGiftAmount.HasValue && quartileRanges.MedianAmount < _alertType.MinimumMedianGiftAmount.Value )
                {
                    // Median gift amount is too small for this rule
                    return null;
                }

                if ( _alertType.MaximumMedianGiftAmount.HasValue && quartileRanges.MedianAmount > _alertType.MaximumMedianGiftAmount.Value )
                {
                    // Median gift amount is too large for this rule
                    return null;
                }

                var numberOfFrequencyStdDevs = GivingAutomationHelper.GetFrequencyDeviationCount(
                    frequencyStats.StdDevDays,
                    frequencyStats.MeanDays,
                    ( decimal ) daysSinceLastTransaction );

                var reasons = new List<string>();

                if ( numberOfFrequencyStdDevs <= ( _alertType.FrequencySensitivityScale.Value * -1 ) )
                {
                    // The current date is later the frequency sensitivity scale
                    reasons.Add( nameof( _alertType.FrequencySensitivityScale ) );
                }

                if ( !reasons.Any() )
                {
                    // If the current date is earlier than the expected next transaction date, don't generate an alert
                    return null;
                }

                return new FinancialTransactionAlert
                {
                    TransactionId = null,
                    PersonAliasId = mostRecentTransaction.AuthorizedPersonAliasId,
                    GivingId = mostRecentTransaction.AuthorizedPersonGivingId,
                    AlertTypeId = _alertType.Id,
                    Amount = null,
                    AmountCurrentMedian = quartileRanges.MedianAmount,
                    AmountCurrentIqr = quartileRanges.IQRAmount,
                    AmountIqrMultiplier = null,
                    FrequencyCurrentMean = frequencyStats.MeanDays,
                    FrequencyCurrentStandardDeviation = frequencyStats.StdDevDays,
                    FrequencyDifferenceFromMean = null,
                    FrequencyZScore = null,
                    ReasonsKey = reasons.ToJson(),
                    AlertDateTime = _context.Now,
                    AlertDateKey = _context.Now.ToDateKey()
                };
            }
        }


        #endregion Late Transaction Alerts

        #region Helper Methods

        /// <summary>
        /// Saves the "last classifications run date time" value back to Giving Automation settings.
        /// </summary>
        private static void SaveLastClassificationsRunDateTime( GivingAutomationContext context, DateTime lastRunDateTime )
        {
            if ( context?.Settings == null )
            {
                return;
            }

            context.Settings.GivingClassificationSettings.LastRunDateTime = lastRunDateTime;
            GivingAutomationSettings.SaveGivingAutomationSettings( context.Settings );
        }

        /// <summary>
        /// Performs follow-up processing after alerts have been persisted (workflows, bus events, communications, etc).
        /// </summary>
        /// <param name="alertsAdded">The alerts that were added.</param>
        private static void HandlePostAlertsAddedLogic( List<FinancialTransactionAlert> alertsAdded )
        {
            foreach ( var alert in alertsAdded )
            {
                if ( alert.Id == 0 )
                {
                    continue;
                }

                // This Task does all of the various Workflow, Bus Events, System Communications, etc
                new ProcessTransactionAlertActions.Message
                {
                    FinancialTransactionAlertId = alert.Id
                }.Send();
            }
        }

        /// <summary>
        /// Gets a giving-unit attribute value by comparing the value across all people in the giving unit.
        /// </summary>
        /// <param name="persons">The people in the giving unit.</param>
        /// <param name="guidString">The attribute Guid (string form).</param>
        private static string GetGivingUnitAttributeValue( List<Person> persons, string guidString )
        {
            if ( !persons.Any() )
            {
                return string.Empty;
            }

            var key = AttributeCache.Get( guidString )?.Key;

            if ( key.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var unitValue = persons.First().GetAttributeValue( key );

            for ( var i = 1; i < persons.Count; i++ )
            {
                var person = persons[i];
                var personValue = person.GetAttributeValue( key );

                if ( unitValue != personValue )
                {
                    // The people in this giving unit have different values for this. We don't know which is actually correct, so assume no value.
                    return string.Empty;
                }
            }

            return unitValue;
        }

        /// <summary>
        /// Returns the attribute value updates to apply when a giving unit has no qualifying transactions in the last 12 months
        /// (based on the current Giving Automation transaction filters).
        /// </summary>
        /// <remarks>
        /// Intentionally does not clear <see cref="Rock.SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE"/> or <see cref="Rock.SystemGuid.Attribute.PERSON_ERA_LAST_GAVE"/>.
        /// </remarks>
        internal static List<AttributeValueUpdate> CreateClearedAttributeValueUpdates( DateTime now )
        {
            return new List<AttributeValueUpdate>
            {
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE.AsGuid(), string.Empty ),
                AttributeValueUpdateFactory.Create( SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE.AsGuid(), now )
            };
        }

        /// <summary>
        /// Adds an error message to the job context and logs the exception (if provided).
        /// </summary>
        /// <param name="context">The current job context.</param>
        /// <param name="message">The error message to record.</param>
        /// <param name="ex">The exception to log (optional).</param>
        private static void AddError( GivingAutomationContext context, string message, Exception ex = null )
        {
            if ( context != null && message.IsNotNullOrWhiteSpace() )
            {
                context.Errors.Enqueue( message );
            }

            if ( ex != null )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        #endregion Helper Methods

        #region DTOs

        /// <summary>
        /// Holds job-scoped state and configuration values used while executing the Giving Automation processing run.
        /// </summary>
        internal sealed class GivingAutomationContext
        {
            /// <summary>
            /// Gets or sets the current Giving Automation settings.
            /// </summary>
            public GivingAutomationSettings Settings { get; set; }

            /// <summary>
            /// Gets or sets the current Rock date/time for this execution.
            /// </summary>
            public DateTime Now { get; set; }

            /// <summary>
            /// Gets or sets the timestamp representing one year prior to <see cref="Now"/>.
            /// </summary>
            public DateTime OneYearAgo { get; set; }

            /// <summary>
            /// Gets or sets the timestamp representing one week prior to <see cref="Now"/>.
            /// </summary>
            public DateTime OneWeekAgo { get; set; }

            /// <summary>
            /// Gets or sets the SQL command timeout, in seconds.
            /// </summary>
            public int CommandTimeout { get; set; }

            /// <summary>
            /// Gets or sets the alert types that are configured to run today.
            /// </summary>
            public List<FinancialTransactionAlertType> AlertTypes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether debug mode is enabled for this run.
            /// </summary>
            public bool DebugModeEnabled { get; set; }

            /// <summary>
            /// Tracks the number of alerts created during this run (updated via <see cref="System.Threading.Interlocked"/>).
            /// </summary>
            public int AlertsCreated;

            /// <summary>
            /// Tracks the number of giving units visited/attempted during this run.
            /// </summary>
            public int GivingUnitsProcessed;

            /// <summary>
            /// Tracks the number of giving units that failed processing due to an exception.
            /// </summary>
            public int GivingUnitsFailed;

            /// <summary>
            /// A thread-safe collection of error messages encountered during job execution.
            /// </summary>
            public ConcurrentQueue<string> Errors { get; } = new ConcurrentQueue<string>();

            /// <summary>
            /// Gets or sets the time period within which transactions will be considered as a single giving event.
            /// </summary>
            public int TransactionWindowDurationHours { get; set; } = 24;

            /* 11-17-2021 MDP

            Talking with some local statistics nerds, 4 datapoints is the absolute bare minimum for an IQR,
            and 5 datapoints is the bare minimum for anything meaningful. It gets
            more meaningful as you approach 10. However, in the case of Church Giving,
            "at least 5 in last 12 months" is a reasonable rule (without making it more complex).

            Therefore here are our rules:
                -  Giving Classifications requires at least 5 in last 12 months
                -  Alert Types
                    - With sensitivity: Requires at least 5 in last 12 months
                    - Without sensitivity, doesn't require transaction history,
            */

            /// <summary>
            /// The minimum transaction count for classifications
            /// </summary>
            public int MinimumTransactionCountForClassifications { get; set; } = 5;

            /// <summary>
            /// The minimum transaction count needed to get meaningful statistics
            /// for Alert Types that have either the Amount Sensitivity or Frequency Sensitivity defined.
            /// Need at least 5 in last 12 months (see above engineering note).
            /// </summary>
            public int MinimumTransactionCountForSensitivityAlertTypes { get; set; } = 5;

            /// <summary>
            /// Initializes a new instance of the <see cref="GivingAutomationContext"/> class with default values.
            /// </summary>
            public GivingAutomationContext()
            {
                Settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                Now = RockDateTime.Now;
                OneYearAgo = Now.AddMonths( -12 );
                OneWeekAgo = Now.AddDays( -7 );
            }
        }

        #endregion DTOs

        #region Debug

        /// <summary>
        /// Lightweight debug helper used only when running in a development environment and debug mode is enabled.
        /// </summary>
        private static class DebugHelper
        {
            private static bool IsEnabled { get; set; } = false;
            private static int ProcessedCount = 0;
            private static readonly Stopwatch TotalStopwatch = new Stopwatch();
            private static ConcurrentBag<long> ElapsedTimesMs = new ConcurrentBag<long>();

            /// <summary>
            /// Enables debug logging for this job run.
            /// </summary>
            internal static void Enable()
            {
                IsEnabled = true;
                ProcessedCount = 0;

                TotalStopwatch.Restart();

                ElapsedTimesMs = new ConcurrentBag<long>();
            }

            /// <summary>
            /// Records that one giving unit was processed and tracks elapsed time (in ms) when available.
            /// </summary>
            /// <param name="elapsedMs">Elapsed milliseconds for processing a single giving unit.</param>
            internal static void RecordProcessed( long elapsedMs )
            {
                if ( !IsEnabled )
                {
                    return;
                }

                Interlocked.Increment( ref ProcessedCount );
                if ( elapsedMs >= 0 )
                {
                    ElapsedTimesMs.Add( elapsedMs );
                }
            }

            /// <summary>
            /// Writes a final debug summary for the job run.
            /// </summary>
            /// <param name="totalCount">The number of giving units processed.</param>
            /// <param name="processedCount">The number of giving units visited/attempted.</param>
            /// <param name="failedCount">The number of giving units that failed.</param>
            /// <param name="errorCount">The number of errors recorded.</param>
            internal static void WriteFinalSummary( int totalCount, int processedCount, int failedCount, int errorCount )
            {
                if ( !IsEnabled || totalCount == 0 )
                {
                    return;
                }

                TotalStopwatch.Stop();

                var avg = ElapsedTimesMs.Any() ? ElapsedTimesMs.Average() : 0;
                Write( $"Summary: total={totalCount}, processed={processedCount}, failed={failedCount}, errors={errorCount}, elapsedMs={TotalStopwatch.ElapsedMilliseconds}, avgMsPerGivingUnit={avg:0.0}" );
            }

            internal static void Write( string message )
            {
                if ( IsEnabled && System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    Debug.WriteLine( $"\tGiving Automation {RockDateTime.Now:mm:ss.f} {message}" );
                }
            }

        }

        #endregion Debug
    }
}