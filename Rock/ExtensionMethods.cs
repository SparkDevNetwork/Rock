//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Rock
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        #region Object Extensions

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJSON( this object obj )
        {
            return JsonConvert.SerializeObject( obj );
        }

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="recursionDepth">constrains the number of object levels to process.</param>
        /// <returns></returns>
        public static string ToJSON( this object obj, int recursionDepth )
        {
            return JsonConvert.SerializeObject( obj, new JsonSerializerSettings { MaxDepth = recursionDepth } );
        }

        /// <summary>
        /// Creates a copy of the object's property as a DynamicObject.
        /// </summary>
        /// <param name="obj">The object to copy.</param>
        /// <returns></returns>
        public static ExpandoObject ToDynamic( this object obj )
        {
            dynamic expando = new ExpandoObject();
            var dict = expando as IDictionary<string, object>;
            var properties = obj.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach (var prop in properties)
            {
                dict[prop.Name] = prop.GetValue( obj, null );
            }

            return expando;
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// Splits a Camel or Pascal cased identifier into seperate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited by any combination of whitespace, comma, semi-colon, or pipe characters
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string[] SplitDelimitedValues( this string str )
        {
            char[] delimiter = new char[] {','};
            return Regex.Replace( str, @"[\s\|,;]+", "," ).Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
        }

        /// <summary>
        /// Replaces every instance of oldValue (regardless of case) with the newValue.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The value to insert.</param>
        /// <returns></returns>
        public static string ReplaceCaseInsensitive( this string str, string oldValue, string newValue )
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = str.ToUpper();
            string upperPattern = oldValue.ToUpper();
            int inc = ( str.Length / oldValue.Length ) *
                      ( newValue.Length - oldValue.Length );
            char[] chars = new char[str.Length + Math.Max( 0, inc )];
            while ( ( position1 = upperString.IndexOf( upperPattern,
                                              position0 ) ) != -1 )
            {
                for ( int i = position0; i < position1; ++i )
                    chars[count++] = str[i];
                for ( int i = 0; i < newValue.Length; ++i )
                    chars[count++] = newValue[i];
                position0 = position1 + oldValue.Length;
            }
            if ( position0 == 0 ) return str;
            for ( int i = position0; i < str.Length; ++i )
                chars[count++] = str[i];
            return new string( chars, 0, count );
        }

        public static string EscapeQuotes( this string str )
        {
            return str.Replace( "'", "\\'" ).Replace( "\"", "\\" );
        }

        public static string Ellipsis( this string str, int maxLength )
        {
            if (str.Length <= maxLength)
                return str;

            maxLength -= 3;
            var truncatedString = str.Substring(0, maxLength);
            var lastSpace = truncatedString.LastIndexOf( ' ' );
            if ( lastSpace > 0 )
                truncatedString = truncatedString.Substring( 0, lastSpace );

            return truncatedString + "...";
        }

        #endregion

		#region Boolean Extensions

		/// <summary>
		/// A numeric 1 or 0
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public static int Bit( this Boolean field )
		{
			return field ? 1 : 0;
		}

		#endregion

		#region DateTime Extensions

		/// <summary>
        /// The total months.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static int TotalMonths( this DateTime end, DateTime start )
        {
            return ( start.Year * 12 + start.Month ) - ( end.Year * 12 + end.Month );
        }

        /// <summary>
        /// The total years.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static int TotalYears( this DateTime end, DateTime start )
        {
            return ( start.Year) - ( end.Year);
        }

        /// <summary>
        /// Returns a friendly elapsed time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static string ToElapsedString( this DateTime? dateTime )
        {
            if ( dateTime.HasValue )
            {
                string direction = "Ago";
                TimeSpan timeSpan = DateTime.Now.Subtract( dateTime.Value );
                if ( timeSpan.TotalMilliseconds < 0 )
                {
                    direction = "From Now";
                    timeSpan = timeSpan.Negate();
                }

                string duration = "";

                // Less than one second
                if ( timeSpan.TotalSeconds <= 1 )
                    duration = "1 Second";

                else if ( timeSpan.TotalSeconds < 60 )
                    duration = string.Format( "{0:N0} Seconds", Math.Truncate( timeSpan.TotalSeconds ) );
                else if ( timeSpan.TotalMinutes <= 1 )
                    duration = "1 Minute";
                else if ( timeSpan.TotalMinutes < 60 )
                    duration = string.Format( "{0:N0} Minutes", Math.Truncate( timeSpan.TotalMinutes ) );
                else if ( timeSpan.TotalHours <= 1 )
                    duration = "1 Hour";
                else if ( timeSpan.TotalHours < 24 )
                    duration = string.Format( "{0:N0} Hours", Math.Truncate( timeSpan.TotalHours ) );
                else if ( timeSpan.TotalDays <= 1 )
                    duration = "1 Day";
                else if ( timeSpan.TotalDays < 31 )
                    duration = string.Format( "{0:N0} Days", Math.Truncate( timeSpan.TotalDays ) );
                else if ( DateTime.Now.TotalMonths( dateTime.Value ) <= 1 )
                    duration = "1 Month";
                else if ( DateTime.Now.TotalMonths( dateTime.Value ) <= 18 )
                    duration = string.Format( "{0:N0} Months", DateTime.Now.TotalMonths( dateTime.Value ) );
                else if ( DateTime.Now.TotalYears( dateTime.Value ) <= 1 )
                    duration = "1 Year";
                else
                    duration = string.Format( "{0:N0} Years", DateTime.Now.TotalYears( dateTime.Value ) );

                return duration + " " + direction;
            }
            else
                return string.Empty;

        }

        #endregion

        #region Control Extensions

        /// <summary>
        /// Loads a user control using the constructor with the parameters specified.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="userControlPath">The user control path.</param>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <returns></returns>
        public static System.Web.UI.UserControl LoadControl( this System.Web.UI.Control control, string userControlPath, params object[] constructorParameters )
        {
            List<Type> constParamTypes = new List<Type>();
            foreach ( object constParam in constructorParameters )
                constParamTypes.Add( constParam.GetType() );

            System.Web.UI.UserControl ctl = control.Page.LoadControl( userControlPath ) as System.Web.UI.UserControl;

            ConstructorInfo constructor = ctl.GetType().BaseType.GetConstructor( constParamTypes.ToArray() );

            if ( constructor == null )
                throw new MemberAccessException( "The requested constructor was not found on " + ctl.GetType().BaseType.ToString() );
            else
                constructor.Invoke( ctl, constructorParameters );

            return ctl;
        }

        #endregion

        #region WebControl Extensions

        /// <summary>
        /// Adds a CSS class name to a web control
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static void AddCssClass( this System.Web.UI.WebControls.WebControl webControl, string className )
        {
            string match = @"\b" + className + "\b";
            string css = webControl.CssClass;

            if (!Regex.IsMatch(css, match, RegexOptions.IgnoreCase))
                webControl.CssClass = Regex.Replace( css + " " + className, @"^\s+", "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Removes a CSS class name from a web control.
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static void RemoveCssClass( this System.Web.UI.WebControls.WebControl webControl, string className )
        {
            string match = @"\s*\b" + className + "\b";
            string css = webControl.CssClass;

            if ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                webControl.CssClass = Regex.Replace( css, match, "", RegexOptions.IgnoreCase );
        }

        #endregion

        #region HtmlControl Extensions

        /// <summary>
        /// Adds a CSS class name to an html control
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void AddCssClass( this System.Web.UI.HtmlControls.HtmlControl htmlControl, string className )
        {
            string match = @"\b" + className + "\b";
            string css = htmlControl.Attributes["class"] ?? string.Empty;

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                htmlControl.Attributes["class"] = Regex.Replace( css + " " + className, @"^\s+", "", RegexOptions.IgnoreCase );
        }

        /// <summary>
        /// Removes a CSS class name from an html control.
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void RemoveCssClass( this System.Web.UI.HtmlControls.HtmlControl htmlControl, string className )
        {
            string match = @"\s*\b" + className + @"\b";
            string css = htmlControl.Attributes["class"] ?? string.Empty;

            if ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                htmlControl.Attributes["class"] = Regex.Replace( css, match, "", RegexOptions.IgnoreCase );
        }

        #endregion

        #region DropDownList Extensions

        /// <summary>
        /// Try's to set the selected value, if the value does not exist, wills et the first item in the list
        /// </summary>
        /// <param name="ddl">The DDL.</param>
        /// <param name="value">The value.</param>
        public static void SetValue (this System.Web.UI.WebControls.DropDownList ddl, string value)
        {
            try
            {
                ddl.SelectedValue = value;
            }
            catch
            {
                if ( ddl.Items.Count > 0 )
                    ddl.SelectedIndex = 0;
            }
                
        }

        #endregion

        #region Enum Extensions

        /// <summary>
        /// Converts to the enum value to it's string value
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <returns></returns>
        public static String ConvertToString( this Enum eff )
        {
            return Enum.GetName( eff.GetType(), eff ).SplitCase();
        }

        /// <summary>
        /// Converts a string value to an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static T ConvertToEnum<T>( this String enumValue )
        {
            return ( T )Enum.Parse( typeof( T ), enumValue.Replace(" " , "") );
        }

        #endregion

        #region GenericCollection Extensions

        /// <summary>
        /// Concatonate the items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        public static string AsDelimited<T>( this List<T> items, string delimiter)
        {
            List<string> strings = new List<string>();
            foreach ( T item in items )
                strings.Add( item.ToString() );
            return String.Join( delimiter, strings.ToArray() );
        }

        /// <summary>
        /// Joins a dictionary of items
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static string Join( this Dictionary<string, string> items, string delimter )
        {
            List<string> parms = new List<string>();
            foreach ( var item in items )
                parms.Add( string.Join( "=", new string[] { item.Key, item.Value } ) );
            return string.Join( delimter, parms.ToArray() );
        }

        #endregion

        #region IQueryable extensions

        /// <summary>
        /// Orders the list by the name of a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>( this IQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "OrderBy" );
        }

        /// <summary>
        /// Orders the list by the name of a property in descending order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>( this IQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "OrderByDescending" );
        }

        /// <summary>
        /// Then Orders the list by the name of a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>( this IOrderedQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "ThenBy" );
        }

        /// <summary>
        /// Then Orders the list by a a property in descending order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>( this IOrderedQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "ThenByDescending" );
        }

        static IOrderedQueryable<T> ApplyOrder<T>( IQueryable<T> source, string property, string methodName )
        {
            string[] props = property.Split( '.' );
            Type type = typeof( T );
            ParameterExpression arg = Expression.Parameter( type, "x" );
            Expression expr = arg;
            foreach ( string prop in props )
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty( prop );
                expr = Expression.Property( expr, pi );
                type = pi.PropertyType;
            }
            Type delegateType = typeof( Func<,> ).MakeGenericType( typeof( T ), type );
            LambdaExpression lambda = Expression.Lambda( delegateType, expr, arg );

            object result = typeof( Queryable ).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2 )
                    .MakeGenericMethod( typeof( T ), type )
                    .Invoke( null, new object[] { source, lambda } );
            return ( IOrderedQueryable<T> )result;
        }

        /// <summary>
        /// Sorts the object by the specified sort property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> Sort<T>( this IQueryable<T> source, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            if ( sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending )
                return source.OrderBy( sortProperty.Property );
            else
                return source.OrderByDescending( sortProperty.Property );
        }


        #endregion
    }
}