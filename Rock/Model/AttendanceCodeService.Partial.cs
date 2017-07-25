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
        /// An array of alpha characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static char[] alphaCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z' };

        /// <summary>
        /// An array of alpha characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static char[] numericCharacters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// A list of <see cref="System.String"/> values that are not allowable as attendance codes.
        /// </summary>
        private static List<string> noGood = new List<string> {
            "4NL", "4SS", "5CK", "5HT", "5LT", "5NM", "5TD", "5XX", "666", "BCH", "CLT", "CNT", "D4M", "D5H", "DCK", "DMN", "DSH", "F4G", "FCK", "FGT", "G4Y", "GZZ", "H8R",
            "JNK", "JZZ", "KKK", "KLT", "KNT", "L5D", "LCK", "LSD", "MFF", "MLF", "ND5", "NDS", "NDZ", "NGR", "P55", "PCP", "PHC", "PHK", "PHQ", "PM5", "PMS", "PN5", "PNS",
            "PRC", "PRK", "PRN", "PRQ", "PSS", "RCK", "SCK", "S3X", "SHT", "SLT", "SNM", "STD", "SXX", "THC", "V4G", "WCK", "XTC", "XXX", "911" };

        /// <summary>
        /// Returns a new <see cref="Rock.Model.AttendanceCode"/> comprised of random alpha numeric characters.
        /// </summary>
        /// <param name="codeLength">A <see cref="System.Int32"/> representing the length of the (security) code.</param>
        /// <returns>A new <see cref="Rock.Model.AttendanceCode"/></returns>
        public static AttendanceCode GetNew( int codeLength = 3 )
        {
            return GetNew( codeLength, 0, 0, false );
        }

        /// <summary>
        /// Returns a new <see cref="Rock.Model.AttendanceCode" /> with a specified number of alpha characters followed by
        /// another specified number of numeric characters.  The numeric character sequence will not repeat for "today" so 
        /// ensure that you're using a sufficient numericLength otherwise it will be unable to find a unique number.
        /// Also note as the issued numeric codes reaches the maximum (from the set of possible), it will take longer and
        /// longer to find an unused number.
        /// </summary>
        /// <param name="alphaLength">A <see cref="System.Int32"/> representing the length of the (alpha) portion of the code.</param>
        /// <param name="numericLength">A <see cref="System.Int32"/> representing the length of the (digit) portion of the code.</param>
        /// <param name="isRandomized">A <see cref="System.Boolean"/> that controls whether or not the AttendanceCodes should be generated randomly or in order (starting from the smallest).</param>
        /// <returns>
        /// A new <see cref="Rock.Model.AttendanceCode" />
        /// </returns>
        public static AttendanceCode GetNew( int alphaLength = 2, int numericLength = 4, bool isRandomized = true )
        {
            return GetNew( 0, alphaLength, numericLength, isRandomized );
        }

        /// <summary>
        /// Gets the new.
        /// </summary>
        /// <param name="alphaNumericLength">Length of the alpha numeric.</param>
        /// <param name="alphaLength">Length of the alpha.</param>
        /// <param name="numericLength">Length of the numeric.</param>
        /// <param name="isRandomized">if set to <c>true</c> [is randomized].</param>
        /// <returns></returns>
        public static AttendanceCode GetNew( int alphaNumericLength, int alphaLength, int numericLength, bool isRandomized )
        {
            lock ( _obj )
            {
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    string alphaNumericCode = string.Empty;
                    string numericCode = string.Empty;

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

                    // Find a good alphanumeric code prefix
                    if ( alphaNumericLength > 0 || alphaLength > 0 )
                    {
                        alphaNumericCode =
                            ( alphaNumericLength > 0 ? GenerateRandomCode( alphaNumericLength ) : string.Empty ) +
                            ( alphaLength > 0 ? GenerateRandomAlphaCode( alphaLength ) : string.Empty );
                        while ( noGood.Any( s => alphaNumericCode.Contains( s ) ) || _todaysCodes.Contains( alphaNumericCode ) )
                        {
                            alphaNumericCode =
                                ( alphaNumericLength > 0 ? GenerateRandomCode( alphaNumericLength ) : string.Empty ) +
                                ( alphaLength > 0 ? GenerateRandomAlphaCode( alphaLength ) : string.Empty );
                        }
                    }

                    // Find a good unique numeric code for today
                    if ( numericLength > 0 )
                    {
                        if ( isRandomized )
                        {
                            numericCode = GenerateRandomNumericCode( numericLength );
                            while ( noGood.Any( s => s == numericCode ) || _todaysCodes.Any( c => c.EndsWith( numericCode ) ) )
                            {
                                numericCode = GenerateRandomNumericCode( numericLength );
                            }
                        }
                        else
                        {
                            int codeLen = alphaNumericLength + alphaLength + numericLength;
                            var lastCode = _todaysCodes.Where( c => c.Length == codeLen ).OrderBy( c => c.Substring( alphaNumericLength + alphaLength ) ).LastOrDefault();
                            if ( lastCode != null )
                            {
                                var maxCode = lastCode.Substring( alphaNumericLength + alphaLength );
                                numericCode = ( maxCode.AsInteger() + 1 ).ToString( "D" + numericLength );
                            }
                            else
                            {
                                numericCode = 1.ToString( "D" + numericLength );
                            }
                        }
                    }

                    string code = alphaNumericCode + numericCode;
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
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( codeCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random (security) code containing only alpha characters.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private static string GenerateRandomAlphaCode( int length )
        {
            StringBuilder sb = new StringBuilder();

            int poolSize = alphaCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( alphaCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random (security) code containing only numeric characters.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private static string GenerateRandomNumericCode( int length )
        {
            StringBuilder sb = new StringBuilder();

            int poolSize = numericCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( numericCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Flushes the todays codes.
        /// </summary>
        public static void FlushTodaysCodes()
        {
            lock ( _obj )
            {
                _todaysCodes = null;
            }
        }
    }
}
