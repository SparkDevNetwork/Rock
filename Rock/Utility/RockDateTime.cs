// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Configuration;

namespace Rock
{
    /// <summary>
    /// Special Class that returns current Current DateTime based on the TimeZone set in Web.Config
    /// This is done because the system may be hosted in a different timezone than the organization
    /// Rock developers should use RockDateTime.Now instead of DateTime.Now
    /// </summary>
    public class RockDateTime
    {
        /// <summary>
        /// Gets or sets the org time zone information.
        /// </summary>
        /// <value>
        /// The org time zone information.
        /// </value>
        private static TimeZoneInfo OrgTimeZoneInfo { get; set; }

        /// <summary>
        /// Gets current datetime based on the OrgTimeZone setting set in web.config
        /// </summary>
        /// <value>
        /// The current datetime for the Organization's TimeZone
        /// </value>
        public static DateTime Now
        {
            get
            {
                // determine the OrgTimeZOneInfo from web.config then cache it 
                if ( OrgTimeZoneInfo == null )
                {
                    string orgTimeZoneSetting = ConfigurationManager.AppSettings["OrgTimeZone"];

                    if ( string.IsNullOrWhiteSpace( orgTimeZoneSetting ) )
                    {
                        OrgTimeZoneInfo = TimeZoneInfo.Local;
                    }
                    else
                    {
                        // if Web.Config has the OrgTimeZone set to the special "Local" (intended for Developer Mode), just use the Local DateTime. However, a production install of Rock will always have a real Time Zone string
                        if ( orgTimeZoneSetting.Equals( "Local", StringComparison.OrdinalIgnoreCase ) )
                        {
                            OrgTimeZoneInfo = TimeZoneInfo.Local;
                        }
                        else
                        {
                            OrgTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( orgTimeZoneSetting );
                        }
                    }
                }

                return TimeZoneInfo.ConvertTime( DateTime.UtcNow, OrgTimeZoneInfo );
            }
        }
    }
}
