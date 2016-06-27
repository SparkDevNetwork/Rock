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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using DDay.iCal;
using DotLiquid;
using DotLiquid.Util;
using Humanizer;
using Humanizer.Localisation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Lava
{
    /// <summary>
    /// 
    /// </summary>
    public static class RockFilters
    {
        #region String Filters

        /// <summary>
        /// obfuscate a given email
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ObfuscateEmail( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                string[] emailParts = input.Split( '@' );

                if ( emailParts.Length != 2 )
                {
                    return input;
                }
                else
                {
                    return string.Format( "{0}xxxxx@{1}", emailParts[0].Substring( 0, 1 ), emailParts[1] );
                }
            }
        }

        /// <summary>
        /// pluralizes string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Pluralize( string input )
        {
            return input == null
                ? input
                : input.Pluralize();
        }

        /// <summary>
        /// Possessives the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Possessive( string input )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input.EndsWith( "s" ) )
            {
                return input + "'";
            }
            else
            {
                return input + "'s";
            }
        }

        /// <summary>
        /// pluralizes string based on the value for quantity
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string PluralizeForQuantity( string input, object quantity )
        {
            if ( input == null )
            {
                return input;
            }

            decimal numericQuantity;
            if ( quantity is string )
            {
                numericQuantity = ( quantity as string ).AsDecimal();
            }
            else
            {
                numericQuantity = Convert.ToDecimal( quantity );
            }

            if ( numericQuantity > 1 )
            {
                return input.Pluralize();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// convert a integer to a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToString( int input )
        {
            return input.ToString();
        }

        /// <summary>
        /// singularize string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Singularize( string input )
        {
            return input == null
                ? input
                : input.Singularize();
        }

        /// <summary>
        /// takes computer-readible-formats and makes them human readable
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Humanize( string input )
        {
            return input == null
                ? input
                : input.Humanize();
        }

        /// <summary>
        /// returns sentence in 'Title Case'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TitleCase( string input )
        {
            return input == null
                ? input
                : input.Titleize();
        }

        /// <summary>
        /// returns sentence in 'PascalCase'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPascal( string input )
        {
            return input == null
                ? input
                : input.Dehumanize();
        }

        /// <summary>
        /// returns sentence in 'PascalCase'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCssClass( string input )
        {
            return input == null
                ? input
                : input.ToLower().Replace( " ", "-" );
        }

        /// <summary>
        /// returns sentence in 'Sentence case'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SentenceCase( string input )
        {
            return input == null
                ? input
                : input.Transform( To.SentenceCase );
        }

        /// <summary>
        /// takes 1, 2 and returns 1st, 2nd
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinal( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.Ordinalize();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns one, two
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToWords( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToWords();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns first, second
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToOrdinalWords( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToOrdinalWords();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes 1,2 and returns I, II, IV
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NumberToRomanNumerals( string input )
        {
            if ( input == null )
            {
                return input;
            }

            int number;

            if ( int.TryParse( input, out number ) )
            {
                return number.ToRoman();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// formats string to be appropriate for a quantity
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string ToQuantity( string input, object quantity )
        {
            int numericQuantity;
            if ( quantity is string )
            {
                numericQuantity = (int)( ( quantity as string ).AsDecimal() );
            }
            else
            {
                numericQuantity = Convert.ToInt32( quantity );
            }

            return input == null
                ? input
                : input.ToQuantity( numericQuantity );
        }

        /// <summary>
        /// Replace occurrences of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace( object input, object @string, object replacement = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            string replacementString = ( replacement ?? string.Empty ).ToString();
            string pattern = Regex.Escape( @string.ToString() );

            /*// escape common regex meta characters
            var listOfRegExChars = new List<string> { ".", "$", "{", "}", "^", "[", "]", "*", @"\", "+", "|", "?", "<", ">" };
            if ( listOfRegExChars.Contains( @string ) )
            {
                @string = @"\" + @string;
            }*/

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( pattern ) )
            {
                return inputAsString;
            }

            return string.IsNullOrEmpty( inputAsString )
                ? inputAsString
                : Regex.Replace( inputAsString, pattern, replacementString );
        }

        /// <summary>
        /// Replace the first occurrence of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceFirst( object input, string @string, string replacement = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( @string ) )
            {
                return inputAsString;
            }

            // escape common regex meta characters
            var listOfRegExChars = new List<string> { ".", "$", "{", "}", "^", "[", "]", "*", @"\", "+", "|", "?", "<", ">" };
            if ( listOfRegExChars.Contains( @string ) )
            {
                @string = @"\" + @string;
            }

            bool doneReplacement = false;
            return Regex.Replace( inputAsString, @string, m =>
            {
                if ( doneReplacement )
                {
                    return m.Value;
                }

                doneReplacement = true;
                return replacement;
            } );
        }

        /// <summary>
        /// Replace the last occurrence of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="search">The search.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public static string ReplaceLast( object input, string search, string replacement = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( search ) )
            {
                return inputAsString;
            }

            int place = inputAsString.LastIndexOf( search );
            if ( place > 0 )
            {
                return inputAsString.Remove( place, search.Length ).Insert( place, replacement );
            }
            else
            {
                return input.ToString();
            }
        }

        /// <summary>
        /// Remove a substring - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Remove( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? inputAsString
                : inputAsString.Replace( @string, string.Empty );
        }

        /// <summary>
        /// Trims the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Trim( object input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            return input.ToString().Trim();
        }

        /// <summary>
        /// Remove the first occurrence of a substring - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveFirst( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? inputAsString
                : ReplaceFirst( inputAsString, @string, string.Empty );
        }

        /// <summary>
        /// Appends the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="string">The string.</param>
        /// <returns></returns>
        public static string Append( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return inputAsString == null
                ? inputAsString
                : inputAsString + @string;
        }

        /// <summary>
        /// Prepend a string to another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string Prepend( object input, string @string )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            string inputAsString = input.ToString();

            return inputAsString == null
                ? inputAsString
                : @string + inputAsString;
        }

        /// <summary>
        /// Returns the passed default value if the value is undefined or empty, otherwise the value of the variable
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="defaultString">The default string.</param>
        /// <returns></returns>
        public static string Default( object input, string defaultString )
        {
            if ( input == null )
            {
                return defaultString;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? defaultString
                : inputAsString;
        }

        /// <summary>
        /// Decodes an HTML string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string HtmlDecode( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                return HttpUtility.HtmlDecode( input );
            }
        }

        /// <summary>
        /// Converts a string to its escaped representation using Uri.EscapeDataString
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string EscapeDataString( string input )
        {
            if ( input == null )
            {
                return null;
            }
            else
            {
                return Uri.EscapeDataString( input );
            }
        }

        /// <summary>
        /// Tests if the inputted string matches the regex
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expression">The regex expression.</param>
        /// <returns></returns>
        public static bool RegExMatch( string input, string expression )
        {
            if ( input == null )
            {
                return false;
            }

            Regex regex = new Regex( expression );
            Match match = regex.Match( input );

            return match.Success;
        }

        /// <summary>
        /// The slice filter returns a substring, starting at the specified index.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="start">If the passed index is negative, it is counted from the end of the string.</param>
        /// <param name="length">An optional second parameter can be passed to specify the length of the substring.  If no second parameter is given, a substring of one character will be returned.</param>
        /// <returns></returns>
        public static String slice( string input, int start, int length = 1 )
        {
            // If a negative start, subtract if from the length
            if ( start < 0 )
            {
                start = input.Length + start;
            }
            // Make sure start is never < 0
            start = start >= 0 ? start : 0;

            // If length takes us off the end, fix it
            length = length > ( input.Length - start ) ? ( input.Length - start ) : length;

            return input.Substring(start, length);
        }

        #endregion

        #region DateTime Filters

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date( object input, string format )
        {
            if ( input == null )
            {
                return null;
            }

            if ( input.ToString() == "Now" )
            {
                input = RockDateTime.Now.ToString();
            }

            if ( string.IsNullOrWhiteSpace( format ) )
            {
                return input.ToString();
            }

            // if format string is one character add a space since a format string can't be a single character http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx#UsingSingleSpecifiers
            if ( format.Length == 1 )
            {
                format = " " + format;
            }

            DateTime date;

            return DateTime.TryParse( input.ToString(), out date )
                ? Liquid.UseRubyDateFormat ? date.ToStrFTime( format ).Trim() : date.ToString( format ).Trim()
                : input.ToString().Trim();
        }

        /// <summary>
        /// Sundays the date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string SundayDate( object input )
        {
            if ( input == null )
            {
                return null;
            }

            DateTime date = DateTime.MinValue;

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                if ( !DateTime.TryParse( input.ToString(), out date ) )
                {
                    return null;
                }
            }

            if ( date != DateTime.MinValue )
            {
                return date.SundayDate().ToShortDateString();
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Dateses from i cal.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public static List<DateTime> DatesFromICal( object input, object option = null )
        {
            // if no option was specified, default to returning just 1 (to preserve previous behavior)
            option = option ?? 1;

            int returnCount = 1;
            if ( option.GetType() == typeof( int ) )
            {
                returnCount = (int)option;
            }
            else if ( option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the dates
                if ( string.Equals( (string)option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    returnCount = int.MaxValue;
                }
            }

            List<DateTime> nextOccurrences = new List<DateTime>();

            if ( input is string )
            {
                nextOccurrences = GetOccurrenceDates( (string)input, returnCount );
            }
            else if ( input is IList )
            {
                foreach ( var item in input as IList )
                {
                    if ( item is string )
                    {
                        nextOccurrences.AddRange( GetOccurrenceDates( (string)item, returnCount ) );
                    }
                }
            }

            nextOccurrences.Sort( ( a, b ) => a.CompareTo( b ) );

            return nextOccurrences.Take( returnCount ).ToList();
        }

        /// <summary>
        /// Gets the occurrence dates.
        /// </summary>
        /// <param name="iCalString">The i cal string.</param>
        /// <param name="returnCount">The return count.</param>
        /// <returns></returns>
        private static List<DateTime> GetOccurrenceDates( string iCalString, int returnCount )
        {
            iCalendar calendar = iCalendar.LoadFromStream( new StringReader( iCalString ) ).First() as iCalendar;
            DDay.iCal.Event calendarEvent = calendar.Events[0] as Event;

            if ( calendarEvent.DTStart != null )
            {
                List<Occurrence> dates = calendar.GetOccurrences( RockDateTime.Now, RockDateTime.Now.AddYears( 1 ) ).Take( returnCount ).ToList();
                return dates.Select( d => d.Period.StartTime.Value ).ToList();
            }
            else
            {
                return new List<DateTime>();
            }
        }

        /// <summary>
        /// Adds a time interval to a date
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static DateTime? DateAdd( object input, int amount, string interval = "d" )
        {
            DateTime? date = null;

            if ( input == null )
            {
                return null;
            }

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                DateTime d;
                bool success = DateTime.TryParse( input.ToString(), out d );
                if ( success )
                {
                    date = d;
                }
            }

            if ( date.HasValue )
            {
                TimeSpan timeInterval = new TimeSpan();
                switch ( interval )
                {
                    case "d":
                        timeInterval = new TimeSpan( amount, 0, 0, 0 );
                        break;
                    case "h":
                        timeInterval = new TimeSpan( 0, amount, 0, 0 );
                        break;
                    case "m":
                        timeInterval = new TimeSpan( 0, 0, amount, 0 );
                        break;
                    case "s":
                        timeInterval = new TimeSpan( 0, 0, 0, amount );
                        break;
                }

                date = date.Value.Add( timeInterval );
            }

            return date;
        }

        /// <summary>
        /// takes a date time and compares it to RockDateTime.Now and returns a human friendly string like 'yesterday' or '2 hours ago'
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="compareDate">The compare date.</param>
        /// <returns></returns>
        public static string HumanizeDateTime( object input, object compareDate = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            DateTime dtInput;
            DateTime dtCompare;

            if ( input != null && input is DateTime )
            {
                dtInput = (DateTime)input;
            }
            else
            {
                if ( input == null || !DateTime.TryParse( input.ToString(), out dtInput ) )
                {
                    return string.Empty;
                }
            }

            if ( compareDate == null || !DateTime.TryParse( compareDate.ToString(), out dtCompare ) )
            {
                dtCompare = RockDateTime.Now;
            }

            return dtInput.Humanize( true, dtCompare );
        }

        /// <summary>
        /// Dayses from now.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string DaysFromNow( object input )
        {
            DateTime dtInputDate = GetDateFromObject( input ).Date;
            DateTime dtCompareDate = RockDateTime.Now.Date;

            int daysDiff = ( dtInputDate - dtCompareDate ).Days;

            string response = string.Empty;

            switch ( daysDiff )
            {
                case -1:
                    {
                        response = "yesterday";
                        break;
                    }

                case 0:
                    {
                        response = "today";
                        break;
                    }

                case 1:
                    {
                        response = "tomorrow";
                        break;
                    }

                default:
                    {
                        if ( daysDiff > 0 )
                        {
                            response = string.Format( "in {0} days", daysDiff );
                        }
                        else
                        {
                            response = string.Format( "{0} days ago", daysDiff * -1 );
                        }

                        break;
                    }
            }

            return response;
        }

        /// <summary>
        /// takes two datetimes and humanizes the difference like '1 day'. Supports 'Now' as end date
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, object precision )
        {
            if ( precision is String )
            {
                return HumanizeTimeSpan( sStartDate, sEndDate, precision.ToString(), "min" );
            }

            int precisionUnit = 1;

            if ( precision is int )
            {
                precisionUnit = (int)precision;
            }

            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;
                return difference.Humanize( precisionUnit );
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Humanizes the time span.
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="unit">The minimum unit.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, string unit = "Day", string direction = "min" )
        {
            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            TimeUnit unitValue = TimeUnit.Day;

            switch ( unit )
            {
                case "Year":
                    unitValue = TimeUnit.Year;
                    break;
                case "Month":
                    unitValue = TimeUnit.Month;
                    break;
                case "Week":
                    unitValue = TimeUnit.Week;
                    break;
                case "Day":
                    unitValue = TimeUnit.Day;
                    break;
                case "Hour":
                    unitValue = TimeUnit.Hour;
                    break;
                case "Minute":
                    unitValue = TimeUnit.Minute;
                    break;
                case "Second":
                    unitValue = TimeUnit.Second;
                    break;
            }

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;

                if ( direction.ToLower() == "max" )
                {
                    return difference.Humanize( maxUnit: unitValue );
                }
                else
                {
                    return difference.Humanize( minUnit: unitValue );
                }
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Gets the date from object.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private static DateTime GetDateFromObject( object date )
        {
            DateTime oDateTime = DateTime.MinValue;

            if ( date is String )
            {
                if ( (string)date == "Now" )
                {
                    return RockDateTime.Now;
                }
                else
                {
                    if ( DateTime.TryParse( (string)date, out oDateTime ) )
                    {
                        return oDateTime;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
            }
            else if ( date is DateTime )
            {
                return (DateTime)date;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// takes two datetimes and returns the difference in the unit you provide
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        public static Int64? DateDiff( object sStartDate, object sEndDate, string unit )
        {
            DateTime startDate = GetDateFromObject( sStartDate );
            DateTime endDate = GetDateFromObject( sEndDate );

            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;

                switch ( unit )
                {
                    case "d":
                        return (Int64)difference.TotalDays;
                    case "h":
                        return (Int64)difference.TotalHours;
                    case "m":
                        return (Int64)difference.TotalMinutes;
                    case "M":
                        return (Int64)GetMonthsBetween( startDate, endDate );
                    case "Y":
                        return (Int64)( endDate.Year - startDate.Year );
                    case "s":
                        return (Int64)difference.TotalSeconds;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static int GetMonthsBetween( DateTime from, DateTime to )
        {
            if ( from > to )
            {
                return GetMonthsBetween( to, from );
            }

            var monthDiff = Math.Abs( ( to.Year * 12 + ( to.Month - 1 ) ) - ( from.Year * 12 + ( from.Month - 1 ) ) );

            if ( from.AddMonths( monthDiff ) > to || to.Day < from.Day )
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }

        #endregion

        #region Number Filters

        /// <summary>
        /// Formats the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string Format( object input, string format )
        {
            if ( input == null )
            {
                return null;
            }
            else if ( string.IsNullOrWhiteSpace( format ) )
            {
                return input.ToString();
            }

            return string.Format( "{0:" + format + "}", input );
        }

        /// <summary>
        /// Formats the specified input as currency using the CurrencySymbol from Global Attributes
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string FormatAsCurrency( object input )
        {
            if ( input == null )
            {
                return null;
            }

            if ( input is string )
            {
                // if the input is a string, just append the currency symbol to the front, even if it can't be converted to a number
                var currencySymbol = GlobalAttributesCache.Value( "CurrencySymbol" );
                return string.Format( "{0}{1}", currencySymbol, input );
            }
            else
            {
                // if the input an integer, decimal, double or anything else that can be parsed as a decimal, format that
                decimal? inputAsDecimal = input.ToString().AsDecimalOrNull();
                return inputAsDecimal.FormatAsCurrency();
            }
        }

        /// <summary>
        /// Addition - Overriding this to change the logic. The default filter will concat if the type is 
        /// string. This one does the math if the input can be parsed as a int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Plus( object input, object operand )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;
            decimal iOperand = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput + iOperand;
            }
            else
            {
                return string.Concat( input, operand );
            }
        }

        /// <summary>
        /// Minus - Overriding this to change the logic. This one does the math if the input can be parsed as a int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Minus( object input, object operand )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;
            decimal iOperand = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput - iOperand;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Times - Overriding this to change the logic. This one does the math if the input can be parsed as a int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static object Times( object input, object operand )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;
            decimal iOperand = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) && decimal.TryParse( operand.ToString(), out iOperand ) )
            {
                return iInput * iOperand;
            }
            else
            {
                return Enumerable.Repeat( (string)input, (int)operand );
            }
        }

        /// <summary>
        /// Divideds the by.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="operand">The operand.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static object DividedBy( object input, object operand, int precision = 2 )
        {
            if ( input == null || operand == null )
            {
                return null;
            }

            try
            {
                decimal dInput = 0;
                decimal dOperand = 0;

                if ( decimal.TryParse( input.ToString(), out dInput ) && decimal.TryParse( operand.ToString(), out dOperand ) )
                {
                    decimal result = ( dInput / dOperand );
                    return decimal.Round( result, precision );
                }

                return "Could not convert input to number";
            }
            catch ( Exception ex )
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Floors the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object Floor( object input )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) )
            {
                return decimal.Floor( iInput );
            }
            else
            {
                return "Could not convert input to number to round";
            }
        }

        /// <summary>
        /// Ceilings the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object Ceiling( object input )
        {
            if ( input == null )
            {
                return input;
            }

            decimal iInput = -1;

            if ( decimal.TryParse( input.ToString(), out iInput ) )
            {
                return decimal.Ceiling( iInput );
            }
            else
            {
                return "Could not convert input to number to round";
            }
        }

        #endregion

        #region Attribute Filters

        /// <summary>
        /// DotLiquid Attribute Filter
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Attribute( DotLiquid.Context context, object input, string attributeKey, string qualifier = "" )
        {
            IHasAttributes item = null;

            if ( input == null || attributeKey == null )
            {
                return string.Empty;
            }

            // Try to get RockContext from the dotLiquid context
            var rockContext = GetRockContext( context );

            AttributeCache attribute = null;
            string rawValue = string.Empty;

            // If Input is "Global" then look for a global attribute with key
            if ( input.ToString().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
            {
                var globalAttributeCache = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext );
                attribute = globalAttributeCache.Attributes
                    .FirstOrDefault( a => a.Key.Equals( attributeKey, StringComparison.OrdinalIgnoreCase ) );
                if ( attribute != null )
                {
                    // Get the value
                    string theValue = globalAttributeCache.GetValue( attributeKey );
                    if ( theValue.HasMergeFields() )
                    {
                        // Global attributes may reference other global attributes, so try to resolve this value again
                        rawValue = theValue.ResolveMergeFields( new Dictionary<string, object>() );
                    }
                    else
                    {
                        rawValue = theValue;
                    }
                }
            }

            // If input is an object that has attributes, find its attribute value
            else
            {
                if ( input is IHasAttributes )
                {
                    item = (IHasAttributes)input;
                }
                else if ( input is IHasAttributesWrapper )
                {
                    item = ( (IHasAttributesWrapper)input ).HasAttributesEntity;
                }

                if ( item != null )
                {
                    if ( item.Attributes == null )
                    {
                        item.LoadAttributes( rockContext );
                    }

                    if ( item.Attributes.ContainsKey( attributeKey ) )
                    {
                        attribute = item.Attributes[attributeKey];
                        rawValue = item.AttributeValues[attributeKey].Value;
                    }
                }
            }

            // If valid attribute and value were found
            if ( attribute != null && !string.IsNullOrWhiteSpace( rawValue ) )
            {
                Person currentPerson = null;

                // First check for a person override value included in lava context
                if ( context.Scopes != null )
                {
                    foreach ( var scopeHash in context.Scopes )
                    {
                        if ( scopeHash.ContainsKey( "CurrentPerson" ) )
                        {
                            currentPerson = scopeHash["CurrentPerson"] as Person;
                        }
                    }
                }

                if ( currentPerson == null )
                {
                    var httpContext = System.Web.HttpContext.Current;
                    if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                    {
                        currentPerson = httpContext.Items["CurrentPerson"] as Person;
                    }
                }

                if ( attribute.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    // Check qualifier for 'Raw' if present, just return the raw unformatted value
                    if ( qualifier.Equals( "RawValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return rawValue;
                    }

                    // Check qualifier for 'Url' and if present and attribute's field type is a ILinkableFieldType, then return the formatted url value
                    var field = attribute.FieldType.Field;
                    if ( qualifier.Equals( "Url", StringComparison.OrdinalIgnoreCase ) && field is Rock.Field.ILinkableFieldType )
                    {
                        return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( rawValue, attribute.QualifierValues );
                    }

                    // check if attribute is a key value list and return a collection of key/value pairs
                    if ( field is Rock.Field.Types.KeyValueListFieldType )
                    {
                        var keyValueField = (Rock.Field.Types.KeyValueListFieldType)field;

                        return keyValueField.GetValuesFromString( null, rawValue, attribute.QualifierValues, false );
                    }

                    // If qualifier was specified, and the attribute field type is an IEntityFieldType, try to find a property on the entity
                    if ( !string.IsNullOrWhiteSpace( qualifier ) && field is Rock.Field.IEntityFieldType )
                    {
                        IEntity entity = ( (Rock.Field.IEntityFieldType)field ).GetEntity( rawValue );
                        if ( entity != null )
                        {
                            if ( qualifier.Equals( "object", StringComparison.OrdinalIgnoreCase ) )
                            {
                                return entity;
                            }
                            else
                            {
                                return entity.GetPropertyValue( qualifier ).ToStringSafe();
                            }
                        }
                    }

                    // Otherwise return the formatted value
                    return field.FormatValue( null, rawValue, attribute.QualifierValues, false );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Properties the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static object Property( DotLiquid.Context context, object input, string propertyKey, string qualifier = "" )
        {
            if ( input != null )
            {
                return input.GetPropertyValue( propertyKey );
            }

            return string.Empty;
        }

        #endregion

        #region Person Filters

        /// <summary>
        /// Persons the by identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonById( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personId = -1;

            if ( !Int32.TryParse( input.ToString(), out personId ) )
            {
                return null;
            }

            var rockContext = new RockContext();

            return new PersonService( rockContext ).Get( personId );
        }

        /// <summary>
        /// Persons the by unique identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByGuid( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? personGuid = input.ToString().AsGuidOrNull();

            if ( personGuid.HasValue )
            {
                var rockContext = new RockContext();

                return new PersonService( rockContext ).Get( personGuid.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Persons the by alias identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByAliasId( DotLiquid.Context context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personAliasId = -1;

            if ( !Int32.TryParse( input.ToString(), out personAliasId ) )
            {
                return null;
            }

            var rockContext = new RockContext();

            return new PersonAliasService( rockContext ).Get( personAliasId ).Person;
        }

        /// <summary>
        /// Gets the parents of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Parents( DotLiquid.Context context, object input )
        {
            var person = GetPerson( input );

            if ( person != null )
            {
                Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                var parents = new PersonService( new RockContext() ).GetFamilyMembers( person.Id ).Where( m => m.GroupRole.Guid == adultGuid ).Select( a => a.Person );
                return parents.ToList();
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets the children of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Children( DotLiquid.Context context, object input )
        {
            var person = GetPerson( input );

            if ( person != null )
            {
                Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                var children = new PersonService( new RockContext() ).GetFamilyMembers( person.Id ).Where( m => m.GroupRole.Guid == childGuid ).Select( a => a.Person );
                return children.ToList();
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets an address for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Address( DotLiquid.Context context, object input, string addressType, string qualifier = "" )
        {
            var person = GetPerson( input );

            if ( person != null )
            {
                Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

                Location location = null;

                switch ( addressType )
                {
                    case "Mailing":
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupType.Guid == familyGuid )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.IsMailingLocation == true )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                    case "MapLocation":
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupType.Guid == familyGuid )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.IsMappedLocation == true )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                    default:
                        location = new GroupMemberService( GetRockContext( context ) )
                            .Queryable( "GroupLocations.Location" )
                            .AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.Group.GroupType.Guid == familyGuid )
                            .SelectMany( m => m.Group.GroupLocations )
                            .Where( gl =>
                                gl.GroupLocationTypeValue.Value == addressType )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        break;
                }

                if ( location != null )
                {
                    if ( qualifier == "" )
                    {
                        return location.GetFullStreetAddress();
                    }
                    else
                    {
                        var matches = Regex.Matches( qualifier, @"\[\[([^\]]+)\]\]" );
                        foreach ( var match in matches )
                        {
                            string propertyKey = match.ToString().Replace( "[", "" );
                            propertyKey = propertyKey.ToString().Replace( "]", "" );
                            propertyKey = propertyKey.ToString().Replace( " ", "" );

                            switch ( propertyKey )
                            {
                                case "Street1":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street1 );
                                    break;
                                case "Street2":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street2 );
                                    break;
                                case "City":
                                    qualifier = qualifier.Replace( match.ToString(), location.City );
                                    break;
                                case "County":
                                    qualifier = qualifier.Replace( match.ToString(), location.County );
                                    break;
                                case "State":
                                    qualifier = qualifier.Replace( match.ToString(), location.State );
                                    break;
                                case "PostalCode":
                                case "Zip":
                                    qualifier = qualifier.Replace( match.ToString(), location.PostalCode );
                                    break;
                                case "Country":
                                    qualifier = qualifier.Replace( match.ToString(), location.Country );
                                    break;
                                case "GeoPoint":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), string.Format( "{0},{1}", location.GeoPoint.Latitude.ToString(), location.GeoPoint.Longitude.ToString() ) );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Latitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Latitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Longitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Longitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "FormattedAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedAddress );
                                    break;
                                case "FormattedHtmlAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedHtmlAddress );
                                    break;
                                default:
                                    qualifier = qualifier.Replace( match.ToString(), "" );
                                    break;
                            }
                        }

                        return qualifier;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets an number for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="phoneType">Type of the phone number.</param>
        /// <param name="countryCode">Whether or not there should be a country code returned</param>
        /// <returns></returns>
        public static string PhoneNumber( DotLiquid.Context context, object input, string phoneType = "Home", bool countryCode = false )
        {
            var person = GetPerson( input );
            string phoneNumber = null;

            if ( person != null )
            {
                var phoneNumberQuery = new PhoneNumberService( GetRockContext( context ) )
                            .Queryable()
                            .AsNoTracking()
                            .Where( p =>
                               p.PersonId == person.Id )
                            .Where( a => a.NumberTypeValue.Value == phoneType )
                            .FirstOrDefault();

                if ( phoneNumberQuery != null )
                {
                    if ( countryCode && !string.IsNullOrEmpty( phoneNumberQuery.CountryCode ) )
                    {
                        phoneNumber = phoneNumberQuery.NumberFormattedWithCountryCode;
                    }
                    else
                    {
                        phoneNumber = phoneNumberQuery.NumberFormatted;
                    }
                }
            }

            return phoneNumber;
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <returns>A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:LOGO,P,P,{0},,{1} ^FD").</returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size )
        {
            return ZebraPhoto( context, input, size, 1.0, 1.0 );
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <param name="brightness">The brightness adjustment (-1.0 to 1.0).</param>
        /// <param name="contrast">The contrast adjustment (-1.0 to 1.0).</param>
        /// <returns>A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:LOGO,P,P,{0},,{1} ^FD").</returns>
        public static string ZebraPhoto( DotLiquid.Context context, object input, string size, double brightness = 1.0, double contrast = 1.0 )
        {
            var person = GetPerson( input );
            try
            {
                if ( person != null )
                {
                    Stream initialPhotoStream;
                    if ( person.PhotoId.HasValue )
                    {
                        initialPhotoStream = new BinaryFileService( GetRockContext( context ) ).Get( person.PhotoId.Value ).ContentStream;
                    }
                    else
                    {
                        var photoUrl = new StringBuilder();
                        photoUrl.Append( HttpContext.Current.Server.MapPath( "~/" ) );

                        if ( person.Age.HasValue && person.Age.Value < 18 )
                        {
                            // it's a child
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-male.png" );
                            }
                        }
                        else
                        {
                            // it's an adult
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-male.png" );
                            }
                        }

                        initialPhotoStream = File.Open( photoUrl.ToString(), FileMode.Open );
                    }

                    Bitmap initialBitmap = new Bitmap( initialPhotoStream );

                    // Adjust the image if any of the parameters not default
                    if ( brightness != 1.0 || contrast != 1.0 )
                    {
                        initialBitmap = ImageAdjust( initialBitmap, (float)brightness, (float)contrast );
                    }

                    // Calculate rectangle to crop image into
                    int height = initialBitmap.Height;
                    int width = initialBitmap.Width;
                    Rectangle cropSection = new Rectangle( 0, 0, height, width );
                    if ( height < width )
                    {
                        cropSection = new Rectangle( ( width - height ) / 2, 0, ( width + height ) / 2, height ); // (width + height)/2 is a simplified version of the (width - height)/2 + height function
                    }
                    else if ( height > width )
                    {
                        cropSection = new Rectangle( 0, ( height - width ) / 2, width, ( height + width ) / 2 );
                    }

                    // Crop and resize image
                    int pixelSize = size.AsIntegerOrNull() ?? 395;
                    Bitmap resizedBitmap = new Bitmap( pixelSize, pixelSize );
                    using ( Graphics g = Graphics.FromImage( resizedBitmap ) )
                    {
                        g.DrawImage( initialBitmap, new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), cropSection, GraphicsUnit.Pixel );
                    }

                    // Grayscale Image
                    var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
                    var outputBitmap = new Bitmap( resizedBitmap.Width, resizedBitmap.Height, PixelFormat.Format1bppIndexed );
                    var data = new sbyte[resizedBitmap.Width, resizedBitmap.Height];
                    var inputData = resizedBitmap.LockBits( new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb );
                    try
                    {
                        var scanLine = inputData.Scan0;
                        var line = new byte[inputData.Stride];
                        for ( var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride )
                        {
                            Marshal.Copy( scanLine, line, 0, line.Length );
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                // Change to greyscale
                                data[x, y] = (sbyte)( 64 * ( ( ( line[x * 3 + 2] * 0.299 + line[x * 3 + 1] * 0.587 + line[x * 3 + 0] * 0.114 ) / 255 ) - 0.4 ) );
                            }
                        }
                    }
                    finally
                    {
                        resizedBitmap.UnlockBits( inputData );
                    }

                    //Dither Image
                    var outputData = outputBitmap.LockBits( new Rectangle( 0, 0, outputBitmap.Width, outputBitmap.Height ), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed );
                    try
                    {
                        var scanLine = outputData.Scan0;
                        for ( var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride )
                        {
                            var line = new byte[outputData.Stride];
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                var j = data[x, y] > 0;
                                if ( j ) line[x / 8] |= masks[x % 8];
                                var error = (sbyte)( data[x, y] - ( j ? 32 : -32 ) );
                                if ( x < resizedBitmap.Width - 1 ) data[x + 1, y] += (sbyte)( 7 * error / 16 );
                                if ( y < resizedBitmap.Height - 1 )
                                {
                                    if ( x > 0 ) data[x - 1, y + 1] += (sbyte)( 3 * error / 16 );
                                    data[x, y + 1] += (sbyte)( 5 * error / 16 );
                                    if ( x < resizedBitmap.Width - 1 ) data[x + 1, y + 1] += (sbyte)( 1 * error / 16 );
                                }
                            }

                            Marshal.Copy( line, 0, scanLine, outputData.Stride );
                        }
                    }
                    finally
                    {
                        outputBitmap.UnlockBits( outputData );
                    }

                    // Convert from x to .png
                    MemoryStream convertedStream = new MemoryStream();
                    outputBitmap.Save( convertedStream, System.Drawing.Imaging.ImageFormat.Png );
                    convertedStream.Seek( 0, SeekOrigin.Begin );

                    // Convert the .png stream into a ZPL-readable Hex format
                    var content = convertedStream.ReadBytesToEnd();
                    StringBuilder zplImageData = new StringBuilder();

                    foreach ( Byte b in content )
                    {
                        string hexRep = string.Format( "{0:X}", b );
                        if ( hexRep.Length == 1 )
                            hexRep = "0" + hexRep;
                        zplImageData.Append( hexRep );
                    }

                    convertedStream.Dispose();
                    initialPhotoStream.Dispose();

                    return string.Format( "^FS ~DYE:LOGO,P,P,{0},,{1} ^FD", content.Length, zplImageData.ToString() );
                }
            }
            catch
            {
                // intentially blank
            }

            return string.Empty;
        }

        /// <summary>
        /// Adjust the brightness, contrast or gamma of the given image.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <param name="brightness">The brightness multiplier (-1.99 to 1.99 fully white).</param>
        /// <param name="contrast">The contrast multiplier (2.0 would be twice the contrast).</param>
        /// <param name="gamma">The gamma multiplier (1.0 would no change in gamma).</param>
        /// <returns>A new adjusted image.</returns>
        private static Bitmap ImageAdjust( Bitmap originalImage, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f )
        {
            Bitmap adjustedImage = originalImage;

            float adjustedBrightness = brightness - 1.0f;
            // Matrix used to effect the image
            float[][] ptsArray = {
                new float[] { contrast, 0, 0, 0, 0 }, // scale red
                new float[] { 0, contrast, 0, 0, 0 }, // scale green
                new float[] { 0, 0, contrast, 0, 0 }, // scale blue
                new float[] { 0, 0, 0, 1.0f, 0 },     // no change to alpha
                new float[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix( new ColorMatrix( ptsArray ), ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
            imageAttributes.SetGamma( gamma, ColorAdjustType.Bitmap );
            Graphics g = Graphics.FromImage( adjustedImage );
            g.DrawImage( originalImage, new Rectangle( 0, 0, adjustedImage.Width, adjustedImage.Height ),
                0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes );

            return adjustedImage;
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Groups( DotLiquid.Context context, object input, string groupTypeId, string status = "Active" )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                var groupQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "Group, GroupRole" ).AsNoTracking()
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == numericalGroupTypeId.Value &&
                        m.Group.IsActive );

                if ( status != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = (GroupMemberStatus)Enum.Parse( typeof( GroupMemberStatus ), status, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Groups the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Group( DotLiquid.Context context, object input, string groupId, string status = "Active" )
        {
            var person = GetPerson( input );
            int? numericalGroupId = groupId.AsIntegerOrNull();

            if ( string.IsNullOrWhiteSpace( status ) )
            {
                status = "All";
            }

            if ( person != null && numericalGroupId.HasValue )
            {
                var groupQuery = new GroupMemberService( GetRockContext( context ) )
                    .Queryable( "Group, GroupRole" ).AsNoTracking()
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.Id == numericalGroupId.Value &&
                        m.Group.IsActive );

                if ( status != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = (GroupMemberStatus)Enum.Parse( typeof( GroupMemberStatus ), status, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of which they have attended at least once
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GroupsAttended( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new AttendanceService( GetRockContext( context ) ).Queryable().AsNoTracking()
                    .Where( a => a.Group.GroupTypeId == numericalGroupTypeId && a.PersonAlias.PersonId == person.Id && a.DidAttend == true )
                    .Select( a => a.Group ).Distinct().ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the last attendance item for a given person in a group of type provided
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Attendance LastAttendedGroupOfType( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                var attendance = new AttendanceService( GetRockContext( context ) ).Queryable( "Group" ).AsNoTracking()
                    .Where( a => a.Group.GroupTypeId == numericalGroupTypeId && a.PersonAlias.PersonId == person.Id && a.DidAttend == true )
                    .OrderByDescending( a => a.StartDateTime ).FirstOrDefault();

                return attendance;
            }

            return new Attendance();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GeofencingGroups( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupTypeRoleId">The group type role identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Person> GeofencingGroupMembers( DotLiquid.Context context, object input, string groupTypeId, string groupTypeRoleId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();
            int? numericalGroupTypeRoleId = groupTypeRoleId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue && numericalGroupTypeRoleId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .SelectMany( g => g.Members.Where( m => m.GroupRole.Id == numericalGroupTypeRoleId ) )
                    .Select( m => m.Person )
                    .ToList();
            }

            return new List<Model.Person>();
        }

        /// <summary>
        /// Returnes the nearest group of a specific type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Rock.Model.Group NearestGroup( DotLiquid.Context context, object input, string groupTypeId )
        {
            var person = GetPerson( input );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( GetRockContext( context ) )
                    .GetNearestGroup( person.Id, numericalGroupTypeId.Value );
            }

            return null;
        }

        /// <summary>
        /// Returns the Campus (or Campuses) that the Person belongs to
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public static object Campus( DotLiquid.Context context, object input, object option = null )
        {
            var person = GetPerson( input );

            bool getAll = false;
            if ( option != null && option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the campuses (if they are part of multiple families from different campuses)
                if ( string.Equals( (string)option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    getAll = true;
                }
            }

            if ( getAll )
            {
                return person.GetFamilies().Select( a => a.Campus ).OrderBy( a => a.Name );
            }
            else
            {
                return person.GetCampus();
            }
            
        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static RockContext GetRockContext( DotLiquid.Context context )
        {
            if ( context.Registers.ContainsKey( "rock_context" ) )
            {
                return context.Registers["rock_context"] as RockContext;
            }
            else
            {
                var rockContext = new RockContext();
                context.Registers["rock_context"] = rockContext;
                return rockContext;
            }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static Person GetPerson( object input )
        {
            if ( input != null )
            {
                var person = input as Person;
                if ( person != null )
                {
                    return person;
                }

                var checkinPerson = input as CheckIn.CheckInPerson;
                if ( checkinPerson != null )
                {
                    return checkinPerson.Person;
                }
            }

            return null;
        }

        #endregion

        #region Misc Filters

        /// <summary>
        /// Redirects the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string PageRedirect( string input )
        {
            // check for no redirect in query string
            string redirectValue = HttpContext.Current.Request.QueryString["Redirect"];

            if ( redirectValue != null && redirectValue == "false" )
            {
                return string.Format( "<p class='alert alert-warning'>Without the redirect query string parameter you would be redirected to: <a href='{0}'>{0}</a>.</p>", input );
            }

            if ( input != null )
            {
                HttpContext.Current.Response.Redirect( input, true );
            }

            return string.Empty;
        }

        /// <summary>
        /// creates a postback javascript function
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static string Postback( object input, string command )
        {
            if ( input != null )
            {
                return string.Format( "javascript:__doPostBack('[ClientId]','{0}^{1}'); return false;", command, input.ToString() );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ToJSON( object input )
        {
            return input.ToJson();
        }

        /// <summary>
        /// Returns a dynamic object from a JSON string
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static object FromJSON( object input )
        {
            var converter = new ExpandoObjectConverter();
            object contentObject = null;
            var value = input as string;

            try
            {
                // first try to deserialize as straight ExpandoObject
                contentObject = JsonConvert.DeserializeObject<ExpandoObject>( value, converter );
            }
            catch
            {
                try
                {
                    // if it didn't deserialize as straight ExpandoObject, try it as a List of ExpandoObjects
                    contentObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( value, converter );
                }
                catch
                {
                    // if it didn't deserialize as a List of ExpandoObject, try it as a List of plain objects
                    contentObject = JsonConvert.DeserializeObject<List<object>>( value, converter );
                }
            }

            return contentObject;
        }

        /// <summary>
        /// Converts Markdown to HTML
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string FromMarkdown( string input )
        {
            if ( input != null )
            {
                return input.ConvertMarkdownToHtml();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// adds a meta tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the content attribute of the tag.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static string AddMetaTagToHead( string input, string attributeName, string attributeValue )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                HtmlMeta metaTag = new HtmlMeta();
                metaTag.Attributes.Add( attributeName, attributeValue );
                metaTag.Content = input;
                page.Header.Controls.Add( metaTag );
            }

            return null;
        }

        /// <summary>
        /// adds a link tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static string AddLinkTagToHead( string input, string attributeName, string attributeValue )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                HtmlLink imageLink = new HtmlLink();
                imageLink.Attributes.Add( attributeName, attributeValue );
                imageLink.Attributes.Add( "href", input );
                page.Header.Controls.Add( imageLink );
            }

            return null;
        }

        /// <summary>
        /// adds a link tag to the head of the document
        /// </summary>
        /// <param name="input">The input to use for the href of the tag.</param>
        /// <returns></returns>
        public static string SetPageTitle( string input )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                page.BrowserTitle = input;
                page.PageTitle = input;
                page.Header.Title = input;
            }

            return null;
        }

        /// <summary>
        /// Pages the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parm">The parm.</param>
        /// <returns></returns>
        public static object Page( string input, string parm )
        {
            RockPage page = HttpContext.Current.Handler as RockPage;

            if ( page != null )
            {
                switch ( parm )
                {
                    case "Title":
                        {
                            return page.BrowserTitle;
                        }

                    case "Url":
                        {
                            return HttpContext.Current.Request.Url.AbsoluteUri;
                        }

                    case "Id":
                        {
                            return page.PageId.ToString();
                        }

                    case "Host":
                        {
                            return HttpContext.Current.Request.Url.Host;
                        }

                    case "Path":
                        {
                            return HttpContext.Current.Request.Url.AbsolutePath;
                        }

                    case "SiteName":
                        {
                            return page.Site.Name;
                        }

                    case "SiteId":
                        {
                            return page.Site.Id.ToString();
                        }

                    case "Theme":
                        {
                            if ( page.Theme != null )
                            {
                                return page.Theme;
                            }
                            else
                            {
                                return page.Site.Theme;
                            }
                        }

                    case "Layout":
                        {
                            return page.Layout.Name;
                        }

                    case "Scheme":
                        {
                            return HttpContext.Current.Request.Url.Scheme;
                        }

                    case "QueryString":
                        {
                            var test = page.PageParameters();
                            return page.PageParameters();
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a lava property to a key value pair
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Dictionary<string, object> PropertyToKeyValue( object input )
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var type = input.GetType();

            if ( type == typeof( KeyValuePair<string, object> ) )
            {
                var key = type.GetProperty( "Key" );
                var value = type.GetProperty( "Value" );

                result.Add( "Key", key.GetValue( input, null ).ToString() );
                result.Add( "Value", value.GetValue( input, null ) );
            }

            return result;
        }

        #endregion

        #region Array Filters

        /// <summary>
        /// Rearranges an array in a random order
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object Shuffle( object input )
        {
            if ( input == null )
            {
                return input;
            }

            if ( !( input is IList ) )
            {
                return input;
            }

            var inputList = input as IList;
            Random rng = new Random();
            int n = inputList.Count;
            while ( n > 1 )
            {
                n--;
                int k = rng.Next( n + 1 );
                var value = inputList[k];
                inputList[k] = inputList[n];
                inputList[n] = value;
            }

            return inputList;
        }

        /// <summary>
        /// Wheres the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns></returns>
        public static object Where( object input, string filterKey, object filterValue )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( (IEnumerable)input ) )
                {
                    if ( value is ILiquidizable )
                    {
                        var liquidObject = value as ILiquidizable;
                        if ( liquidObject.ContainsKey( filterKey ) && liquidObject[filterKey].Equals( filterValue ) )
                        {
                            result.Add( liquidObject );
                        }
                    }
                    else if (value is IDictionary<string, object>)
                    {
                        var dictionaryObject = value as IDictionary<string, object>;
                        if ( dictionaryObject.ContainsKey( filterKey ) && dictionaryObject[filterKey].Equals( filterValue ) )
                        {
                            result.Add( dictionaryObject );
                        }
                    }
                }

                return result;
            }

            return input;
        }

        /// <summary>
        /// Selects the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="selectKey">The select key.</param>
        /// <returns></returns>
        public static object Select( object input, string selectKey )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( (IEnumerable)input ) )
                {
                    if ( value is ILiquidizable )
                    {
                        var liquidObject = value as ILiquidizable;
                        if ( liquidObject.ContainsKey( selectKey ) )
                        {
                            result.Add( liquidObject[selectKey] );
                        }
                    }
                    else if ( value is IDictionary<string, object> )
                    {
                        var dictionaryObject = value as IDictionary<string, object>;
                        if ( dictionaryObject.ContainsKey( selectKey ) && dictionaryObject[selectKey].Equals( selectKey ) )
                        {
                            result.Add( dictionaryObject );
                        }
                    }
                }

                return result;
            }

            return input;
        }

        /// <summary>
        /// Sorts the list of items by the specified attribute's value
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        public static object SortByAttribute( DotLiquid.Context context, object input, string attributeKey )
        {
            if ( input is IEnumerable )
            {
                var rockContext = GetRockContext( context );
                var inputList = ( input as IEnumerable ).OfType<Rock.Attribute.IHasAttributes>().ToList();
                foreach ( var item in inputList )
                {
                    if ( item.Attributes == null )
                    {
                        item.LoadAttributes( rockContext );
                    }
                }

                if ( inputList.Count > 1 && inputList[0].Attributes.ContainsKey( attributeKey ) )
                {
                    var attributeCache = inputList[0].Attributes[attributeKey];

                    inputList.Sort( ( item1, item2 ) =>
                    {
                        var item1AttributeValue = item1.AttributeValues.Where( a => a.Key == attributeKey ).FirstOrDefault().Value.SortValue;
                        var item2AttributeValue = item2.AttributeValues.Where( a => a.Key == attributeKey ).FirstOrDefault().Value.SortValue;
                        if ( item1AttributeValue is IComparable && item2AttributeValue is IComparable )
                        {
                            return ( item1AttributeValue as IComparable ).CompareTo( item2AttributeValue as IComparable );
                        }
                        else
                        {
                            return 0;
                        }
                    } );
                }

                return inputList;
            }
            else
            {
                return input;
            }
        }

        #endregion
    }
}
