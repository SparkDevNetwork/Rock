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
using System.Text;
using System.Threading.Tasks;

namespace Rock.Slingshot
{
    public static class SlingshotExtensionMethods
    {
        /// <summary>
        /// The Minimum Date Boundary for a DateTime field in SQL Server.
        /// </summary>
        public static readonly DateTime MinSqlDate = new DateTime( 1753, 1, 1 );

        /// <summary>
        /// The Maximum Date Boundary for a DateTime field in SQL Server.
        /// </summary>
        public static readonly DateTime MaxSqlDate = new DateTime( 9999, 12, 31, 23, 59, 59, 99 );


        /// <summary>
        /// Gets a date value that is within the safe range of SQL Server DateTime ranges (see MinSqlDate and MaxSqlDate).
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/>.</param>
        /// <returns></returns>
        public static DateTime ToSQLSafeDate( this DateTime dateTime )
        {
            if ( dateTime <= MinSqlDate )
            {
                return MinSqlDate;
            }

            if ( dateTime >= MaxSqlDate )
            {
                return MaxSqlDate;
            }

            return dateTime;
        }

        /// <summary>
        /// Gets a nullable date value that is within the safe range of SQL Server DateTime ranges (see MinSqlDate and MaxSqlDate).
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime?"/>.</param>
        /// <returns></returns>
        public static DateTime? ToSQLSafeDate( this DateTime? dateTime )
        {
            if ( !dateTime.HasValue )
            {
                return null;
            }

            return dateTime.Value.ToSQLSafeDate();
        }
    }
}
