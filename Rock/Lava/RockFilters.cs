﻿// <copyright>
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
using System.Reflection;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Util;
using Humanizer;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// 
    /// </summary>
    public static class RockFilters
    {
        #region String Filters

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
                return input;

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
                return input;

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
                return input;

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
                return input;

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
        public static string ToQuantity( string input, int quantity )
        {
            return input == null
                ? input
                : input.ToQuantity( quantity );
        }

        /// <summary>
        /// Replace occurrences of a string with another - this is a Rock version on this filter which takes any object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="string"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace( object input, string @string, string replacement = "" )
        {
            if ( input == null )
            {
                return string.Empty;
            }
            
            string inputAsString = input.ToString();

            // escape common regex meta characters
            var listOfRegExChars = new List<string> { ".", "$", "{", "}", "^", "[", "]", "*", @"\", "+", "|", "?", "<", ">" };
            if ( listOfRegExChars.Contains( @string ) )
            {
                @string = @"\" + @string;
            }

            if ( string.IsNullOrEmpty( inputAsString ) || string.IsNullOrEmpty( @string ) )
                return inputAsString;

            return string.IsNullOrEmpty( inputAsString )
                ? inputAsString
                : Regex.Replace( inputAsString, @string, replacement );
        }

        /// <summary>
        /// Replace the first occurence of a string with another - this is a Rock version on this filter which takes any object
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
                return inputAsString;

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
                    return m.Value;

                doneReplacement = true;
                return replacement;
            } );
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

            if ( input == null ) {
                return defaultString;
            }

            string inputAsString = input.ToString();

            return string.IsNullOrWhiteSpace( inputAsString )
                ? defaultString
                : inputAsString;
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
                return null;

            if ( input.ToString() == "Now" )
            {
                input = RockDateTime.Now.ToString();
            }

            if ( string.IsNullOrWhiteSpace( format ) )
                return input.ToString();

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
        /// takes a date time and compares it to RockDateTime.Now and returns a human friendly string like 'yesterday' or '2 hours ago'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HumanizeDateTime( object input )
        {
            if ( input == null )
                return string.Empty;

            DateTime dtInput;

            if ( input is DateTime )
            {
                dtInput = (DateTime)input;
            }
            else
            {
                if ( !DateTime.TryParse( input.ToString(), out dtInput ) )
                {
                    return string.Empty;
                }
            }

            return dtInput.Humanize( false, RockDateTime.Now );

        }

        /// <summary>
        /// takes two datetimes and humanizes the difference like '1 day'. Supports 'Now' as end date
        /// </summary>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        /// <param name="precision">The precision.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, int precision = 1 )
        {
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;

            // convert start date if string
            if ( sStartDate is String )
            {
                if ( (string)sStartDate == "Now" )
                {
                    startDate = RockDateTime.Now;
                }
                else
                {
                    if ( !DateTime.TryParse( (string)sStartDate, out startDate ) )
                    {
                        return null;
                    }
                }
            }
            else if ( sStartDate is DateTime )
            {
                startDate = (DateTime)sStartDate;
            }

            // convert end date if string
            if ( sEndDate is String )
            {
                if ( (string)sEndDate == "Now" )
                {
                    endDate = RockDateTime.Now;
                }
                else
                {
                    if ( !DateTime.TryParse( (string)sEndDate, out endDate ) )
                    {
                        return null;
                    }
                }
            }
            else if ( sEndDate is DateTime )
            {
                endDate = (DateTime)sEndDate;
            }


            if ( startDate != DateTime.MinValue && endDate != DateTime.MinValue )
            {
                TimeSpan difference = endDate - startDate;
                return difference.Humanize( precision );
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
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

            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;

            // convert start date if string
            if ( sStartDate is String )
            {
                if ( (string)sStartDate == "Now" )
                {
                    startDate = RockDateTime.Now;
                }
                else
                {
                    if ( !DateTime.TryParse( (string)sStartDate, out startDate ) )
                    {
                        return null;
                    }
                }
            }
            else if ( sStartDate is DateTime )
            {
                startDate = (DateTime)sStartDate;
            }

            // convert end date if string
            if ( sEndDate is String )
            {
                if ( (string)sEndDate == "Now" )
                {
                    endDate = RockDateTime.Now;
                }
                else
                {
                    if ( !DateTime.TryParse( (string)sEndDate, out endDate ) )
                    {
                        return null;
                    }
                }
            }
            else if ( sEndDate is DateTime )
            {
                endDate = (DateTime)sEndDate;
            }



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
            if ( from > to ) return GetMonthsBetween( to, from );

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
            if ( input == null || attributeKey == null )
            {
                return string.Empty;
            }

            // Try to get RockContext from the dotLiquid context
            var rockContext = GetRockContext(context);

            AttributeCache attribute = null;
            string rawValue = string.Empty;

            // If Input is "Global" then look for a global attribute with key
            if (input.ToString().Equals( "Global", StringComparison.OrdinalIgnoreCase ) )
            {
                var globalAttributeCache = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext );
                attribute = globalAttributeCache.Attributes
                    .FirstOrDefault( a => a.Key.Equals(attributeKey, StringComparison.OrdinalIgnoreCase));
                if (attribute != null )
                {
                    rawValue = globalAttributeCache.GetValue( attributeKey );
                }
            }

            // If input is an object that has attributes, find it's attribute value
            else if ( input is IHasAttributes)
            {
                var item = (IHasAttributes)input;
                if ( item.Attributes == null)
                {
                    item.LoadAttributes( rockContext );
                }

                if ( item.Attributes.ContainsKey(attributeKey))
                {
                    attribute = item.Attributes[attributeKey];
                    rawValue = item.AttributeValues[attributeKey].Value;
                }
            }

            // If valid attribute and value were found
            if ( attribute != null && 
                !string.IsNullOrWhiteSpace(rawValue) &&
                attribute.IsAuthorized( Authorization.VIEW, null ) )
            {
                // Check qualifier for 'Raw' if present, just return the raw unformatted value
                if ( qualifier.Equals("RawValue", StringComparison.OrdinalIgnoreCase) )
                {
                    return rawValue;
                }

                // Check qualifier for 'Url' and if present and attribute's field type is a ILinkableFieldType, then return the formatted url value
                var field = attribute.FieldType.Field;
                if ( qualifier.Equals("Url", StringComparison.OrdinalIgnoreCase) && field is Rock.Field.ILinkableFieldType )
                {
                    return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( rawValue, attribute.QualifierValues );
                }

                // If qualifier was specified, and the attribute field type is an IEntityFieldType, try to find a property on the entity
                if ( !string.IsNullOrWhiteSpace(qualifier) && field is Rock.Field.IEntityFieldType )
                {
                    IEntity entity = ( (Rock.Field.IEntityFieldType)field ).GetEntity( rawValue );
                    if (entity != null)
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
                return input.GetPropertyValue(propertyKey);
            }
            return string.Empty;
        }

        #endregion

        #region Person Filters

        /// <summary>
        /// Addresses the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Address( DotLiquid.Context context, object input, string addressType, string qualifier = "" )
        {
            if ( input != null && input is Person )
            {
                var person = (Person)input;
                

                Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var location = new GroupMemberService( GetRockContext(context) )
                    .Queryable( "GroupLocations.Location" )
                    .Where( m => 
                        m.PersonId == person.Id && 
                        m.Group.GroupType.Guid == familyGuid )
                    .SelectMany( m => m.Group.GroupLocations )
                    .Where( gl => 
                        gl.GroupLocationTypeValue.Value == addressType )
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

                if (location != null)
                {
                    if ( qualifier == "" )
                    {
                        return location.GetFullStreetAddress();
                    }
                    else
                    {
                        var matches = Regex.Matches(qualifier, @"\[\[([^\]]+)\]\]");
                        foreach ( var match in matches )
                        {
                            string propertyKey = match.ToString().Replace("[", "");
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
                                        qualifier = qualifier.Replace( match.ToString(), string.Format("{0},{1}", location.GeoPoint.Latitude.ToString(), location.GeoPoint.Longitude.ToString()) );
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
        /// Gets the rock context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static RockContext GetRockContext( DotLiquid.Context context)
        {
            if ( context.Registers.ContainsKey("rock_context"))
            {
                return context.Registers["rock_context"] as RockContext;
            }
            else
            {
                var rockContext = new RockContext();
                context.Registers.Add( "rock_context", rockContext );
                return rockContext;
            }
        }

        #endregion
    }
}
