//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Attendance Code POCO Service class
    /// </summary>
    public partial class AttendanceCodeService
    {
        private static readonly Object obj = new object();

        private char[] codeCharacters = new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'X', 'Z', '2', '4', '5', '6', '7', '8', '9' };
        private List<string> noGood = new List<string> { "666", "KKK" };

        public IQueryable<AttendanceCode> Get( DateTime day, string code )
        {
            DateTime today = day.Date;
            DateTime tomorrow = today.AddDays( 1 );
            return Repository.AsQueryable().Where( c => c.Code == code && c.IssueDateTime <= today && c.IssueDateTime < tomorrow);
        }

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
