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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.AttendanceCode"/> entity types
    /// </summary>
    public partial class AttendanceCodeService
    {
        private static readonly Object obj = new object();

        /// <summary>
        /// An array of characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static char[] codeCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z', '2', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// A list of <see cref="System.String"/> values that are not allowable as attendance codes.
        /// </summary>
        private static List<string> noGood = new List<string> { "666", "KKK", "FCK", "SHT", "5HT", "DCK" };

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.AttendanceCode"/> entities that used a specified code on a specified date.
        /// </summary>
        /// <param name="day">A <see cref="System.DateTime" /> representing the date to search by.</param>
        /// <param name="code">A <see cref="System.String"/> representing the code to search for.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.AttendanceCode"/> entities that contain a specific code on a specified day. </returns>
        public IQueryable<AttendanceCode> Get( DateTime day, string code )
        {
            DateTime today = day.Date;
            DateTime tomorrow = today.AddDays( 1 );
            return Queryable().Where( c => c.Code == code && c.IssueDateTime >= today && c.IssueDateTime < tomorrow);
        }

        /// <summary>
        /// Returns a new <see cref="Rock.Model.AttendanceCode"/>
        /// </summary>
        /// <param name="codeLength">A <see cref="System.Int32"/> representing the length of the (security) code.</param>
        /// <returns>A new <see cref="Rock.Model.AttendanceCode"/></returns>
        public static AttendanceCode GetNew(int codeLength = 3)
        {
            string code = string.Empty;

            var attendanceCode = new AttendanceCode();

            var rockContext = new Rock.Data.RockContext();
            var service = new AttendanceCodeService( rockContext );

            // Make sure only one instance at a time is checking for unique code
            lock (obj)
            {
                // Find a good unique code for today
                while ( code == string.Empty ||
                    noGood.Any( s => s == code ) ||
                    service.Get( RockDateTime.Today, code ).Any() )
                {
                    code = GenerateRandomCode( codeLength );
                }

                attendanceCode.IssueDateTime = RockDateTime.Now;
                attendanceCode.Code = code;
                service.Add( attendanceCode );
                rockContext.SaveChanges();
            }

            return attendanceCode;
        }

        /// <summary>
        /// Generates a random (security) code.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private static string GenerateRandomCode( int length )
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            int poolSize = codeCharacters.Length;

            for(int i = 0; i < length; i++)
            {
                sb.Append( codeCharacters[rnd.Next( poolSize )] );
            }

            return sb.ToString();
        }
    }
}
