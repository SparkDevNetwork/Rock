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
using System.Globalization;

namespace Rock
{
    /// <summary>
    /// DateTime and TimeStamp Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region DateTime Extensions

        /// <summary>
        /// Returns the value for <see cref="DateTime.ToShortDateString"/> or empty string if the date is null
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "20.0" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToShortDateString( DateTime? dateTime )
        {
            if ( dateTime.HasValue )
            {
                return dateTime.Value.ToShortDateString();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion DateTime Extensions

        #region TimeSpan Extensions

        #endregion TimeSpan Extensions

        #region Time/Date Rounding 

        #endregion Time/Date Rounding 
    }
}
