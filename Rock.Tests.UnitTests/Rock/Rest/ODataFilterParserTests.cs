using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Rest;

namespace Rock.Tests.UnitTests.Rest
{
    [TestClass]
    public class ODataFilterParserTests
    {
        [TestMethod]
        [DataRow( 10, 0, false )]
        [DataRow( 100, 0, false )]
        [DataRow( 1000, 0, false )]

        [DataRow( 10, 1, false )]
        [DataRow( 100, 1, false )]
        [DataRow( 1000, 1, false )]
        [DataRow( 10000, 1, false )]

        [DataRow( 10, 5, false )]
        [DataRow( 100, 5, false )]
        [DataRow( 1000, 5, false )]
        [DataRow( 10000, 5, false )]

        [DataRow( 10, 11, false )]
        [DataRow( 100, 11, false )]
        [DataRow( 1000, 11, false )]

        [DataRow( 10, 0, true )]
        [DataRow( 100, 0, true )]
        [DataRow( 1000, 0, true )]

        [DataRow( 10, 1, true )]
        [DataRow( 100, 1, true )]
        [DataRow( 1000, 1, true )]
        [DataRow( 10000, 1, true )]

        [DataRow( 10, 5, true )]
        [DataRow( 100, 5, true )]
        [DataRow( 1000, 5, true )]
        [DataRow( 10000, 5, true )]

        [DataRow( 10, 11, true )]
        [DataRow( 100, 11, true )]
        [DataRow( 1000, 11, true )]

        [DataRow( 1000, 22, true )]

        public void PerformanceTest( int urlCount, int filterCount, bool useEscapeUriString )
        {
            string baseUrl = $"https://localhost:44329/api/People?$filter=";
            List<string> filterExpressionList = new List<string>();

            for ( int i = 0; i < filterCount; i++ )
            {
                if ( i % 2 == 0 )
                {
                    filterExpressionList.Add( $"Guid eq guid'{Guid.NewGuid():D}'" );
                }
                else
                {
                    filterExpressionList.Add( $"ModifiedDateTime eq datetime'{RockDateTime.Now.ToISO8601DateString()}'" );
                }
            }

            var filterExpressionFilter = filterExpressionList.AsDelimited( " and " );
            string originalUrl;
            if ( useEscapeUriString )
            {
                originalUrl = baseUrl + System.Net.WebUtility.UrlEncode( filterExpressionFilter );
            }
            else
            {
                originalUrl = baseUrl + Uri.EscapeDataString( filterExpressionFilter );
            }

            var warmup = RockEnableQueryAttribute.ParseRawFilterFromOriginalUrl( originalUrl, filterExpressionFilter );

            List<double> elaspedMS = new List<double>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            for ( int i = 0; i < urlCount; i++ )
            {
                stopwatch.Restart();
                var updatedUrl = RockEnableQueryAttribute.ParseRawFilterFromOriginalUrl( originalUrl, filterExpressionFilter );
                stopwatch.Stop();
                elaspedMS.Add( stopwatch.Elapsed.TotalMilliseconds );
            }

            var averageMS = elaspedMS.Average();

            const double maxExpectedMS = 0.25;

            Assert.IsTrue( averageMS < maxExpectedMS );
        }

        [TestMethod]
        [DataRow(
            "Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53",
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747-07:00" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747Z and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747Z and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747Z",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747Z" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747+09:00",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747+09:00" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747-05:00",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747-05:00" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747' and Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747-07:00 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747' and Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747-07:00 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747Z'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747Z" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747+09:00'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747+09:00" )]
        public void FilterDidParseCorrectlyTest( string originalFilter, string expectedResult )
        {
            var tzDefault = RockDateTime.OrgTimeZoneInfo;

            var zones = TimeZoneInfo.GetSystemTimeZones();

            // Set the Rock server timezone to UTC-07:00.
            var tzTest = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

            try
            {
                RockDateTime.Initialize( tzTest );

                string originalUrlUrlEncoded = System.Net.WebUtility.UrlEncode( $"https://localhost:44329/api/People?$filter={originalFilter}" );
                var actualResultUrlEncoded = RockEnableQueryAttribute.ParseRawFilterFromOriginalUrl( originalUrlUrlEncoded, originalFilter );
                string expectedUrlUrlEncoded = System.Net.WebUtility.UrlEncode( $"https://localhost:44329/api/People?$filter={expectedResult}" );
                Assert.AreEqual( actualResultUrlEncoded, expectedUrlUrlEncoded );

                string originalUrlUriEscapeUriString = Uri.EscapeUriString( $"https://localhost:44329/api/People?$filter={originalFilter}" );
                var actualResultUriEscapeUriString = RockEnableQueryAttribute.ParseRawFilterFromOriginalUrl( originalUrlUriEscapeUriString, originalFilter );
                string expectedUriEscapeUriString = Uri.EscapeUriString( $"https://localhost:44329/api/People?$filter={expectedResult}" );
                Assert.AreEqual( actualResultUriEscapeUriString, expectedUriEscapeUriString );
            }
            finally
            {
                RockDateTime.Initialize( tzDefault );
            }
        }

        [TestMethod]
        [DataRow(
            "PersonAlias/Person",
            "PersonAlias($expand=Person)" )]
        [DataRow(
            "PersonAlias/Person,GroupMember,PhoneNumbers/Guid",
            "PersonAlias($expand=Person),GroupMember,PhoneNumbers($expand=Guid)" )]
        [DataRow(
            "PersonAlias/Person/Users,GroupMember/Group/Person,PhoneNumbers/Guid,SomeOtherNavigationProperty",
            "PersonAlias($expand=Person($expand=Users)),GroupMember($expand=Group($expand=Person)),PhoneNumbers($expand=Guid),SomeOtherNavigationProperty" )]
        [DataRow(
            "",
            "" )]
        public void ExpandDidParseCorrectlyTest( string originalExpand, string expectedResult )
        {
            string originalUrl = $"api/PackageVersionRatings?$expand={originalExpand}";
            var actualResultUrl = RockEnableQueryAttribute.ParseExpandClauseFromOriginalUrl( originalUrl, originalExpand);
            string expectedUrl = $"api/PackageVersionRatings?$expand={expectedResult}";
            Assert.AreEqual( actualResultUrl, expectedUrl );
        }
    }
}
