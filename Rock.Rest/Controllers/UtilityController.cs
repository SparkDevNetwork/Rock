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
using System.Linq;
using System.Web.Http;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
    [RockGuid( "846640a1-874b-4c12-af0f-d50d562ff0cc" )]
    public class UtilityController : ApiController
    {
        /// <summary>
        /// Calculates the sliding date range for the SlidingDateRange control (called from client side) and returns a string representing the date range
        /// </summary>
        /// <param name="slidingDateRangeType">Type of the sliding date range. </param>
        /// <param name="timeUnitType">Type of the time unit. Hour = 0, Day = 1, Week = 2, Month = 3, Year = 4</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/CalculateSlidingDateRange" )]
        [HttpGet]
        [RockGuid( "68045ef4-5898-495e-9638-63f33ed2cf23" )]
        public string CalculateSlidingDateRange( SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, SlidingDateRangePicker.TimeUnitType timeUnitType, int number = 1 )
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( string.Format( "{0}|{1}|{2}|{3}|{4}", slidingDateRangeType, number, timeUnitType, string.Empty, string.Empty ) );
            return dateRange.ToStringAutomatic();
        }

        /// <summary>
        /// Calculates the sliding date range with optional start and end dates.
        /// </summary>
        /// <param name="slidingDateRangeType">Type of the sliding date range.</param>
        /// <param name="timeUnitType">Type of the time unit. Hour = 0, Day = 1, Week = 2, Month = 3, Year = 4</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/CalculateSlidingDateRange" )]
        [HttpGet]
        [RockGuid( "7f2584b2-3182-4ac5-87b1-e9fec45aaa75" )]
        public string CalculateSlidingDateRange( SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, SlidingDateRangePicker.TimeUnitType timeUnitType, string startDate, string endDate, int number = 1 )
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( string.Format( "{0}|{1}|{2}|{3}|{4}", slidingDateRangeType, number, timeUnitType, startDate, endDate ) );
            return dateRange.ToStringAutomatic();
        }

        /// <summary>
        /// Calculates the sliding date range text value for the SlidingDateRange control (called from client side) and returns a string of the sliding date range picker values in text format (Last 4 Weeks, etc)
        /// </summary>
        /// <param name="slidingDateRangeType">Type of the sliding date range. </param>
        /// <param name="timeUnitType">Type of the time unit. Hour = 0, Day = 1, Week = 2, Month = 3, Year = 4</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/GetSlidingDateRangeTextValue" )]
        [HttpGet]
        [RockGuid( "2513f4c9-2578-4a4a-9f4d-b059ae825a77" )]
        public string GetSlidingDateRangeTextValue( SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, SlidingDateRangePicker.TimeUnitType timeUnitType, int number = 1 )
        {
            string textValue = SlidingDateRangePicker.FormatDelimitedValues( string.Format( "{0}|{1}|{2}|{3}|{4}", slidingDateRangeType, number, timeUnitType, string.Empty, string.Empty ) );
            return textValue;
        }

        /// <summary>
        /// Calculates the sliding date range text value for the SlidingDateRange control (called from client side) and returns a string of the sliding date range picker values in text format (Last 4 Weeks, etc)
        /// </summary>
        /// <param name="slidingDateRangeType">Type of the sliding date range. </param>
        /// <param name="timeUnitType">Type of the time unit. Hour = 0, Day = 1, Week = 2, Month = 3, Year = 4</param>
        /// <param name="number">The number.</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/GetSlidingDateRangeTextValue" )]
        [HttpGet]
        [RockGuid( "ac5f9a4c-18ad-4108-99cb-48546c3f1cab" )]
        public string GetSlidingDateRangeTextValue( SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, SlidingDateRangePicker.TimeUnitType timeUnitType, string startDate, string endDate, int number = 1 )
        {
            string textValue = SlidingDateRangePicker.FormatDelimitedValues( string.Format( "{0}|{1}|{2}|{3}|{4}", slidingDateRangeType, number, timeUnitType, startDate, endDate ) );
            return textValue;
        }

        /// <summary>
        /// Gets the campus context.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/GetCampusContext" )]
        [HttpGet]
        [RockGuid( "771a4b90-302e-4daf-bc11-fb7c47615c9f" )]
        public int GetCampusContext()
        {
            string campusCookieCypher = null;
            if ( System.Web.HttpContext.Current.Request.Cookies.AllKeys.Contains( "Rock_Context" ) )
            {
                var contextCookie = System.Web.HttpContext.Current.Request.Cookies["Rock_Context"];
                if ( contextCookie.Values.OfType<string>().Contains( "Rock.Model.Campus" ) )
                {
                    campusCookieCypher = contextCookie.Values["Rock.Model.Campus"];
                }
            }

            if ( campusCookieCypher == null )
            {
                return 0;
            }

            try
            {
                var publicKey = Rock.Security.Encryption.DecryptString( campusCookieCypher ).Split( '|' )[1];

                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = idParts[0].AsInteger();
                    Guid guid = idParts[1].AsGuid();
                    var campus = CampusCache.Get( guid );
                    if ( campus != null )
                    {
                        return campus.Id;
                    }
                }
            }
            catch
            {
                // ignore and return 0
            }

            return 0;
        }

        /// <summary>
        /// Gets the rock semantic version number.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/GetRockSemanticVersionNumber" )]
        [HttpGet]
        [RockGuid( "e44cc71f-2952-4400-a04d-f3c242c8664e" )]
        public string GetRockSemanticVersionNumber()
        {
            return VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
        }

        /// <summary>
        /// Initiates a new workflow
        /// </summary>
        /// <param name="fromNumber">From number.</param>
        /// <param name="toNumber">To number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        //[Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Utility/TextToWorkflow/{fromNumber}/{toNumber}/{message}" )]
        [RockGuid( "a42c1f49-d3e2-4411-abef-f6b2b1d18480" )]
        public string TextToWorkflow( string fromNumber, string toNumber, string message )
        {
            var processResponse = string.Empty;

            Rock.Utility.TextToWorkflow.MessageRecieved( toNumber, fromNumber, message, out processResponse );

            return processResponse;
        }
    }
}
