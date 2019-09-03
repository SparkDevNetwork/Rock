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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock;
using Rock.Web.Cache;

namespace Rock.Crm.ConnectionStatusChangeReport
{
    /// <summary>
    /// Creates a report that summarizes Person connection status changes for a specified period.
    /// </summary>
    public class ConnectionStatusChangeReportBuilder
    {
        private readonly RockContext _DataContext;
        private ConnectionStatusChangeReportSettings _Settings = new ConnectionStatusChangeReportSettings();

        private readonly DateTime _DefaultDateCreated = RockDateTime.Now;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStatusChangeReportBuilder"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="settings">The settings.</param>
        public ConnectionStatusChangeReportBuilder( RockContext dataContext, ConnectionStatusChangeReportSettings settings )
        {
            _DataContext = dataContext;

            this.Settings = settings;

            this.BuildCampusLookupMaps();
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// Gets or sets the report settings.
        /// </summary>
        public ConnectionStatusChangeReportSettings Settings
        {
            get
            {
                return _Settings;
            }

            set
            {
                _Settings = value ?? new ConnectionStatusChangeReportSettings();
            }
        }

        /// <summary>
        /// Gets the report data.
        /// </summary>
        /// <returns></returns>
        public ConnectionStatusChangeReportData CreateReport()
        {
            var currentDate = RockDateTime.Now;

            var report = new ConnectionStatusChangeReportData();

            report.Settings = this.Settings;

            // Get change events.
            var dateRange = _Settings.ReportPeriod.GetDateRange();

            var changeEvents = this.GetChangeEventsForPeriod( _Settings.CampusId, dateRange.Start, dateRange.End, _Settings.FromConnectionStatusId, _Settings.ToConnectionStatusId );

            // If an Original Status is not specified, add an event for people added during the reporting period.
            // This event captures their initial status change: (null) --> (new status).
            if ( _Settings.FromConnectionStatusId == null )
            {
                var newPersonChangeEvents = GetChangeEventsForNewPeople( _Settings.CampusId, dateRange.Start, dateRange.End, _Settings.ToConnectionStatusId );

                changeEvents.AddRange( newPersonChangeEvents );
            }

            // Determine the reporting period. If the start or end date have not been specified, use the first or last date of campus memberships.
            DateTime startDateCompare;

            if ( dateRange.Start == null )
            {
                if ( changeEvents.Count > 0 )
                {
                    startDateCompare = changeEvents.Min( x => x.EventDate );
                }
                else
                {
                    startDateCompare = currentDate;
                }
            }
            else
            {
                startDateCompare = dateRange.Start.Value;
            }

            startDateCompare = startDateCompare.Date;

            var endDateCompare = dateRange.End;

            if ( endDateCompare == null )
            {
                endDateCompare = currentDate;
            }

            endDateCompare = endDateCompare.Value.AddDays( 1 ).AddMilliseconds( -1 );

            report.StartDate = startDateCompare;
            report.EndDate = endDateCompare.Value;

            report.ChangeEvents = changeEvents;

            return report;
        }

        #endregion

        /// <summary>
        /// Create a set of change events for new people added during the reporting period.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="reportStartDate">The report start date.</param>
        /// <param name="reportEndDate">The report end date.</param>
        /// <param name="toConnectionStatusId">To connection status identifier.</param>
        /// <returns></returns>
        private List<ConnectionStatusChangeEventInfo> GetChangeEventsForNewPeople( int? campusId, DateTime? reportStartDate, DateTime? reportEndDate, int? toConnectionStatusId )
        {
            // Add Change Events for new people added within the reporting period.
            var personService = new PersonService( _DataContext );

            var personQuery = personService.Queryable()
                                           .AsNoTracking()
                                           .Where(x => !x.IsSystem );

            if ( campusId != null )
            {
                personQuery = personQuery.Where( x => x.PrimaryCampusId == campusId );
            }

            if ( reportStartDate != null )
            {
                personQuery = personQuery.Where( x => x.CreatedDateTime >= reportStartDate );
            }

            if ( reportEndDate != null )
            {
                personQuery = personQuery.Where( x => x.CreatedDateTime < reportEndDate );
            }

            // If the Updated Connection Status filter is specified, only create events for people with that status.
            if ( toConnectionStatusId != null )
            {
                personQuery = personQuery.Where( x => x.ConnectionStatusValueId == toConnectionStatusId );
            }

            var eventsData = personQuery
                .Select( x => new ConnectionStatusChangeEventData
                {
                    PersonId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    CampusId = x.PrimaryCampusId,
                    OldRawData = string.Empty,
                    NewRawData = (x.ConnectionStatusValueId == null ? string.Empty : x.ConnectionStatusValueId.ToString() ),
                    EventDate = x.CreatedDateTime ?? DateTime.MinValue,
                    CreatedBy = x.CreatedByPersonAlias.Name
                } ).ToList();

            
            SetCalculatedDataFields( eventsData );

            var events = eventsData.Cast<ConnectionStatusChangeEventInfo>().ToList();

            return events;
        }

        /// <summary>
        /// Get a Person query with basic report filters applied.
        /// </summary>
        /// <param name="campusId"></param>
        /// <returns></returns>
        private IQueryable<Person> GetPersonQueryBase( int? campusId )
        {
            // Get Person Query
            var personService = new PersonService( _DataContext );

            var personQuery = personService.Queryable();

            // Filter by Campus
            if ( campusId.GetValueOrDefault(0) != 0 )
            {
                personQuery = personQuery.Where( x => x.PrimaryCampusId != null && x.PrimaryCampusId == campusId );
            }

            return personQuery;
        }

        /// <summary>
        /// Get a History query related to Person.ConnectionStatus property change.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="originalConnectionStatusId">The original connection status identifier.</param>
        /// <param name="updatedConnectionStatusId">The updated connection status identifier.</param>
        /// <returns></returns>
        private IQueryable<History> GetHistoryQueryBase( DateTime? startDate, DateTime? endDate, int? originalConnectionStatusId, int? updatedConnectionStatusId )
        {
            var historyService = new HistoryService( _DataContext );

            var personEntityTypeId = EntityTypeCache.GetId<Person>();

            // Get History records related to Person.ConnectionStatus property change.
            var historyEntriesBaseQuery = historyService.Queryable()
                .AsNoTracking()
                .Where( x => x.EntityTypeId == personEntityTypeId
                        && x.ChangeType == "Property"
                        && x.ValueName == "Connection Status" );

            // Filter by Date
            // Note: History.CreatedDateTime is used to store the occurence date of the event.
            //       This is inconsistent with the primary use of this field as an audit field.
            //       The data model should be adjusted by moving this data to a new field "EventDateTime".
            if ( startDate != null )
            {
                var compareStartDate = startDate.Value;

                historyEntriesBaseQuery = historyEntriesBaseQuery.Where( x => x.CreatedDateTime > compareStartDate );
            }

            if ( endDate != null )
            {
                var compareEndDate = endDate.Value.AddDays( 1 ).AddMilliseconds( -1 );

                historyEntriesBaseQuery = historyEntriesBaseQuery.Where( x => x.CreatedDateTime < compareEndDate );
            }

            // Filter by Original Connection Status.
            if ( originalConnectionStatusId.GetValueOrDefault(0) != 0 )
            {
                // Attempt to find a match by StatusId in OldRawValue (Rock v1.9 or higher), or by name in OldValue (Rock v1.8 or lower).
                var statusName = this.GetConnectionStatusNameOrDefault( originalConnectionStatusId.GetValueOrDefault( 0 ), string.Empty );

                historyEntriesBaseQuery = historyEntriesBaseQuery.Where( x => x.OldRawValue == originalConnectionStatusId.ToString() || x.OldValue == statusName );

            }

            // Filter by Updated Connection Status.
            if ( updatedConnectionStatusId.GetValueOrDefault(0) != 0 )
            {
                // Attempt to find a match by StatusId in OldRawValue (Rock v1.9 or higher), or by name in OldValue (Rock v1.8 or lower).
                var statusName = this.GetConnectionStatusNameOrDefault( updatedConnectionStatusId.GetValueOrDefault( 0 ), string.Empty );

                historyEntriesBaseQuery = historyEntriesBaseQuery.Where( x => x.NewRawValue == updatedConnectionStatusId.ToString() || x.NewValue == statusName );
            }

            return historyEntriesBaseQuery;
        }

        /// <summary>
        /// Create campus membership events for a specified period of time.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="fromConnectionStatusId">From connection status identifier.</param>
        /// <param name="toConnectionStatusId">To connection status identifier.</param>
        /// <returns></returns>
        private List<ConnectionStatusChangeEventInfo> GetChangeEventsForPeriod( int? campusId, DateTime? startDate, DateTime? endDate, int? fromConnectionStatusId, int? toConnectionStatusId )
        {
            // Get base queries.
            var historyEntriesBaseQuery = this.GetHistoryQueryBase( startDate, endDate, fromConnectionStatusId, toConnectionStatusId );
            var personQuery = this.GetPersonQueryBase( campusId );

            // Get the set of events matching the filters.
            var eventsQuery = historyEntriesBaseQuery.Join( personQuery, h => h.EntityId, p => p.Id, ( h, x ) => new ConnectionStatusChangeEventData
            {
                Id = h.Id,
                PersonId = x.Id,
                EventDate = h.CreatedDateTime ?? _DefaultDateCreated,
                FirstName = x.FirstName == null || x.FirstName.Length == 0 ? "-" : x.FirstName,
                LastName = x.LastName == null || x.LastName.Length == 0 ? "-" : x.LastName,
                CampusId = x.PrimaryCampusId,
                CreatedBy = h.CreatedByPersonAlias.Person.NickName + " " + h.CreatedByPersonAlias.Person.LastName,
                OldRawData = h.OldRawValue,
                NewRawData = h.NewRawValue,
                OldValue = h.OldValue,
                NewValue = h.NewValue
            } );

            var eventsData = eventsQuery.ToList();

            SetCalculatedDataFields( eventsData );

            var events = eventsData.Cast<ConnectionStatusChangeEventInfo>().ToList();

            return events;
        }

        private void SetCalculatedDataFields( List<ConnectionStatusChangeEventData> events )
        {
            foreach ( var changeEvent in events )
            {
                // Set Connection Status fields.
                changeEvent.NewConnectionStatusId = changeEvent.NewRawData.AsInteger();
                changeEvent.OldConnectionStatusId = changeEvent.OldRawData.AsInteger();

                changeEvent.NewConnectionStatusName = this.GetConnectionStatusNameOrDefault( changeEvent.NewConnectionStatusId, changeEvent.NewValue );
                changeEvent.OldConnectionStatusName = this.GetConnectionStatusNameOrDefault( changeEvent.OldConnectionStatusId.GetValueOrDefault( 0 ), changeEvent.OldValue );

                // Set Campus Name.
                changeEvent.CampusName = this.GetCampusNameById( changeEvent.CampusId );
            }
        }

        /// <summary>
        /// Add the Campus Name to status change events.
        /// </summary>
        /// <param name="events"></param>
        private void SetCampusName( List<ConnectionStatusChangeEventInfo> events )
        {
            foreach ( var campusEvent in events )
            {
                campusEvent.CampusName = this.GetCampusNameById( campusEvent.CampusId );
            }
        }

        /// <summary>
        /// Get the name of a connection status by Id, or return a default name.
        /// </summary>
        /// <param name="statusId"></param>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        private string GetConnectionStatusNameOrDefault( int statusId, string defaultName )
        {
            var cachedValue = DefinedValueCache.Get( statusId );

            if ( cachedValue == null )
            {
                return defaultName;
            }
            else
            {
                return cachedValue.Value;
            }
        }

        #region Campus Names Lookup

        private Dictionary<string, int> _CampusNameToIdMap = null;
        private Dictionary<int, string> _CampusIdToNameMap = null;

        private void BuildCampusLookupMaps()
        {
            if ( _CampusNameToIdMap == null || _CampusIdToNameMap == null )
            {
                _CampusNameToIdMap = new Dictionary<string, int>();
                _CampusIdToNameMap = new Dictionary<int, string>();

                foreach ( var campus in CampusCache.All( includeInactive: true ) )
                {
                    _CampusNameToIdMap.Add( campus.Name.ToLower(), campus.Id );

                    _CampusIdToNameMap.Add( campus.Id, campus.Name );
                }
            }
        }

        private string GetCampusNameById( int? campusId )
        {
            if ( campusId != null )
            {
                if ( _CampusIdToNameMap.ContainsKey( campusId.Value ) )
                {
                    return _CampusIdToNameMap[campusId.Value];
                }
            }

            return string.Empty;
        }

        #endregion

        #region Support Classes

        private class ConnectionStatusChangeEventData : ConnectionStatusChangeEventInfo
        {
            // Stores the ConnectionStatusValue.Id of the old and new statuses.
            // This data is available for entries created with Rock v1.9 and above.
            public string OldRawData { get; set; }
            public string NewRawData { get; set; }

            // Stores the ConnectionStatusValue.Value of the old and new statuses.
            // This data is used as a fallback if the RawData fields cannot be mapped to a value.
            public string OldValue { get; set; }
            public string NewValue { get; set; }
        }

        #endregion

    }
}

