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

using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
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
        public string TextToWorkflow( string fromNumber, string toNumber, string message )
        {
            var processResponse = string.Empty;

            Utility.TextToWorkflow.MessageRecieved( toNumber, fromNumber, message, out processResponse );

            return processResponse;
        }
    }
}
