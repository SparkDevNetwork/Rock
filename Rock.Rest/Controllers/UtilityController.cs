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
using System.Web.Http;
using Rock.Data;
using Rock.Net;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
    [Rock.SystemGuid.RestControllerGuid( "846640A1-874B-4C12-AF0F-D50D562FF0CC")]
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
        [Rock.SystemGuid.RestActionGuid( "68045EF4-5898-495E-9638-63F33ED2CF23" )]
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
        [Rock.SystemGuid.RestActionGuid( "7F2584B2-3182-4AC5-87B1-E9FEC45AAA75" )]
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
        [Rock.SystemGuid.RestActionGuid( "2513F4C9-2578-4A4A-9F4D-B059AE825A77" )]
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
        [Rock.SystemGuid.RestActionGuid( "AC5F9A4C-18AD-4108-99CB-48546C3F1CAB" )]
        public string GetSlidingDateRangeTextValue( SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, SlidingDateRangePicker.TimeUnitType timeUnitType, string startDate, string endDate, int number = 1 )
        {
            string textValue = SlidingDateRangePicker.FormatDelimitedValues( string.Format( "{0}|{1}|{2}|{3}|{4}", slidingDateRangeType, number, timeUnitType, startDate, endDate ) );
            return textValue;
        }

        /// <summary>
        /// Gets the campus context.
        /// </summary>
        /// <returns>The identifier for the campus context or 0 if not found.</returns>
        [System.Web.Http.Route( "api/Utility/GetCampusContext" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "771A4B90-302E-4DAF-BC11-FB7C47615C9F" )]
        public int GetCampusContext()
        {
            // Get the sitewide context cookie.
            System.Web.HttpCookie contextCookie = null;
            if ( System.Web.HttpContext.Current.Request.Cookies.AllKeys.Contains( RockRequestContext.SiteContextCookieName ) )
            {
                contextCookie = System.Web.HttpContext.Current.Request.Cookies[RockRequestContext.SiteContextCookieName];
            }

            if ( contextCookie == null )
            {
                return 0;
            }

            try
            {
                // See if the cookie contains a campus context value.
                var contextItems = contextCookie.Value.FromJsonOrNull<Dictionary<string, string>>();
                string contextItem = null;
                if ( contextItems?.TryGetValue( "Rock.Model.Campus", out contextItem ) != true )
                {
                    return 0;
                }

                // Attempt to parse the URL-encoded, encrypted campus context value.
                var decodedItem = System.Web.HttpUtility.UrlDecode( contextItem );
                var decryptedItem = Rock.Security.Encryption.DecryptString( decodedItem );
                var itemParts = decryptedItem.Split( '|' );
                if ( itemParts.Length != 2 )
                {
                    return 0;
                }

                var idParts = itemParts[1].Split( '>' );
                if ( idParts.Length != 2 )
                {
                    return 0;
                }

                var campusCache = CampusCache.Get( idParts[1] );

                return campusCache?.Id ?? 0;
            }
            catch
            {
                // Intentionally ignore exception in case parsing fails.
            }

            return 0;
        }

        /// <summary>
        /// Gets the rock semantic version number.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Utility/GetRockSemanticVersionNumber" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "E44CC71F-2952-4400-A04D-F3C242C8664E" )]
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
        [Rock.SystemGuid.RestActionGuid( "A42C1F49-D3E2-4411-ABEF-F6B2B1D18480" )]
        public string TextToWorkflow( string fromNumber, string toNumber, string message )
        {
            var processResponse = string.Empty;

            Rock.Utility.TextToWorkflow.MessageRecieved( toNumber, fromNumber, message, out processResponse );

            return processResponse;
        }
    }
}