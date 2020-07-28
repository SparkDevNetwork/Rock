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
