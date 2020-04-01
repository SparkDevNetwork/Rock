using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration.TestData
{
    public static class TestDataHelper
    {
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

        private static Random randomGenerator = new Random();
        public static DateTime GetRandomDateInRange( DateTime minDate, DateTime maxDate )
        {
            var range = ( maxDate - minDate ).Days;

            return minDate.AddDays( randomGenerator.Next( range ) );
        }

        public static Dictionary<int, DateTime> GetAnalyticsSourceDateTestData()
        {
            return new Dictionary<int, DateTime>
            {
                {20100131,  Convert.ToDateTime("1/31/2010")},
                {20101231,  Convert.ToDateTime("12/31/2010")},
                {20101201,  Convert.ToDateTime("12/1/2010")},
                {20100101,  Convert.ToDateTime("1/1/2010")},
                {20160229,  Convert.ToDateTime("02/29/2016")},
            };
        }

        public static DateTime GetAnalyticsSourceMinDateForYear( RockContext rockContext, int year )
        {
            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MIN([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }

        public static DateTime GetAnalyticsSourceMaxDateForYear( RockContext rockContext, int year )
        {
            return rockContext.Database.SqlQuery<DateTime>( $"SELECT MAX([Date]) FROM AnalyticsSourceDate WHERE CalendarYear = {year}" ).First();
        }
    }
}
