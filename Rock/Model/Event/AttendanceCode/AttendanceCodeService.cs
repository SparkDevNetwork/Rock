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
        private static readonly object _obj = new object();
        private static readonly Random _random = new Random( Guid.NewGuid().GetHashCode() );
        private static DateTime? _todaysDate = null;
        private static HashSet<string> _todaysUsedCodes = null;

        // We're only going to attempt to get a code 1 million times...
        // Interestingly, even when this code approaches the maximum number of possible combinations
        // it still typically takes less than 5000 attempts. However, if the number of
        // attempts jumps over 10,000 there is almost certainly a problem with someone's
        // check-in code configuration so we're going to stop after a million attempts.
        private static readonly int _maxAttempts = 1000000;

        /// <summary>
        /// An array of characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static readonly char[] _codeCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z', '2', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// An array of alpha characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static readonly char[] _alphaCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z' };

        /// <summary>
        /// An array of numeric characters that can be used as a part of  <see cref="Rock.Model.AttendanceCode">AttendanceCodes</see>
        /// </summary>
        private static readonly char[] _numericCharacters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// A list of <see cref="System.String"/> values that are not allowable as attendance codes.
        /// </summary>
        public static readonly List<string> NoGood = new List<string> {
            "4NL", "4SS", "5CK", "5HT", "5LT", "5NM", "5TD", "5XX", "666", "BCH", "CLT", "CNT", "D4M", "D5H", "DCK", "DMN", "DSH", "F4G", "FCK", "FGT", "G4Y", "GZZ", "H8R",
            "JNK", "JZZ", "KKK", "KLT", "KNT", "L5D", "LCK", "LSD", "MFF", "MLF", "ND5", "NDS", "NDZ", "NGR", "P55", "PCP", "PHC", "PHK", "PHQ", "PM5", "PMS", "PN5", "PNS",
            "PRC", "PRK", "PRN", "PRQ", "PSS", "RCK", "SCK", "S3X", "SHT", "SLT", "SNM", "STD", "SXX", "THC", "V4G", "WCK", "XTC", "XXX", "911", "1XL", "2XL", "3XL", "4XL",
            "5XL", "6XL", "7XL", "8XL", "9XL", "XXL", "F4T", "FRT", "DHR", "MFR", "FKR"
        };

        /// <summary>
        /// A list of <see cref="System.String"/> values that are not allowable as attendance codes.
        /// </summary>
        [Obsolete( "Use NoGood instead" )]
        [RockObsolete( "1.13" )]
        public static readonly List<string> noGood = NoGood;

        private static readonly string timeoutExceptionMessage = "Too many attempts to create a unique attendance code.  There is almost certainly a check-in system 'Security Code Length' configuration problem.";

        /// <summary>
        /// Determines whether the provided code is already in use for today by querying the DB.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>
        ///   <c>true</c> if [is code already in use] [the specified code]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCodeAlreadyInUse( string code )
        {
            DateTime today = RockDateTime.Today;
            DateTime tomorrow = today.AddDays( 1 );
            return this.Queryable().AsNoTracking().Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow && c.Code == code ).Any();
        }

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
        /// <param name="isRandomized">A <see cref="System.Boolean"/> that controls whether or not the AttendanceCodes should be generated randomly or in order (starting from the smallest). Only effect the numeric code.</param>
        /// <returns>
        /// A new <see cref="Rock.Model.AttendanceCode" />
        /// </returns>
        public static AttendanceCode GetNew( int alphaLength = 2, int numericLength = 4, bool isRandomized = true )
        {
            return GetNew( 0, alphaLength, numericLength, isRandomized );
        }

        /// <summary>
        /// Returns a new <see cref="Rock.Model.AttendanceCode" />. The code will contain the specified number of alphanumeric characters,
        /// followed by the alpha characters and then the specified number of numeric characters. The character sequence will not repeat for "today".
        /// Also the numeric character sequence will not repeat for "today". In both cases ensure that you're using a sufficient length for each
        /// otherwise there will not be enough possible codes.
        /// Also note as the issued numeric codes reaches the maximum (from the set of possible), it will take longer and
        /// longer to find an unused number. So specifing a larger number of characters then needed will increase performance.
        /// </summary>
        /// <param name="alphaNumericLength">A <see cref="System.Int32"/> representing the length for a mixed alphanumberic code.</param>
        /// <param name="alphaLength">A <see cref="System.Int32"/> representing the length of the (alpha) portion of the code.</param>
        /// <param name="numericLength">A <see cref="System.Int32"/> representing the length of the (digit) portion of the code.</param>
        /// <param name="isRandomized">A <see cref="System.Boolean"/> that controls whether or not the AttendanceCodes should be generated randomly or in order (starting from the smallest). Only effect the numeric code.</param>
        /// <returns></returns>
        public static AttendanceCode GetNew( int alphaNumericLength, int alphaLength, int numericLength, bool isRandomized )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var service = new AttendanceCodeService( rockContext );

                return service.CreateNewCode( alphaNumericLength, alphaLength, numericLength, isRandomized );
            }
        }

        /// <summary>
        /// <para>
        /// Creates a new <see cref="Rock.Model.AttendanceCode" />. The code will contain the specified number of alphanumeric characters,
        /// followed by the alpha characters and then the specified number of numeric characters. The character sequence will not repeat for "today".
        /// Also the numeric character sequence will not repeat for "today". In both cases ensure that you're using a sufficient length for each
        /// otherwise there will not be enough possible codes.
        /// </para>
        /// <para>
        /// Also note as the issued numeric codes reaches the maximum (from the set of possible), it will take longer and
        /// longer to find an unused number. So specifing a larger number of characters then needed will increase performance.
        /// </para>
        /// <para>
        /// The security code will be persisted to the database before it is
        /// returned. That is, <see cref="DbContext.SaveChanges"/> will be called.
        /// </para>
        /// </summary>
        /// <param name="alphaNumericLength">A <see cref="System.Int32"/> representing the length for a mixed alphanumberic code.</param>
        /// <param name="alphaLength">A <see cref="System.Int32"/> representing the length of the (alpha) portion of the code.</param>
        /// <param name="numericLength">A <see cref="System.Int32"/> representing the length of the (digit) portion of the code.</param>
        /// <param name="isRandomized">A <see cref="System.Boolean"/> that controls whether or not the AttendanceCodes should be generated randomly or in order (starting from the smallest). Only effect the numeric code.</param>
        /// <returns>A new instance of <see cref="AttendanceCode"/>.</returns>
        /// <exception cref="TimeoutException">Timeout while trying to create a new code.</exception>
        internal AttendanceCode CreateNewCode( int alphaNumericLength, int alphaLength, int numericLength, bool isRandomized )
        {
            lock ( _obj )
            {
                DateTime today = RockDateTime.Today;
                if ( _todaysUsedCodes == null || !_todaysDate.HasValue || !_todaysDate.Value.Equals( today ) )
                {
                    _todaysDate = today;
                    DateTime tomorrow = today.AddDays( 1 );

                    _todaysUsedCodes = new HashSet<string>( Queryable().AsNoTracking().Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow ).Select( c => c.Code ).ToList() );
                }

                string alphaNumericCode = string.Empty;
                string alphaCode = string.Empty;
                string numericCode = string.Empty;
                string code = string.Empty;
                string lastCode = string.Empty;

                for ( int attempts = 0; attempts <= _maxAttempts; attempts++ )
                {
                    if ( attempts == _maxAttempts )
                    {
                        throw new TimeoutException( timeoutExceptionMessage );
                    }

                    if ( alphaNumericLength > 0 )
                    {
                        alphaNumericCode = GenerateRandomCode( alphaNumericLength );
                    }

                    if ( alphaLength > 0 )
                    {
                        alphaCode = GenerateRandomAlphaCode( alphaLength );
                    }

                    if ( numericLength > 0 )
                    {
                        int codeLen = alphaNumericLength + alphaLength + numericLength;

                        if ( lastCode.IsNullOrWhiteSpace() )
                        {
                            lastCode = _todaysUsedCodes.Where( c => c.Length == codeLen ).OrderBy( c => c.Substring( alphaNumericLength + alphaLength ) ).LastOrDefault();
                        }

                        numericCode = GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
                    }

                    code = alphaNumericCode + alphaCode + numericCode;

                    // Check if code is already in use or contains bad/non-allowed strings.
                    if ( NoGood.Any( s => code.Contains( s ) ) || _todaysUsedCodes.Contains( code ) )
                    {
                        lastCode = code;
                        alphaNumericCode = string.Empty;
                        alphaCode = string.Empty;
                        numericCode = string.Empty;
                        code = string.Empty;
                        continue;
                    }

                    // When using a clustered environment we need to check the DB to make sure the code hasn't been assigned by another server
                    if ( WebFarm.RockWebFarm.IsEnabled() )
                    {
                        if ( IsCodeAlreadyInUse( code ) )
                        {
                            lastCode = code;
                            alphaNumericCode = string.Empty;
                            alphaCode = string.Empty;
                            numericCode = string.Empty;
                            code = string.Empty;
                            continue;
                        }
                    }

                    // If we get to this point the code can be used
                    break;
                }

                _todaysUsedCodes.Add( code );

                var attendanceCode = new AttendanceCode();
                attendanceCode.IssueDateTime = RockDateTime.Now;
                attendanceCode.Code = code;

                Add( attendanceCode );
                Context.SaveChanges();

                return attendanceCode;
            }
        }

        /// <summary>
        /// Gets the next numeric code as string.
        /// </summary>
        /// <param name="alphaNumericLength">Length of the alpha numeric.</param>
        /// <param name="alphaLength">Length of the alpha.</param>
        /// <param name="numericLength">Length of the numeric.</param>
        /// <param name="isRandomized">if set to <c>true</c> [is randomized].</param>
        /// <param name="lastCode">The last code.</param>
        /// <returns></returns>
        public static string GetNextNumericCodeAsString( int alphaNumericLength, int alphaLength, int numericLength, bool isRandomized, string lastCode )
        {
            // Find a good unique numeric code for today
            string numericCode = string.Empty;
            int attempts = 0;

            if ( isRandomized )
            {
                numericCode = GenerateRandomNumericCode( numericLength );

                // Leaving the noGood check here because it is possible that this method used outside of GetNew().
                /*
                     4/4/2022 - NA

                     Formerly, the numeric portion of the code was ALSO being checked to verify it was not
                     *contained* within any of the _todaysUsedCodes. Not only was that was not intuitive, it
                     lead to situations where use of only 1 or 2 numeric codes would immediately run out of 
                     codes since the comparison would ignore the other parts of the full code (i.e., any
                     alphanumeric or alpha prefixed characters which otherwise would make the new code unique).

                     Therefore this is being changed to only verify that the numeric code is not in the NoGood
                     list.

                     Reason: Nothing less than a "4" could be practically used -- even if using a alphanumeric or alpha
                             prefix.
                */
                while ( NoGood.Any( s => numericCode.Contains( s ) ) )
                {
                    attempts++;

                    // We're only going to attempt this a million times...
                    if ( attempts > _maxAttempts )
                    {
                        throw new TimeoutException( timeoutExceptionMessage );
                    }

                    numericCode = GenerateRandomNumericCode( numericLength );
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty( lastCode ) )
                {
                    var maxCode = lastCode.Substring( alphaNumericLength + alphaLength );
                    int nextCode = maxCode.AsInteger() + 1;
                    numericCode = nextCode.ToString( "D" + numericLength );
                    if ( numericCode.Length > numericLength )
                    {
                        throw new Exception( $"Error generating numeric check-in code {numericCode}. The number of digits exceeds the configured length of {numericLength}. Check the check-in system 'Security Code Length to adjust this." );
                    }

                    while ( NoGood.Any( s => numericCode.Contains( s ) ) )
                    {
                        attempts++;

                        // We're only going to attempt this a million times...
                        if ( attempts > _maxAttempts )
                        {
                            throw new TimeoutException( timeoutExceptionMessage );
                        }

                        nextCode++;
                        numericCode = nextCode.ToString( "D" + numericLength );
                    }
                }
                else
                {
                    numericCode = 1.ToString( "D" + numericLength );
                }
            }

            return numericCode;
        }

        /// <summary>
        /// Generates a random (security) code.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private static string GenerateRandomCode( int length )
        {
            StringBuilder sb = new StringBuilder();

            int poolSize = _codeCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( _codeCharacters[_random.Next( poolSize )] );
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

            int poolSize = _alphaCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( _alphaCharacters[_random.Next( poolSize )] );
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

            int poolSize = _numericCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( _numericCharacters[_random.Next( poolSize )] );
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
                _todaysUsedCodes = null;
            }
        }
    }
}
