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
using System.Text.RegularExpressions;

namespace Rock.SystemGuid
{
    internal class DuplicateSystemGuidException : Exception
    {
        private readonly Exception thrownException;
        private readonly string itemMessage;
        private readonly System.Data.SqlClient.SqlException sqlException;

        private DuplicateSystemGuidException( Exception thrownException, System.Data.SqlClient.SqlException duplicateGuidSqlException, string itemMessage )
            : base()
        {
            this.thrownException = thrownException;
            this.sqlException = duplicateGuidSqlException;
            this.itemMessage = itemMessage;
        }

        internal DuplicateSystemGuidException( string message )
            : base( message )
        {
        }

        public override string Message
        {
            get
            {
                if ( itemMessage == null || sqlException == null )
                {
                    return base.Message;
                }

                if ( itemMessage.IsNotNullOrWhiteSpace() )
                {
                    return $"{itemMessage}, SQL Exception: {sqlException.Message}";
                }

                return sqlException.Message;
            }
        }

        public override string StackTrace
        {
            get
            {
                if ( thrownException != null && sqlException != null )
                {
                    return $"{thrownException.StackTrace}\r\n\r\nSQL Stacktrace:\r\n{sqlException.StackTrace}";
                }
                else
                {
                    return base.StackTrace;
                }
            }
        }

        private static System.Data.SqlClient.SqlException FindDuplicateGuidSqlException( Exception ex )
        {
            System.Data.SqlClient.SqlException sqlException = null;
            var exception = ex;
            while ( exception != null )
            {
                if ( exception is System.Data.SqlClient.SqlException )
                {
                    sqlException = exception as System.Data.SqlClient.SqlException;

                    // 2601 is a Unique Index constraint error, if it also mentions the "IX_Guid" constraint, then we want to convert it to DuplicateSystemGuidException
                    // https://docs.microsoft.com/en-us/previous-versions/sql/sql-server-2008-r2/ms151779(v=sql.105)
                    if ( sqlException != null && sqlException.Number == 2601 && Regex.IsMatch( sqlException.Message, "IX_Guid", RegexOptions.IgnoreCase ) )
                    {
                        return sqlException;
                    }
                }

                exception = exception.InnerException;
            }

            return null;
        }

        /// <summary>
        /// When adding items to the database that have a <see cref="RockGuidAttribute" />, this
        /// will search thru the exception and return a DuplicateSystemGuidException if the
        /// exception is due to a Unique Constraint on the Guid.
        /// If this does return DuplicateSystemGuidException, throw the DuplicateSystemGuidException instead of the thrown exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="itemMessage">A message indication which thing has a duplicate. For example, the BlockType Path/Name </param>
        /// <returns>DuplicateSystemGuidException.</returns>
        public static DuplicateSystemGuidException CatchDuplicateSystemGuidException( Exception ex, string itemMessage )
        {
            var duplicateGuidSqlException = FindDuplicateGuidSqlException( ex );
            if ( duplicateGuidSqlException == null )
            {
                return null;
            }

            DuplicateSystemGuidException duplicateSystemGuidException = new DuplicateSystemGuidException( ex, duplicateGuidSqlException, itemMessage );

            return duplicateSystemGuidException;
        }
    }
}
