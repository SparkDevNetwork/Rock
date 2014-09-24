using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DotLiquid.Util;
using Humanizer;

namespace DotLiquid
{
	public static class StandardFilters
	{
		/// <summary>
		/// Return the size of an array or of an string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static int Size(object input)
		{
			if (input is string)
				return ((string) input).Length;
			if (input is IEnumerable)
				return ((IEnumerable) input).Cast<object>().Count();
			return 0;
		}

		/// <summary>
		/// convert a input string to DOWNCASE
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Downcase(string input)
		{
			return input == null ? input : input.ToLower();
		}

		/// <summary>
		/// convert a input string to UPCASE
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Upcase(string input)
		{
			return input == null
				? input
				: input.ToUpper();
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
        /// takes a date time and compares it to RockDateTime.Now and returns a human friendly string like 'yesterday' or '2 hours ago'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HumanizeDateTime( string input )
        {
            if ( input == null )
                return input;

            DateTime dateProvided;

            if ( DateTime.TryParse(input.ToString(), out dateProvided ))
            {
                return dateProvided.Humanize( false, RockDateTime.Now );
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// takes two datetimes and humanizes the difference like '1 day'. Supports 'Now' as end date
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( string sStartDate, string sEndDate, int precision =  1)
        {
            if ( string.IsNullOrWhiteSpace(sStartDate) || string.IsNullOrWhiteSpace(sEndDate) )
                return "Two dates must be provided";

            if ( sEndDate == "Now" )
            {
                sEndDate = RockDateTime.Now.ToString();
            }

            DateTime startDate, endDate;

            if ( DateTime.TryParse( sStartDate, out startDate ) && DateTime.TryParse(sEndDate, out endDate ))
            {
                TimeSpan difference = endDate - startDate;
                return difference.Humanize(precision);
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// takes two datetimes and returns the difference in the unit you provide
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Int64? DateDiff( string sStartDate, string sEndDate, string unit )
        {
            if ( string.IsNullOrWhiteSpace( sStartDate ) || string.IsNullOrWhiteSpace( sEndDate ) )
                return null;

            if ( sEndDate == "Now" )
            {
                sEndDate = RockDateTime.Now.ToString();
            }

            if ( sStartDate == "Now" )
            {
                sStartDate = RockDateTime.Now.ToString();
            }

            DateTime startDate, endDate;

            if ( DateTime.TryParse( sStartDate, out startDate ) && DateTime.TryParse( sEndDate, out endDate ) )
            {
                TimeSpan difference = endDate - startDate;

                switch ( unit )
                {
                    case "D":
                        return (Int64)difference.TotalDays;
                    case "H":
                        return (Int64)difference.TotalHours;
                    case "M":
                        return (Int64)difference.TotalMinutes;
                    case "S":
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
        public static string PascalCase( string input )
        {
            return input == null
                ? input
                : input.Dehumanize();
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
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToQuantity( string input, int quantity )
        {
            return input == null
                ? input
                : input.ToQuantity( quantity );
        }

		/// <summary>
		/// capitalize words in the input sentence
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Capitalize(string input)
		{
			if (input.IsNullOrWhiteSpace())
				return input;

			return string.IsNullOrEmpty(input)
				? input
				: CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
		}

		public static string Escape(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			try
			{
				return WebUtility.HtmlEncode(input);
			}
			catch
			{
				return input;
			}
		}

		public static string H(string input)
		{
			return Escape(input);
		}

		/// <summary>
		/// Truncates a string down to x characters
		/// </summary>
		/// <param name="input"></param>
		/// <param name="length"></param>
		/// <param name="truncateString"></param>
		/// <returns></returns>
		public static string Truncate(string input, int length = 50, string truncateString = "...")
		{
			if (string.IsNullOrEmpty(input))
				return input;

			int l = length - truncateString.Length;

			return input.Length > length
				? input.Substring(0, l < 0 ? 0 : l) + truncateString
				: input;
		}

		public static string TruncateWords(string input, int words = 15, string truncateString = "...")
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var wordList = input.Split(' ').ToList();
			int l = words < 0 ? 0 : words;

			return wordList.Count > l
				? string.Join(" ", wordList.Take(l).ToArray()) + truncateString
				: input;
		}

		/// <summary>
		/// Split input string into an array of substrings separated by given pattern.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static string[] Split(string input, string pattern)
		{
			return input.IsNullOrWhiteSpace()
				? new[] { input }
				: input.Split(new[] { pattern }, StringSplitOptions.RemoveEmptyEntries);
		}

		public static string StripHtml(string input)
		{
			return input.IsNullOrWhiteSpace()
				? input
				: Regex.Replace(input, @"<.*?>", string.Empty);
		}

		/// <summary>
		/// Remove all newlines from the string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string StripNewlines(string input)
		{
			return input.IsNullOrWhiteSpace()
				? input
                : Regex.Replace(input, @"(\r?\n)", String.Empty);
                
                //: Regex.Replace(input, Environment.NewLine, string.Empty);
		}

		/// <summary>
		/// Join elements of the array with a certain character between them
		/// </summary>
		/// <param name="input"></param>
		/// <param name="glue"></param>
		/// <returns></returns>
		public static string Join(IEnumerable input, string glue = " ")
		{
			if (input == null)
				return null;

			IEnumerable<object> castInput = input.Cast<object>();
			return string.Join(glue, castInput);
		}

		/// <summary>
		/// Sort elements of the array
		/// provide optional property with which to sort an array of hashes or drops
		/// </summary>
		/// <param name="input"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static IEnumerable Sort(object input, string property = null)
		{
			List<object> ary;
			if (input is IEnumerable)
				ary = ((IEnumerable) input).Flatten().Cast<object>().ToList();
			else
				ary = new List<object>(new[] { input });
			if (!ary.Any())
				return ary;

			if (string.IsNullOrEmpty(property))
				ary.Sort();
			else if ((ary.All(o => o is IDictionary)) && ((IDictionary) ary.First()).Contains(property))
				ary.Sort((a, b) => Comparer.Default.Compare(((IDictionary) a)[property], ((IDictionary) b)[property]));
			else if (ary.All(o => o.RespondTo(property)))
				ary.Sort((a, b) => Comparer.Default.Compare(a.Send(property), b.Send(property)));

			return ary;
		}

		/// <summary>
		/// Map/collect on a given property
		/// </summary>
		/// <param name="input"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static IEnumerable Map(IEnumerable input, string property)
		{
			List<object> ary = input.Cast<object>().ToList();
			if (!ary.Any())
				return ary;

			if ((ary.All(o => o is IDictionary)) && ((IDictionary) ary.First()).Contains(property))
				return ary.Select(e => ((IDictionary) e)[property]);
			if (ary.All(o => o.RespondTo(property)))
				return ary.Select(e => e.Send(property));

			return ary;
		}

		/// <summary>
		/// Replace occurrences of a string with another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string Replace(string input, string @string, string replacement = "")
		{
			if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
				return input;

			return string.IsNullOrEmpty(input)
				? input
				: Regex.Replace(input, @string, replacement);
		}

		/// <summary>
		/// Replace the first occurence of a string with another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceFirst(string input, string @string, string replacement = "")
		{
			if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(@string))
				return input;

			bool doneReplacement = false;
			return Regex.Replace(input, @string, m =>
			{
				if (doneReplacement)
					return m.Value;

				doneReplacement = true;
				return replacement;
			});
		}

		/// <summary>
		/// Remove a substring
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Remove(string input, string @string)
		{
			return input.IsNullOrWhiteSpace()
				? input
				: input.Replace(@string, string.Empty);
		}

		/// <summary>
		/// Remove the first occurrence of a substring
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string RemoveFirst(string input, string @string)
		{
			return input.IsNullOrWhiteSpace()
				? input
				: ReplaceFirst(input, @string, string.Empty);
		}

		/// <summary>
		/// Add one string to another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Append(string input, string @string)
		{
			return input == null
				? input
				: input + @string;
		}

		/// <summary>
		/// Prepend a string to another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Prepend(string input, string @string)
		{
			return input == null
				? input
				: @string + input;
		}

		/// <summary>
		/// Add <br /> tags in front of all newlines in input string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string NewlineToBr(string input)
		{
            return input.IsNullOrWhiteSpace()
                    ? input
                    : Regex.Replace(input, @"(\r?\n)", "<br />$1");
		}

		/// <summary>
		/// Formats a date using a .NET date format string
		/// </summary>
		/// <param name="input"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static string Date(object input, string format)
		{
			if (input == null)
				return null;

            if ( input.ToString() == "Now" )
            {
                input = RockDateTime.Now.ToString();
            }

			if (format.IsNullOrWhiteSpace())
				return input.ToString();

			DateTime date;

			return DateTime.TryParse(input.ToString(), out date)
				? Liquid.UseRubyDateFormat ? date.ToStrFTime(format) : date.ToString(format)
				: input.ToString();
		}

		/// <summary>
		/// Get the first element of the passed in array 
		/// 
		/// Example:
		///   {{ product.images | first | to_img }}
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static object First(IEnumerable array)
		{
			if (array == null)
				return null;

			return array.Cast<object>().FirstOrDefault();
		}

		/// <summary>
		/// Get the last element of the passed in array 
		/// 
		/// Example:
		///   {{ product.images | last | to_img }}
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static object Last(IEnumerable array)
		{
			if (array == null)
				return null;

			return array.Cast<object>().LastOrDefault();
		}

		/// <summary>
		/// Addition
		/// </summary>
		/// <param name="input"></param>
		/// <param name="operand"></param>
		/// <returns></returns>
		public static object Plus(object input, object operand)
		{
			return input is string
				? string.Concat(input, operand)
				: DoMathsOperation(input, operand, Expression.Add);
		}

		/// <summary>
		/// Subtraction
		/// </summary>
		/// <param name="input"></param>
		/// <param name="operand"></param>
		/// <returns></returns>
		public static object Minus(object input, object operand)
		{
			return DoMathsOperation(input, operand, Expression.Subtract);
		}

		/// <summary>
		/// Multiplication
		/// </summary>
		/// <param name="input"></param>
		/// <param name="operand"></param>
		/// <returns></returns>
		public static object Times(object input, object operand)
		{
			return input is string && operand is int
				? Enumerable.Repeat((string) input, (int) operand)
				: DoMathsOperation(input, operand, Expression.Multiply);
		}

		/// <summary>
		/// Division
		/// </summary>
		/// <param name="input"></param>
		/// <param name="operand"></param>
		/// <returns></returns>
		public static object DividedBy(object input, object operand)
		{
			return DoMathsOperation(input, operand, Expression.Divide);
		}

		public static object Modulo(object input, object operand)
		{
			return DoMathsOperation(input, operand, Expression.Modulo);
		}

		private static object DoMathsOperation(object input, object operand, Func<Expression, Expression, BinaryExpression> operation)
		{
			return input == null || operand == null
				? null
				: ExpressionUtility.CreateExpression(operation, input.GetType(), operand.GetType(), input.GetType(), true)
					.DynamicInvoke(input, operand);
		}
	}

	internal static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string s)
		{
			return string.IsNullOrEmpty(s) || s.Trim().Length == 0;
		}
	}
}