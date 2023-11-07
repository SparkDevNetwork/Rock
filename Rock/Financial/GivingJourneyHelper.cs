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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// </summary>
    internal class GivingJourneyHelper
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GivingJourneyHelper"/> class.
        /// </summary>
        internal GivingJourneyHelper()
        {
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the SQL command timeout.
        /// </summary>
        /// <value>The SQL command timeout.</value>
        internal int? SqlCommandTimeout { get; set; }

        /// <summary>
        /// Gets the total count of updated journey stages
        /// </summary>
        /// <value>The updated journey stage count.</value>
        internal int UpdatedJourneyStageCount
        {
            get
            {
                return _givingJourneyChangeCount.Sum( x => x.Value ) + _givingJourneyNoneOfTheAboveCount;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when [on progress].
        /// </summary>
        internal event EventHandler<ProgressEventArgs> OnProgress;

        #endregion Events

        #region Fields

        // NOTE: Have these get grabbed from the cache every time to avoid issues if the cache is flushed while this is running.
        private AttributeCache _currentJourneyStageAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_CURRENT_GIVING_JOURNEY_STAGE.AsGuid() );
        private AttributeCache _previousJourneyStageAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_PREVIOUS_GIVING_JOURNEY_STAGE.AsGuid() );
        private AttributeCache _journeyStageChangeDateAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_GIVING_JOURNEY_STAGE_CHANGE_DATE.AsGuid() );

        // populated in ProcessGivingJourneys
        private Dictionary<string, Dictionary<int, List<AttributeValueCache>>> _attributeValuesByGivingIdAndPersonId = null;
        private Dictionary<string, List<int>> _personIdsByGivingId = null;

        private Dictionary<GivingJourneyStage, int> _givingJourneyChangeCount = new Dictionary<GivingJourneyStage, int>();
        private int _givingJourneyNoneOfTheAboveCount = 0;

        #endregion Fields

        #region Internal Methods

        /// <summary>
        /// Processes the giving journeys.
        /// </summary>
        internal void UpdateGivingJourneyStages()
        {
            var givingAnalyticsSetting = GivingAutomationSettings.LoadGivingAutomationSettings();

            var rockContext = new RockContext();
#if REVIEW_NET5_0_OR_GREATER
            rockContext.Database.SetCommandTimeout( this.SqlCommandTimeout );
#else
            rockContext.Database.CommandTimeout = this.SqlCommandTimeout;
#endif
            var personService = new PersonService( rockContext );

            // Limit to only Business and Person type records.
            // Include deceased to cover transactions that could have occurred when they were not deceased
            // or transactions that are dated after they were marked deceased.
            var personQuery = personService.Queryable( new PersonService.PersonQueryOptions
            {
                IncludeDeceased = true,
                IncludeBusinesses = true,
                IncludePersons = true,
                IncludeNameless = false,
                IncludeRestUsers = false
            } );

            var personAliasService = new PersonAliasService( rockContext );
            var personAliasQuery = personAliasService.Queryable();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionGivingAnalyticsQuery = financialTransactionService.GetGivingAutomationSourceTransactionQuery();

            if ( OnProgress != null )
            {
                string progressMessage = "Calculating journey classifications...";
                OnProgress.Invoke( this, new ProgressEventArgs( progressMessage ) );
            }

            /* Get Non-Giver GivingIds */
            var nonGiverGivingIdsQuery = personQuery.Where( p =>
                    !financialTransactionGivingAnalyticsQuery.Any( ft => personAliasQuery.Any( pa => pa.Id == ft.AuthorizedPersonAliasId && pa.Person.GivingId == p.GivingId ) ) );

            var nonGiverGivingIdsList = nonGiverGivingIdsQuery.Select( a => a.GivingId ).Distinct().ToList();

            /* Get TransactionDateList for each GivingId in the system */
            var transactionDateTimes = financialTransactionGivingAnalyticsQuery.Select( a => new
            {
                GivingId = personAliasQuery.Where( pa => pa.Id == a.AuthorizedPersonAliasId ).Select( pa => pa.Person.GivingId ).FirstOrDefault(),
                a.TransactionDateTime
            } ).Where( a => a.GivingId != null ).ToList();

            var transactionDateTimesByGivingId = transactionDateTimes
                    .GroupBy( g => g.GivingId )
                    .Select( s => new
                    {
                        GivingId = s.Key,
                        TransactionDateTimeList = s.Select( x => x.TransactionDateTime ).ToList()
                    } ).ToDictionary( k => k.GivingId, v => v.TransactionDateTimeList );

            List<AttributeCache> journeyStageAttributesList = new List<AttributeCache> { _currentJourneyStageAttribute, _previousJourneyStageAttribute, _journeyStageChangeDateAttribute };
            if ( journeyStageAttributesList.Any( a => a == null ) )
            {
                throw new Exception( "Journey Stage Attributes are not installed correctly." );
            }

            var journeyStageAttributeIds = journeyStageAttributesList.Where( a => a != null ).Select( a => a.Id ).ToList();

            var personCurrentJourneyAttributeValues = new AttributeValueService( rockContext ).Queryable()
                .WhereAttributeIds( journeyStageAttributeIds )
                .Where( av => av.EntityId.HasValue )
                .Join(
                    personQuery.Where( x => !string.IsNullOrEmpty( x.GivingId ) ),
                    av => av.EntityId.Value,
                    p => p.Id,
                    ( av, p ) => new
                    {
                        AttributeId = av.AttributeId,
                        AttributeValue = av.Value,
                        PersonGivingId = p.GivingId,
                        PersonId = p.Id
                    } )
                .GroupBy( a => a.PersonGivingId )
                .Select( a => new
                {
                    GivingId = a.Key,
                    AttributeValues = a.ToList()
                } ).ToDictionary( k => k.GivingId, v => v.AttributeValues );

            var givingJourneySettings = givingAnalyticsSetting.GivingJourneySettings;
            var currentDate = RockDateTime.Today;

            var formerGiverGivingIds = new List<string>();
            var lapsedGiverGivingIds = new List<string>();
            var newGiverGivingIds = new List<string>();
            var occasionalGiverGivingIds = new List<string>();
            var consistentGiverGivingIds = new List<string>();

            var noneOfTheAboveGiverGivingIds = new List<string>();

            foreach ( var givingIdTransactions in transactionDateTimesByGivingId )
            {
                var givingId = givingIdTransactions.Key;
                var transactionDateList = givingIdTransactions.Value.Where( a => a.HasValue ).Select( a => a.Value ).ToList();

                GivingJourneyStage? givingIdGivingJourneyStage = GetGivingJourneyStage( givingJourneySettings, currentDate, transactionDateList );

                switch ( givingIdGivingJourneyStage )
                {
                    case GivingJourneyStage.Former:
                        formerGiverGivingIds.Add( givingId );
                        break;
                    case GivingJourneyStage.Lapsed:
                        lapsedGiverGivingIds.Add( givingId );
                        break;
                    case GivingJourneyStage.New:
                        newGiverGivingIds.Add( givingId );
                        break;
                    case GivingJourneyStage.Occasional:
                        occasionalGiverGivingIds.Add( givingId );
                        break;
                    case GivingJourneyStage.Consistent:
                        consistentGiverGivingIds.Add( givingId );
                        break;
                    case GivingJourneyStage.None:
                        // Shouldn't happen since we are only looking at people with transactions, and we have already
                        // figured out the non-givers
                        break;
                    default:
                        // if they are non of the above, then add them to the "none of the above" list
                        noneOfTheAboveGiverGivingIds.Add( givingId );
                        break;
                }

            }

            Debug.WriteLine( $@"
FormerGiverCount: {formerGiverGivingIds.Count}
LapsedGiverCount: {lapsedGiverGivingIds.Count}
NewGiverCount: {newGiverGivingIds.Count}
OccasionalGiverCount: {occasionalGiverGivingIds.Count}
ConsistentGiverCount: {consistentGiverGivingIds.Count}
NonGiverCount: {nonGiverGivingIdsList.Count}
NoneOfTheAboveCount: {noneOfTheAboveGiverGivingIds.Count}
" );

            _attributeValuesByGivingIdAndPersonId = personCurrentJourneyAttributeValues
                .ToDictionary(
                    k => k.Key,
                    v =>
                    {
                        var lookupByPersonId = v.Value
                            .Select( s => new AttributeValueCache( s.AttributeId, s.PersonId, s.AttributeValue ) )
                            .GroupBy( g => g.EntityId.Value )
                            .ToDictionary( k => k.Key, vv => vv.ToList() );
                        return lookupByPersonId;
                    } );

            _personIdsByGivingId = personQuery.Where( x => !string.IsNullOrEmpty( x.GivingId ) )
                .Select( a => new { a.GivingId, PersonId = a.Id } )
                .GroupBy( a => a.GivingId )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select( p => p.PersonId ).ToList() );

            UpdateJourneyStageAttributeValuesForGivingId( formerGiverGivingIds, GivingJourneyStage.Former );
            UpdateJourneyStageAttributeValuesForGivingId( lapsedGiverGivingIds, GivingJourneyStage.Lapsed );
            UpdateJourneyStageAttributeValuesForGivingId( newGiverGivingIds, GivingJourneyStage.New );
            UpdateJourneyStageAttributeValuesForGivingId( occasionalGiverGivingIds, GivingJourneyStage.Occasional );
            UpdateJourneyStageAttributeValuesForGivingId( consistentGiverGivingIds, GivingJourneyStage.Consistent );
            UpdateJourneyStageAttributeValuesForGivingId( nonGiverGivingIdsList, GivingJourneyStage.None );
            UpdateJourneyStageAttributeValuesForGivingId( noneOfTheAboveGiverGivingIds, null );
        }

        /// <summary>
        /// Gets the giving journey stage.
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="transactionDateList">The transaction date list.</param>
        /// <returns></returns>
        internal static GivingJourneyStage? GetGivingJourneyStage( GivingJourneySettings givingJourneySettings, DateTime currentDate, List<DateTime> transactionDateList )
        {
            if ( !transactionDateList.Any() )
            {
                return GivingJourneyStage.None;
            }

            var mostRecentTransactionDateTime = transactionDateList.Max();
            var firstTransactionDateTime = transactionDateList.Min();
            var daysBetweenList = GetBetweenDatesDays( transactionDateList );
            var medianDaysBetween = GetMedian( daysBetweenList );
            var daysSinceMostRecentTransaction = ( currentDate - mostRecentTransactionDateTime ).TotalDays;
            var daysSinceFirstTransaction = ( currentDate - firstTransactionDateTime ).TotalDays;

            GivingJourneyStage? givingIdGivingJourneyStage;

            if ( IsFormerGiver( givingJourneySettings, medianDaysBetween, daysSinceMostRecentTransaction ) )
            {
                givingIdGivingJourneyStage = GivingJourneyStage.Former;
            }
            else if ( IsLapsedGiver( givingJourneySettings, medianDaysBetween, daysSinceMostRecentTransaction ) )
            {
                givingIdGivingJourneyStage = GivingJourneyStage.Lapsed;
            }
            else if ( IsNewGiver( givingJourneySettings, transactionDateList.Count, daysSinceFirstTransaction ) )
            {
                givingIdGivingJourneyStage = GivingJourneyStage.New;
            }
            else if ( IsOccasionalGiver( givingJourneySettings, medianDaysBetween ) )
            {
                givingIdGivingJourneyStage = GivingJourneyStage.Occasional;
            }
            else if ( IsConsistentGiver( givingJourneySettings, medianDaysBetween ) )
            {
                givingIdGivingJourneyStage = GivingJourneyStage.Consistent;
            }
            else
            {
                givingIdGivingJourneyStage = null;
            }

            return givingIdGivingJourneyStage;
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Updates the journey stage attribute values for giving identifier.
        /// </summary>
        /// <param name="givingIds">The giving ids.</param>
        /// <param name="calculatedGivingJourneyStage">The calculated giving journey stage.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private void UpdateJourneyStageAttributeValuesForGivingId( List<string> givingIds, GivingJourneyStage? calculatedGivingJourneyStage )
        {
            int numberOfGivingUnitsChanged = 0;

            if ( OnProgress != null )
            {
                string progressMessage = "Updating to " + ( calculatedGivingJourneyStage.HasValue ? calculatedGivingJourneyStage.Value.ConvertToString() : "unclassified" );
                OnProgress.Invoke( this, new ProgressEventArgs( progressMessage ) );
            }

            List<AttributeCache> journeyStageAttributesList = new List<AttributeCache> { _currentJourneyStageAttribute, _previousJourneyStageAttribute, _journeyStageChangeDateAttribute };

            var currentDate = RockDateTime.Today;
            var numberOfGivingUnits = givingIds.Count();
            var progressMax = numberOfGivingUnits;

            DateTime lastGivingJourneyHelperProgressUpdate = DateTime.MinValue;

            var progressPosition = 0;
            foreach ( var givingId in givingIds )
            {
                var attributeValuesByPersonId = _attributeValuesByGivingIdAndPersonId.GetValueOrNull( givingId );
                var personIdsWithGivingId = _personIdsByGivingId.GetValueOrNull( givingId ) ?? new List<int>();
                progressPosition++;

                foreach ( var personId in personIdsWithGivingId )
                {
                    var attributeValuesByAttributeId = attributeValuesByPersonId?.GetValueOrNull( personId )?.GroupBy( k => k.AttributeId ).ToDictionary( k => k.Key, v => v.FirstOrDefault() );

                    var currentJourneyStage = ( GivingJourneyStage? ) attributeValuesByAttributeId?.GetValueOrNull( _currentJourneyStageAttribute.Id )?.Value.AsIntegerOrNull();

                    if ( currentJourneyStage == null && calculatedGivingJourneyStage == null )
                    {
                        // They were unclassified and are still unclassified
                        // Go to the next person
                        continue;
                    }

                    bool changedFromUnclassifiedToClassified = !currentJourneyStage.HasValue && calculatedGivingJourneyStage.HasValue;
                    bool changedToUnclassified = currentJourneyStage.HasValue && !calculatedGivingJourneyStage.HasValue;
                    bool classifiedStageChanged = currentJourneyStage.HasValue && calculatedGivingJourneyStage.HasValue && calculatedGivingJourneyStage.Value != currentJourneyStage.Value;
                    bool stageChanged = changedFromUnclassifiedToClassified || changedToUnclassified || classifiedStageChanged;

                    // if the calculated journey stage has changed, get the Person and update the attribute values
                    if ( stageChanged )
                    {
                        using ( var rockContext = new RockContext() )
                        {
#if REVIEW_NET5_0_OR_GREATER
                            rockContext.Database.SetCommandTimeout( this.SqlCommandTimeout );
#else
                            rockContext.Database.CommandTimeout = this.SqlCommandTimeout;
#endif
                            var personService = new PersonService( rockContext );

                            var person = personService.Get( personId );
                            Rock.Attribute.Helper.LoadAttributes( person, journeyStageAttributesList.ToList() );
                            if ( currentJourneyStage.HasValue )
                            {
                                person.SetAttributeValue( _previousJourneyStageAttribute.Key, currentJourneyStage.ConvertToInt() );
                            }
                            else
                            {
                                person.SetAttributeValue( _previousJourneyStageAttribute.Key, ( int? ) null );
                            }
                            
                            person.SetAttributeValue( _journeyStageChangeDateAttribute.Key, currentDate );

                            if ( calculatedGivingJourneyStage.HasValue )
                            {
                                person.SetAttributeValue( _currentJourneyStageAttribute.Key, calculatedGivingJourneyStage.ConvertToInt() );
                            }
                            else
                            {
                                person.SetAttributeValue( _currentJourneyStageAttribute.Key, ( int? ) null );
                            }

                            person.SaveAttributeValues( rockContext );
                            numberOfGivingUnitsChanged++;
                        }
                    }
                }

                if ( OnProgress != null )
                {
                    if ( ( RockDateTime.Now - lastGivingJourneyHelperProgressUpdate ).TotalSeconds >= 3 || ( progressMax - progressPosition < 100 ) )
                    {
                        var onProgressEventArgs = new ProgressEventArgs( calculatedGivingJourneyStage, numberOfGivingUnits, progressMax, progressPosition );
                        OnProgress.Invoke( this, onProgressEventArgs );
                        lastGivingJourneyHelperProgressUpdate = RockDateTime.Now;
                    }
                }
            }

            if ( calculatedGivingJourneyStage.HasValue )
            {
                _givingJourneyChangeCount.AddOrReplace( calculatedGivingJourneyStage.Value, numberOfGivingUnitsChanged );
            }
            else
            {
                _givingJourneyNoneOfTheAboveCount = numberOfGivingUnitsChanged;
            }
        }

        /// <summary>
        /// Determines whether [is consistent giver] [the specified giving journey settings].
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="medianDaysBetween">The median days between.</param>
        /// <returns><c>true</c> if [is consistent giver] [the specified giving journey settings]; otherwise, <c>false</c>.</returns>
        private static bool IsConsistentGiver( GivingJourneySettings givingJourneySettings, int? medianDaysBetween )
        {
            if ( !medianDaysBetween.HasValue )
            {
                return false;
            }

            if ( !givingJourneySettings.ConsistentGiverMedianLessThanDays.HasValue )
            {
                // not configured
                return false;
            }

            if ( medianDaysBetween < givingJourneySettings.ConsistentGiverMedianLessThanDays )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is occasional giver] [the specified giving journey settings].
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="medianDaysBetween">The median days between.</param>
        /// <returns><c>true</c> if [is occasional giver] [the specified giving journey settings]; otherwise, <c>false</c>.</returns>
        private static bool IsOccasionalGiver( GivingJourneySettings givingJourneySettings, int? medianDaysBetween )
        {
            if ( !medianDaysBetween.HasValue )
            {
                return false;
            }

            if ( !givingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum.HasValue || !givingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum.HasValue )
            {
                // not configured
                return false;
            }

            var medianDaysMin = givingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum.Value;
            var medianDaysMax = givingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum.Value;

            if ( medianDaysBetween.Value >= medianDaysMin && medianDaysBetween.Value <= medianDaysMax )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is former giver] [the specified giving journey settings].
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="medianDaysBetween">The median days between.</param>
        /// <param name="daysSinceMostRecentTransaction">The days since most recent transaction.</param>
        /// <returns><c>true</c> if [is former giver] [the specified giving journey settings]; otherwise, <c>false</c>.</returns>
        private static bool IsFormerGiver( GivingJourneySettings givingJourneySettings, int? medianDaysBetween, double daysSinceMostRecentTransaction )
        {
            bool isFormerGiver = false;

            if ( givingJourneySettings.FormerGiverNoContributionInTheLastDays.HasValue )
            {
                isFormerGiver = daysSinceMostRecentTransaction >= givingJourneySettings.FormerGiverNoContributionInTheLastDays.Value;

                if ( givingJourneySettings.FormerGiverMedianFrequencyLessThanDays.HasValue && medianDaysBetween.HasValue )
                {
                    isFormerGiver = isFormerGiver && ( medianDaysBetween < givingJourneySettings.FormerGiverMedianFrequencyLessThanDays.Value );
                }
            }

            return isFormerGiver;
        }

        /// <summary>
        /// Determines whether [is lapsed giver] [the specified giving journey settings].
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="medianDaysBetween">The median days between.</param>
        /// <param name="daysSinceMostRecentTransaction">The days since most recent transaction.</param>
        /// <returns><c>true</c> if [is lapsed giver] [the specified giving journey settings]; otherwise, <c>false</c>.</returns>
        private static bool IsLapsedGiver( GivingJourneySettings givingJourneySettings, int? medianDaysBetween, double daysSinceMostRecentTransaction )
        {
            bool isLapsedGiver = false;

            if ( givingJourneySettings.LapsedGiverNoContributionInTheLastDays.HasValue )
            {
                isLapsedGiver = daysSinceMostRecentTransaction > givingJourneySettings.LapsedGiverNoContributionInTheLastDays.Value;

                if ( givingJourneySettings.LapsedGiverMedianFrequencyLessThanDays.HasValue && medianDaysBetween.HasValue )
                {
                    isLapsedGiver = isLapsedGiver && ( medianDaysBetween < givingJourneySettings.LapsedGiverMedianFrequencyLessThanDays.Value );
                }
            }

            return isLapsedGiver;
        }

        /// <summary>
        /// Determines whether [is new giver] [the specified giving journey settings].
        /// </summary>
        /// <param name="givingJourneySettings">The giving journey settings.</param>
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="daysSinceFirstTransaction">The days since first transaction.</param>
        /// <returns><c>true</c> if [is new giver] [the specified giving journey settings]; otherwise, <c>false</c>.</returns>
        private static bool IsNewGiver( GivingJourneySettings givingJourneySettings, int transactionCount, double daysSinceFirstTransaction )
        {
            if ( !givingJourneySettings.NewGiverContributionCountBetweenMinimum.HasValue || !givingJourneySettings.NewGiverContributionCountBetweenMaximum.HasValue || !givingJourneySettings.NewGiverFirstGiftInTheLastDays.HasValue )
            {
                // not configured
                return false;
            }

            if ( daysSinceFirstTransaction > givingJourneySettings.NewGiverFirstGiftInTheLastDays.Value )
            {
                // gave more than NewGiverFirstGiftInTheLastDays ago
                return false;
            }

            var minCount = givingJourneySettings.NewGiverContributionCountBetweenMinimum.Value;
            var maxCount = givingJourneySettings.NewGiverContributionCountBetweenMaximum.Value;

            if ( transactionCount >= minCount && transactionCount <= maxCount )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the median.
        /// From https://stackoverflow.com/a/5275324/1755417
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        private static int? GetMedian( IEnumerable<int> valueList )
        {
            var sortedValuesArray = valueList.ToArray();
            Array.Sort( sortedValuesArray );

            int count = sortedValuesArray.Length;
            if ( count == 0 )
            {
                return null;
            }
            else if ( count % 2 == 0 )
            {
                // count is even, average two middle elements
                int medianValue1 = sortedValuesArray[( count / 2 ) - 1];
                int medianValue2 = sortedValuesArray[count / 2];
                var averageOfMiddleElements = ( medianValue1 + medianValue2 ) / 2m;
                return ( int ) averageOfMiddleElements;
            }
            else
            {
                // count is odd, return the middle element
                return sortedValuesArray[count / 2];
            }
        }

        /// <summary>
        /// Gets the between dates days.
        /// </summary>
        /// <param name="transactionDateList">The transaction date list.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GetBetweenDatesDays( List<DateTime> transactionDateList )
        {
            var daysSinceLastTransaction = new List<int>();

            if ( !transactionDateList.Any() )
            {
                return daysSinceLastTransaction;
            }

            var transactionDateListOrderByDate = transactionDateList.OrderBy( a => a ).ToArray();

            var previousTransactionDate = transactionDateListOrderByDate[0];

            foreach ( var transactionDate in transactionDateListOrderByDate )
            {
                var timeSpanSince = transactionDate - previousTransactionDate;
                var daysSince = ( int ) Math.Round( timeSpanSince.TotalDays, 0 );

                previousTransactionDate = transactionDate;

                if ( daysSince == 0 )
                {
                    // if they gave more than one time in a day, only count the daysSince for one of them
                    continue;
                }

                daysSinceLastTransaction.Add( daysSince );
            }

            return daysSinceLastTransaction;
        }

        #endregion Private Methods

        #region Class specific classes

        /// <summary>
        /// Class ProgressEventArgs.
        /// </summary>
        internal class ProgressEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
            /// </summary>
            /// <param name="givingJourneyStage">The giving journey stage.</param>
            /// <param name="numberOfGivingUnits">The number of giving units.</param>
            /// <param name="progressMax">The progress maximum.</param>
            /// <param name="progressPosition">The progress position.</param>
            internal ProgressEventArgs( GivingJourneyStage? givingJourneyStage, int numberOfGivingUnits, int progressMax, int progressPosition )
            {
                this.GivingJourneyStage = givingJourneyStage;
                this.NumberOfGivingUnits = numberOfGivingUnits;
                this.ProgressMax = progressMax;
                this.ProgressPosition = progressPosition;
                if ( givingJourneyStage.HasValue )
                {
                    this.ProgressMessage = $"Updating people with journey stage: {givingJourneyStage.ConvertToString()} ( {ProgressPosition}/{ProgressMax} )";
                }
                else
                {
                    this.ProgressMessage = $"Updating people with un-classified journey stage: {ProgressPosition}/{ProgressMax}";
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
            /// </summary>
            /// <param name="progressMessage">The progress message.</param>
            internal ProgressEventArgs( string progressMessage )
            {
                this.ProgressMessage = progressMessage;
            }

            /// <summary>
            /// The progress message
            /// </summary>
            internal readonly string ProgressMessage;

            /// <summary>
            /// The giving journey stage
            /// </summary>
            internal readonly GivingJourneyStage? GivingJourneyStage;

            /// <summary>
            /// The number of giving units
            /// </summary>
            internal readonly int NumberOfGivingUnits;

            /// <summary>
            /// The progress maximum
            /// </summary>
            internal readonly int ProgressMax;

            /// <summary>
            /// The progress position
            /// </summary>
            internal readonly int ProgressPosition;
        }

        #endregion Class specific classes
    }

    /// <summary>
    /// Enum GivingJourneyStage
    /// </summary>
    public enum GivingJourneyStage
    {
        /// <summary>
        /// Non-Giver
        /// </summary>
        None = 0,

        /// <summary>
        /// New giver.
        /// </summary>
        New = 1,

        /// <summary>
        /// Occasional giver
        /// </summary>
        Occasional = 2,

        /// <summary>
        /// Consistent giver
        /// </summary>
        Consistent = 3,

        /// <summary>
        /// Lapsed giver
        /// </summary>
        Lapsed = 4,

        /// <summary>
        /// Former giver
        /// </summary>
        Former = 5
    }
}
