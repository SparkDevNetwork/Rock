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
using System.Web.Http;
using System.Collections.Generic;
using System.Linq;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.VersionInfo;

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
        public string CalculateSlidingDateRange( Rock.Web.UI.Controls.SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, Rock.Web.UI.Controls.SlidingDateRangePicker.TimeUnitType timeUnitType, int number = 1 )
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( string.Format( "{0}|{1}|{2}||", slidingDateRangeType, number, timeUnitType ) );
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
        public string GetSlidingDateRangeTextValue( Rock.Web.UI.Controls.SlidingDateRangePicker.SlidingDateRangeType slidingDateRangeType, Rock.Web.UI.Controls.SlidingDateRangePicker.TimeUnitType timeUnitType, int number = 1 )
        {
            string textValue = SlidingDateRangePicker.FormatDelimitedValues( string.Format( "{0}|{1}|{2}||", slidingDateRangeType, number, timeUnitType ) );
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
                    var campus = CampusCache.Read( guid );
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
    }
}
