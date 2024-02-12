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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Tests.Shared;
using Rock.Tests.Integration.Crm.Prayer;
using Rock.Tests.Integration.Crm.Steps;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        #region Person Data

        /// <summary>
        /// Get a test person record.
        /// </summary>
        /// <param name="personGuid">A Guid string, select from the set of values defined in TestGuids.TestPeople.</param>
        /// <returns></returns>
        public static Person GetTestPerson( string personGuid )
        {
            var rockContext = new RockContext();

            var guid = new Guid( personGuid );

            var person = new PersonService( rockContext ).Queryable()
                .FirstOrDefault( x => x.Guid == guid );

            Assert.That.IsNotNull( person, "Test person not found in current database." );

            return person;
        }

        private static List<PersonIdPersonAliasId> _PersonIdToAliasIdMap = null;

        /// <summary>
        /// Gets a list of the Id and AliasId of all Person records in the database
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static List<PersonIdPersonAliasId> GetPersonIdWithAliasIdList()
        {
            if ( _PersonIdToAliasIdMap == null )
            {
                var aliasService = new PersonAliasService( new RockContext() );

                _PersonIdToAliasIdMap = aliasService.Queryable()
                    .Where( x => !x.Person.IsSystem )
                    .GroupBy( x => x.PersonId )
                    .Select( a => new PersonIdPersonAliasId
                    {
                        PersonId = a.Key,
                        PersonAliasId = a.FirstOrDefault().Id
                    } )
                    .ToList();
            }

            return _PersonIdToAliasIdMap;
        }

        /// <summary>
        /// A hardcoded Device Id of 2 (Main Campus Checkin)
        /// </summary>
        public const int KioskDeviceId = 2;

        public class PersonIdPersonAliasId
        {
            public int PersonId { get; set; }
            public int PersonAliasId { get; set; }
        }

        #endregion

        private static Random randomGenerator = new Random();
        public static DateTime GetRandomDateInRange( DateTime minDate, DateTime maxDate )
        {
            var range = ( maxDate - minDate ).Days;

            return minDate.AddDays( randomGenerator.Next( range ) );
        }

        /// <summary>
        /// Returns an ordered list of random dates within a specified range.
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <param name="dateCount"></param>
        /// <returns></returns>
        public static List<DateTime> GetRandomDateTimesInRange( DateTime minDate, DateTime maxDate, int dateCount )
        {
            var totalSeconds = Convert.ToInt32( ( maxDate - minDate ).TotalSeconds );

            var dateList = new List<DateTime>();
            while ( dateCount > 0 )
            {
                dateList.Add( minDate.AddSeconds( randomGenerator.Next( totalSeconds ) ) );
                dateCount--;
            }

            dateList.Sort();

            return dateList;
        }

        public static Dictionary<int, DateTime> GetAnalyticsSourceDateTestData()
        {
            return new Dictionary<int, DateTime>
            {
                {20100131,  Convert.ToDateTime("2010-1-31")},
                {20101231,  Convert.ToDateTime("2010-12-31")},
                {20101201,  Convert.ToDateTime("2010-12-1")},
                {20100101,  Convert.ToDateTime("2010-1-1")},
                {20160229,  Convert.ToDateTime("2016-02-29")},
            };
        }

        public static DateTime GetAnalyticsSourceMinDateForYear( RockContext rockContext, int year )
        {
            if ( !rockContext.AnalyticsSourceDates.AsQueryable().Any() )
            {
                var analyticsStartDate = new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
                var analyticsEndDate = new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
                Rock.Model.AnalyticsSourceDate.GenerateAnalyticsSourceDateData( 1, false, analyticsStartDate, analyticsEndDate );
            }

            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MIN([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }

        public static DateTime GetAnalyticsSourceMaxDateForYear( RockContext rockContext, int year )
        {
            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MAX([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }

        #region Attribute Helpers

        /// <summary>
        /// Sets the value of an existing <see cref="AttributeValue"/> and saves it to the database or creates a new database record if one doesn't already exist.
        /// </summary>
        /// <param name="attributeGuid">The parent <see cref="Rock.Model.Attribute"/> unique identifier.</param>
        /// <param name="entityId">The ID of the entity - if any - to which this <see cref="AttributeValue"/> belongs.</param>
        /// <param name="value">The value to be set.</param>
        /// <param name="previousValue">If a <see cref="AttributeValue"/> already exists in the database, it's current value will be returned, so you can set it back after the current tests complete.</param>
        /// <param name="newAttributeValueGuid">If a <see cref="AttributeValue"/> doesn't already exist in the database, the <see cref="Guid"/> of the newly-created record will be returned, so you can delete it after the current tests complete.</param>
        public static void SetAttributeValue( Guid attributeGuid, int? entityId, string value, out string previousValue, out Guid newAttributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                previousValue = null;
                newAttributeValueGuid = Guid.Empty;

                var attributeId = AttributeCache.GetId( attributeGuid );
                if ( !attributeId.HasValue )
                {
                    return;
                }

                var attributeValueService = new AttributeValueService( rockContext );

                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId.Value, entityId );

                if ( attributeValue == null )
                {
                    attributeValue = new AttributeValue
                    {
                        AttributeId = attributeId.Value,
                        EntityId = entityId,
                        Value = value
                    };

                    attributeValueService.Add( attributeValue );

                    // Remember this so we can delete this AttributeValue upon cleanup.
                    newAttributeValueGuid = attributeValue.Guid;
                }
                else
                {
                    // Remeber this so we can set it back upon cleanup.
                    previousValue = attributeValue.Value;
                    attributeValue.Value = value;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the attribute value.
        /// </summary>
        /// <param name="attributeValueGuid">The attribute value unique identifier.</param>
        public static void DeleteAttributeValue( Guid attributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.Get( attributeValueGuid );
                if ( attributeValue == null )
                {
                    return;
                }

                attributeValueService.Delete( attributeValue );
                rockContext.SaveChanges();
            }
        }

        #endregion

        #region Campus

        public static string MainCampusGuidString = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";
        public static string SecondaryCampusName = "Stepping Stone";

        public static Campus GetOrAddCampusSteppingStone( RockContext rockContext )
        {
            // Add a new campus
            var campusService = new CampusService( rockContext );

            var campus2 = campusService.Get( TestGuids.Crm.CampusSteppingStone.AsGuid() );

            if ( campus2 == null )
            {
                campus2 = new Campus();

                campusService.Add( campus2 );
            }

            campus2.Name = SecondaryCampusName;
            campus2.Guid = TestGuids.Crm.CampusSteppingStone.AsGuid();
            campus2.IsActive = true;
            campus2.CampusStatusValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN.AsGuid() );
            campus2.CampusTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL.AsGuid() );

            rockContext.SaveChanges();

            return campus2;
        }

        #endregion

        /// <summary>
        /// Delete a set of person aliases identified by ForeignKey.
        /// </summary>
        /// <param name="foreignKeyList"></param>
        public static void DeletePersonAliases( IEnumerable<string> foreignKeyList )
        {
            var rockContext = new RockContext();

            var personAliasService = new PersonAliasService( rockContext );
            var personAliases = personAliasService.Queryable().Where( pa => foreignKeyList.Contains( pa.ForeignKey ) ).ToList();

            personAliasService.DeleteRange( personAliases );

            rockContext.SaveChanges();
        }

        public static void DeletePersonByGuid( IEnumerable<Guid> guidList )
        {
            try
            {
                var rockContext = new RockContext();

                // Delete Search Key
                //using ( var rockContext = new RockContext() )
                //{
                    var personSearchKeyService = new PersonSearchKeyService( rockContext );
                    var personSearchKeyQuery = personSearchKeyService.Queryable()
                        .Where( psk => guidList.Contains( psk.PersonAlias.Person.Guid ) );
                    personSearchKeyService.DeleteRange( personSearchKeyQuery );
                    rockContext.SaveChanges();
                //}

                // Delete Connection Requests
                //using ( var rockContext = new RockContext() )
                //{
                    var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                    var connectionRequestActivityQuery = connectionRequestActivityService.Queryable()
                        .Where( x => guidList.Contains( x.ConnectorPersonAlias.Person.Guid ) || guidList.Contains( x.ConnectionRequest.PersonAlias.Person.Guid ) );
                    connectionRequestActivityService.DeleteRange( connectionRequestActivityQuery );
                    rockContext.SaveChanges();
                //}

                //using ( var rockContext = new RockContext() )
                //{
                    var connectionRequestService = new ConnectionRequestService( rockContext );
                    var connectionRequestQuery = connectionRequestService.Queryable()
                        .Where( x => guidList.Contains( x.ConnectorPersonAlias.Person.Guid ) || guidList.Contains( x.PersonAlias.Person.Guid ) );
                    connectionRequestService.DeleteRange( connectionRequestQuery );
                    rockContext.SaveChanges();
                //}

                // Delete Auths.
                var authService = new AuthService( rockContext );
                var authQuery = authService.Queryable().Where( a => guidList.Contains( a.PersonAlias.Person.Guid ) );
                authService.DeleteRange( authQuery );
                rockContext.SaveChanges();

                // Delete Person Aliases
                //using ( var rockContext = new RockContext() )
                //{
                var personAliasService = new PersonAliasService( rockContext );
                    var personAliasQuery = personAliasService.Queryable()
                        .Where( pa => guidList.Contains( pa.Person.Guid ) || guidList.Contains( pa.AliasPerson.Guid ) );
                    personAliasService.DeleteRange( personAliasQuery );
                    rockContext.SaveChanges();
                //}

                // Delete Person
                //using ( var rockContext = new RockContext() )
                //{
                    var personService = new PersonService( rockContext );
                    var personQuery = personService.Queryable()
                        .Where( p => guidList.Contains( p.Guid ) );
                    personService.DeleteRange( personQuery );
                    rockContext.SaveChanges();
                //}
            }
            catch ( Exception ex )
            {
                throw ex;
            }

        }


        #region Sample Data

        public static class DataSetIdentifiers
        {
            public static string PrayerSampleData = "PrayerSampleData";
            public static string StepsSampleData = "StepsSampleData";
        }

        /// <summary>
        /// Add a well-known set of test data.
        /// </summary>
        /// <param name="datasetIdentifier">A <c>SampleDataHelper.DataSetIdentifiers</c> value that uniquely identifies the data set.</param>
        public static void AddTestDataSet( string datasetIdentifier )
        {
            var isValid = false;

            if ( datasetIdentifier == DataSetIdentifiers.PrayerSampleData )
            {
                PrayerFeatureDataHelper.AddSampleData();
                isValid = true;
            }
            else if ( datasetIdentifier == DataSetIdentifiers.StepsSampleData )
            {
                StepsFeatureDataHelper.AddSampleTestData();
                isValid = true;
            }

            else
            {

            }

            if ( !isValid )
            {
                throw new Exception( $"Invalid Data Set. The data set \"{datasetIdentifier}\" could not be loaded." );
            }
        }

        public static void RemoveTestDataSet( string datasetIdentifier )
        {
            var isValid = false;

            if ( datasetIdentifier == DataSetIdentifiers.PrayerSampleData )
            {
                PrayerFeatureDataHelper.RemoveSampleData();
                isValid = true;
            }
            else if ( datasetIdentifier == DataSetIdentifiers.StepsSampleData )
            {
                StepsFeatureDataHelper.RemoveStepsFeatureTestData();
                isValid = true;
            }

            if ( !isValid )
            {
                throw new Exception( $"Invalid Data Set. The data set \"{datasetIdentifier}\" could not be loaded." );
            }
        }

        #endregion

        #region Communications

        public static void DeleteCommunicationsByForeignKey( string foreignKey )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            var communications = communicationService.Queryable().Where( x => x.ForeignKey == foreignKey );

            DeleteCommunications( communications, rockContext );
        }
        public static void DeleteCommunications( IEnumerable<Rock.Model.Communication> communications )
        {
            DeleteCommunications( communications, new RockContext() );
        }

        private static void DeleteCommunications( IEnumerable<Rock.Model.Communication> communications, RockContext dataContext )
        {
            var communicationService = new CommunicationService( dataContext );
            communicationService.DeleteRange( communications );

            dataContext.SaveChanges();
        }

        #endregion

        private static RockContext GetActiveRockContext( RockContext rockContext )
        {
            return rockContext ?? new RockContext();
        }

        #region Asserts

        /// <summary>
        /// Asserts that the specified Rock Entity object has a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="identifier"></param>
        public static void AssertRockEntityIsNotNull<T>( T entity, string identifier )
    where T : IEntity
        {
            if ( entity == null )
            {
                var entityType = typeof( T );
                throw new Exception( $"{entityType.GetFriendlyTypeName()} not found. [Identifier={identifier}]" );
            }
        }

        #endregion
    }
}
