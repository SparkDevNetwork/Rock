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
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.AttendanceCode"/> entity types
    /// </summary>
    public partial class AttendanceCodeService
    {
        private static readonly Object _obj = new object();
        private static Random _random = new Random( Guid.NewGuid().GetHashCode() );
        private static DateTime? _today = null;
        private static List<string> _todaysCodes = null;

        /// <summary>
        /// An array of characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static char[] codeCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z', '2', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// A list of <see cref="System.String"/> values that are not allowable as attendance codes.
        /// </summary>
        private static List<string> noGood = new List<string> { 
            "4NL", "4SS", "5CK", "5HT", "5LT", "5NM", "5TD", "5XX", "666", "BCH", "CLT", "CNT", "D4M", "D5H", "DCK", "DMN", "DSH", "F4G", "FCK", "FGT", "G4Y", "GZZ", "H8R", 
            "JZZ", "KKK", "KLT", "KNT", "L5D", "LCK", "LSD", "MFF", "MLF", "ND5", "NDS", "NDZ", "NGR", "P55", "PCP", "PHC", "PHK", "PHQ", "PM5", "PMS", "PN5", "PNS", "PRC", 
            "PRK", "PRN", "PRQ", "PSS", "RCK", "SCK", "SHT", "SLT", "SNM", "STD", "SXX", "THC", "V4G", "WCK", "XTC", "XXX" };

        /// <summary>
        /// Returns a new <see cref="Rock.Model.AttendanceCode"/>
        /// </summary>
        /// <param name="codeLength">A <see cref="System.Int32"/> representing the length of the (security) code.</param>
        /// <returns>A new <see cref="Rock.Model.AttendanceCode"/></returns>
        public static AttendanceCode GetNew( int codeLength = 3 )
        {
            lock ( _obj )
            {
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    var service = new AttendanceCodeService( rockContext );

                    DateTime today = RockDateTime.Today;
                    if ( _todaysCodes == null || !_today.HasValue || !_today.Value.Equals( today ) )
                    {
                        _today = today;
                        DateTime tomorrow = today.AddDays( 1 );
                        _todaysCodes = service.Queryable().AsNoTracking()
                            .Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow )
                            .Select( c => c.Code )
                            .ToList();
                    }

                    // Find a good unique code for today
                    string code = GenerateRandomCode( codeLength );
                    while ( noGood.Any( s => s == code ) || _todaysCodes.Any( c => c == code ) )
                    {
                        code = GenerateRandomCode( codeLength );
                    }
                    _todaysCodes.Add( code );

                    var attendanceCode = new AttendanceCode();
                    attendanceCode.IssueDateTime = RockDateTime.Now;
                    attendanceCode.Code = code;
                    service.Add( attendanceCode );
                    rockContext.SaveChanges();

                    return attendanceCode;
                }
            }
        }

        /// <summary>
        /// Generates a random (security) code.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private static string GenerateRandomCode( int length )
        {
            StringBuilder sb = new StringBuilder();

            int poolSize = codeCharacters.Length;
            for(int i = 0; i < length; i++)
            {
                sb.Append( codeCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }
    }
}
