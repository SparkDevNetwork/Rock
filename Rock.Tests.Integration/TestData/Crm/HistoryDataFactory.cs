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
using System.Diagnostics;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData
{
    public class HistoryDataFactory //: SampleDataFactoryBase
    {
        private const string _TestDataSourceOfChange = "HistoryDataFactory";
        private const int _HistoryTestMaxPeople = 100;
        private readonly DateTime _HistoryTestEndDate = RockDateTime.Now;

        #region Static Methods

        public static void AddSampleData( bool removeExistingData = false )
        {
            var factory = new HistoryDataFactory();

            if ( removeExistingData )
            {
                RemoveSampleData();
            }

            factory.AddSampleDataForConnectionStatusChangeHistory();
        }

        public static void RemoveSampleData()
        {
            var factory = new HistoryDataFactory();

            factory.RemoveTestDataFromCurrentDatabase();
        }

        #endregion

        /// <summary>
        /// Removes the test data from the current database.
        /// </summary>
        public void RemoveTestDataFromCurrentDatabase()
        {
            var dataContext = new RockContext();

            // History table has no dependencies, so use SQL delete for efficiency.
            var recordsDeleted = dataContext.Database.ExecuteSqlCommand( "delete from [History] where [SourceOfChange] = @p0 ", _TestDataSourceOfChange );

            Debug.Print( $"Delete History: {recordsDeleted} records deleted." );
        }

        /// <summary>
        /// Adds a predictable set of Connection Status changes to the test database that are used for integration testing.
        /// </summary>
        public void AddSampleDataForConnectionStatusChangeHistory()
        {
            var dataContext = new RockContext();

            //this.ThrowIfTestHistoryDataExists( dataContext, _TestDataSourceOfChange );

            var memberConnectionStatusId = GetStatusValueIdOrThrow( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER );
            var attenderConnectionStatusId = GetStatusValueIdOrThrow( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE );
            var visitorConnectionStatusId = GetStatusValueIdOrThrow( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR );
            var prospectConnectionStatusId = GetStatusValueIdOrThrow( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT );

            var personService = new PersonService( dataContext );

            var personList = personService.Queryable()
                .AsNoTracking()
                .Where( x => !x.IsSystem )
                .Take( _HistoryTestMaxPeople )
                .ToList();

            var currentDate = _HistoryTestEndDate;

            var rng = new Random();

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            // Create new change history entries for each person.
            int entriesAdded = 0;
            History.HistoryChange historyEntry;

            foreach ( var person in personList )
            {
                var historyChanges = new History.HistoryChangeList();

                var dateOfChange = currentDate;

                if ( person.ConnectionStatusValueId == memberConnectionStatusId )
                {
                    // For each Person who has a Connection Status of Member, add prior change events for Visitor --> Attendee --> Member.

                    // Add a change for Attendee --> Member.
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Attendee" )
                        .SetNewValue( "Member" )
                        .SetRawValues( attenderConnectionStatusId.ToString(), memberConnectionStatusId.ToString() )
                        .SetDateOfChange( dateOfChange );

                    // Add a change for Visitor --> Attendee.
                    // This change record has only the OldValue and NewValue, compatible with records created prior to Rock v1.8
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Visitor" )
                        .SetNewValue( "Attendee" )
                        .SetDateOfChange( dateOfChange );

                    entriesAdded += 2;
                }
                else if ( person.ConnectionStatusValueId == visitorConnectionStatusId )
                {
                    // For each Person who has a Connection Status of Visitor, add prior change events for Web Prospect --> Visitor.
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Web Prospect" )
                        .SetNewValue( "Visitor" )
                        .SetRawValues( prospectConnectionStatusId.ToString(), visitorConnectionStatusId.ToString() )
                        .SetDateOfChange( dateOfChange );

                    entriesAdded += 1;
                }

                Debug.Print( $"Processed History Entries... [PersonId={person.Id}, Date={dateOfChange}]" );

                HistoryService.SaveChanges( dataContext, typeof( Person ), global::Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, historyChanges, true, adminPerson.PrimaryAliasId, _TestDataSourceOfChange );
            }

            Debug.Print( $"Create Data completed: { entriesAdded } history entries created." );
        }

        /// <summary>
        /// Adds a randomized set of Connection Status changes to the test database.
        /// This is only required for performance testing.
        /// </summary>
        public void AddBulkConnectionStatusChangeHistory()
        {
            var dataContext = new RockContext();

            //this.ThrowIfTestHistoryDataExists( dataContext, _TestDataSourceOfChange );

            // Add Connection Status Change History Entries.
            var personService = new PersonService( dataContext );

            var personQuery = personService.Queryable().AsNoTracking().Where( x => !x.IsSystem )
                .Take( 100 ).ToList();

            var statusType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS );

            var statusValues = statusType.DefinedValues.Select( x => x.Id ).ToList();

            int? toStatusId;
            int? fromStatusId;

            var currentDate = _HistoryTestEndDate;

            var rng = new Random();

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            // For each person, add a random number of status changes over a random period of time prior to today.
            int entriesAdded = 0;

            foreach ( var person in personQuery )
            {
                var historyChanges = new History.HistoryChangeList();

                var numberOfChanges = rng.Next( 0, 3 );

                var dateOfChange = currentDate;

                // Set the initial value to the current status.
                fromStatusId = person.ConnectionStatusValueId;

                for ( int i = 1; i <= numberOfChanges; i++ )
                {
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    // Set the target status for this change to be the status prior to the most recent status change.
                    toStatusId = fromStatusId;

                    do
                    {
                        fromStatusId = statusValues.GetRandomElement();
                    }
                    while ( toStatusId == fromStatusId );

                    var historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetNewValue( toStatusId.ToStringSafe() )
                        .SetOldValue( fromStatusId.ToStringSafe() )
                        .SetRawValues( fromStatusId.ToStringSafe(), toStatusId.ToStringSafe() )
                        .SetDateOfChange( dateOfChange );

                    Debug.Print( $"Added History Entry [PersonId={person.Id}, Date={dateOfChange}, OldValue={fromStatusId}, NewValue={toStatusId}" );

                    entriesAdded++;
                }

                HistoryService.SaveChanges( dataContext, typeof( Person ), global::Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, historyChanges, true, adminPerson.PrimaryAliasId, _TestDataSourceOfChange );
            }

            Debug.Print( $"Create Data completed: { entriesAdded } history entries created." );
        }

        /// <summary>
        /// Check if test records already exist in the current database.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="sourceOfChangeIdentifier"></param>
        //private void ThrowIfTestHistoryDataExists( RockContext dataContext, string sourceOfChangeIdentifier )
        //{
        //    var historyService = new HistoryService( dataContext );

        //    var testEntryExists = historyService.Queryable().AsNoTracking().Any( x => x.SourceOfChange == sourceOfChangeIdentifier );

        //    if ( testEntryExists )
        //    {
        //        throw new Exception( $"History Test Data already exists in this database. [SourceOfChange ={ sourceOfChangeIdentifier }]" );
        //    }
        //}

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        /// <summary>
        /// Returns a Connection Status DefinedValueId by name or throws an exception.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        private int GetStatusValueIdOrThrow( string reference )
        {
            var statusType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS );

            var guid = reference.AsGuidOrNull();

            DefinedValueCache connectionStatusType;

            if ( guid != null )
            {
                connectionStatusType = statusType.DefinedValues.FirstOrDefault( x => x.Guid == guid );
            }
            else
            {
                connectionStatusType = statusType.DefinedValues.FirstOrDefault( x => x.Value == reference );
            }

            if ( connectionStatusType == null )
            {
                throw new Exception( $"Connection Status Type not found. [Reference={reference}]" );
            }

            return connectionStatusType.Id;
        }

        //#region SampleDataFactoryBase implementation

        //public override List<SampleDataChangeAction> GetActionList()
        //{
        //    var actions = new List<SampleDataChangeAction>();

        //    AddNewChangeAction( actions, "Add Standard Sample Data", "Add the standard sample data set for Person History." );
        //    AddNewChangeAction( actions, "Remove Standard Sample Data", "Remove the standard sample data set for Person History.", "Delete" );

        //    return actions;
        //}

        //private void AddNewChangeAction( List<SampleDataChangeAction> actions, string name, string description = null, string action = "Add" )
        //{
        //    actions.Add( new SampleDataChangeAction { Category = "Person History", Key = name, Name = name, Description = description, Action = action } );
        //}

        //protected override SampleDataChangeActionExecutionResponse OnExecuteAction( string actionId, ISampleDataChangeActionSettings settings, ITaskMonitorHandle monitor )
        //{
        //    if ( actionId == "Add Standard Sample Data" )
        //    {
        //        AddSampleData();
        //    }
        //    else if ( actionId == "Remove Standard Sample Data" )
        //    {
        //        RemoveSampleData();
        //    }

        //    var response = new SampleDataChangeActionExecutionResponse();

        //    return response;
        //}

        //#endregion
    }
}