//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        private char[] codeCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z', '2', '4', '5', '6', '7', '8', '9' };
        /// <summary>
        /// A list of <see cref="Rock.Model.String"/> values that are not allowable as attendance codes.
        /// </summary>
        private List<string> noGood = new List<string> { "666", "KKK", "FCK", "SHT", "5HT", "DCK" };

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
            return Repository.AsQueryable().Where( c => c.Code == code && c.IssueDateTime <= today && c.IssueDateTime < tomorrow);
        }

        /// <summary>
        /// Returns a new <see cref="Rock.Model.Code"/>
        /// </summary>
        /// <param name="codeLength">A <see cref="System.Int32"/> representing the length of the (security) code.</param>
        /// <returns>A new <see cref="Rock.Model.AttendanceCode"/></returns>
        public AttendanceCode GetNew(int codeLength = 3)
        {
            string code = string.Empty;

            var attendanceCode = new AttendanceCode();

            // Make sure only one instance at a time is checking for unique code
            lock (obj)
            {
                // Find a good unique code for today
                while ( code == string.Empty ||
                    noGood.Any( s => s == code ) ||
                    Get(DateTime.Today, code).Any())
                {
                    code = GenerateRandomCode( codeLength );
                }

                attendanceCode.IssueDateTime = DateTime.Now;
                attendanceCode.Code = code;
                this.Add( attendanceCode, null );
                this.Save( attendanceCode, null );
            }

            return attendanceCode;
        }

        /// <summary>
        /// Generates a random (security) code.
        /// </summary>
        /// <param name="length">A <see cref="System.Int32"/> representing the length that the code needs to be.</param>
        /// <returns>A <see cref="System.String"/> representing the (security) code.</returns>
        private string GenerateRandomCode( int length )
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
