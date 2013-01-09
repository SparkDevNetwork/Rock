//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;

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
        public static string ToJson( this object obj )
        {
            return JsonConvert.SerializeObject( obj );
        }

        ///// <summary>
        ///// Converts object to JSON string
        ///// </summary>
        ///// <param name="obj">Object.</param>
        ///// <param name="recursionDepth">constrains the number of object levels to process.</param>
        ///// <returns></returns>
        //public static string ToJSON( this object obj, int recursionDepth )
        //{
        //    return JsonConvert.SerializeObject( obj, new JsonSerializerSettings { MaxDepth = recursionDepth } );
        //}

        ///// <summary>
        ///// Creates a copy of the object's property as a DynamicObject.
        ///// </summary>
        ///// <param name="obj">The object to copy.</param>
        ///// <returns></returns>
        //public static ExpandoObject ToDynamic( this object obj )
        //{
        //    dynamic expando = new ExpandoObject();
        //    var dict = expando as IDictionary<string, object>;
        //    var properties = obj.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );

        //    foreach ( var prop in properties )
        //    {
        //        dict[prop.Name] = prop.GetValue( obj, null );
        //    }

        //    return expando;
        //}

        ///// <summary>
        ///// Creates an instance of a model and populates it based on the dynamic object's properties.
        ///// </summary>
        ///// <typeparam name="T">The destination model type</typeparam>
        ///// <param name="obj">The dynamic object to convert</param>
        ///// <returns>A populated instance of the model defined in the type arg</returns>
        //public static T ToModel<T>( this object obj )
        //{
        //    var o = obj as ExpandoObject;
        //    var instance = Activator.CreateInstance<T>();

        //    if ( o == null )
        //    {
        //        return instance;
        //    }

        //    var dict = o as IDictionary<string, object>;
        //    var properties = instance.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance )
        //        .Where( prop => dict.ContainsKey( prop.Name ) );

        //    foreach ( var prop in properties )
        //    {
        //        try { prop.SetValue( instance, dict[ prop.Name ] ); }
        //        catch ( Exception ) { }
        //    }

        //    return instance;
        //}

        /// <summary>
        /// Gets the name of the friendly type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetFriendlyTypeName( this Type type )
        {
            Rock.Data.FriendlyTypeNameAttribute attrib = type.GetTypeInfo().GetCustomAttribute<Rock.Data.FriendlyTypeNameAttribute>();
            if ( attrib != null )
            {
                return attrib.FriendlyTypeName;
            }
            else
            {
                if ( typeof(IEntity).IsAssignableFrom(type) )
                {
                    var entityType = Rock.Web.Cache.EntityTypeCache.Read( type.FullName );
                    return entityType.FriendlyName ?? SplitCase( type.Name );
                }
                else
                {
                    return SplitCase( type.Name );
                }
            }
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
            if ( str == null )
                return null;

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited by any combination of whitespace, comma, semi-colon, or pipe characters
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="whitespace">if set to <c>true</c> [whitespace].</param>
        /// <returns></returns>
        public static string[] SplitDelimitedValues( this string str, bool whitespace = true )
        {
            if ( str == null )
                return new string[0];

            string regex = whitespace ? @"[\s\|,;]+" : @"[\|,;]+";

            char[] delimiter = new char[] { ',' };
            return Regex.Replace( str, regex, "," ).Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
        }

        /// <summary>
        /// Replaces every instance of oldValue (regardless of case) with the newValue.
        /// from http://www.codeproject.com/Articles/10890/Fastest-C-Case-Insenstive-String-Replace
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The value to insert.</param>
        /// <returns></returns>
        public static string ReplaceCaseInsensitive( this string str, string oldValue, string newValue )
        {
            if ( str == null )
                return null;

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

        /// <summary>
        /// Adds escape character for quotes in a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeQuotes( this string str )
        {
            if ( str == null )
                return null;

            return str.Replace( "'", "\\'" ).Replace( "\"", "\\" );
        }

        /// <summary>
        /// Truncates a string after a max length and adds ellipsis.  Truncation will occur at first space prior to maxLength
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Ellipsis( this string str, int maxLength )
        {
            if ( str == null )
                return null;

            if ( str.Length <= maxLength )
                return str;

            maxLength -= 3;
            var truncatedString = str.Substring( 0, maxLength );
            var lastSpace = truncatedString.LastIndexOf( ' ' );
            if ( lastSpace > 0 )
                truncatedString = truncatedString.Substring( 0, lastSpace );

            return truncatedString + "...";
        }

        /// <summary>
        /// Pluralizes the specified tring.
        /// </summary>
        /// <param name="str">The string to pluralize.</param>
        /// <returns></returns>
        public static string Pluralize( this string str )
        {
            var pluralizationService = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService( new System.Globalization.CultureInfo( "en-US" ) );
            return pluralizationService.Pluralize( str );
        }

        /// <summary>
        /// Removes any non-numeric characters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AsNumeric( this string str )
        {
            return Regex.Replace( str, @"[^0-9]", "" );
        }

        #endregion

        #region Int Extensions

        /// <summary>
        /// Gets the Defined Value name associated with this id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string DefinedValue( this int? id )
        {
            if ( !id.HasValue )
                return string.Empty;

            var definedValue = Rock.Web.Cache.DefinedValueCache.Read( id.Value );
            if ( definedValue != null )
                return definedValue.Name;
            else
                return string.Empty;
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

        /// <summary>
        /// To the yes no.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToYesNo( this bool value )
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// To the true false.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToTrueFalse( this bool value )
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// Froms the true false.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool FromTrueFalse( this string value )
        {
            return value.Equals( "True" );
        }

        #endregion

        #region DateTime Extensions

        /// <summary>
        /// Returns the age at the current date
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int Age( this DateTime? start )
        {
            if ( start.HasValue )
                return start.Value.Age();
            else
                return 0;
        }

        /// <summary>
        /// Returns the age at the current date
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int Age( this DateTime start )
        {
            var now = DateTime.Today;
            int age = now.Year - start.Year;
            if ( start > now.AddYears( -age ) ) age--;

            return age;
        }

        /// <summary>
        /// The total months.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static int TotalMonths( this DateTime end, DateTime start )
        {
            return ( end.Year * 12 + end.Month ) - ( start.Year * 12 + start.Month );
        }

        /// <summary>
        /// The total years.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static int TotalYears( this DateTime end, DateTime start )
        {
            return ( end.Year ) - ( start.Year );
        }

        /// <summary>
        /// Returns a friendly elapsed time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <returns></returns>
        public static string ToElapsedString( this DateTime? dateTime, bool condensed = false, bool includeTime = true )
        {
            if ( dateTime.HasValue )
            {
                return ToElapsedString( dateTime.Value, condensed, includeTime );
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns a friendly elapsed time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <returns></returns>
        public static string ToElapsedString( this DateTime dateTime, bool condensed = false, bool includeTime = true )
        {
            DateTime start = dateTime;
            DateTime end = DateTime.Now;

            string direction = " Ago";
            TimeSpan timeSpan = end.Subtract( start );
            if ( timeSpan.TotalMilliseconds < 0 )
            {
                direction = " From Now";
                start = end;
                end = dateTime;
                timeSpan = timeSpan.Negate();
            }

            string duration = "";

            if ( timeSpan.TotalHours < 24 && includeTime )
            {
                // Less than one second
                if ( timeSpan.TotalSeconds <= 1 )
                    duration = string.Format( "1{0}", condensed ? "sec" : " Second" );

                else if ( timeSpan.TotalSeconds < 60 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalSeconds ), condensed ? "sec" : " Seconds" );
                else if ( timeSpan.TotalMinutes <= 1 )
                    duration = string.Format( "1{0}", condensed ? "min" : " Minute" );
                else if ( timeSpan.TotalMinutes < 60 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalMinutes ), condensed ? "min" : " Minutes" );
                else if ( timeSpan.TotalHours <= 1 )
                    duration = string.Format( "1{0}", condensed ? "hr" : " Hour" );
                else if ( timeSpan.TotalHours < 24 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalHours ), condensed ? "hr" : " Hours" );
            }

            if ( duration == "" )
            {
                if ( timeSpan.TotalDays <= 1 )
                    duration = string.Format( "1{0}", condensed ? "day" : " Day" );
                else if ( timeSpan.TotalDays < 31 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalDays ), condensed ? "days" : " Days" );
                else if ( end.TotalMonths( start ) <= 1 )
                    duration = string.Format( "1{0}", condensed ? "mon" : " Month" );
                else if ( end.TotalMonths( start ) <= 18 )
                    duration = string.Format( "{0:N0}{1}", end.TotalMonths( start ), condensed ? "mon" : " Months" );
                else if ( end.TotalYears( start ) <= 1 )
                    duration = string.Format( "1{0}", condensed ? "yr" : " Year" );
                else
                    duration = string.Format( "{0:N0}{1}", end.TotalYears( start ), condensed ? "yrs" : " Years" );
            }

            return "(" + duration + ( condensed ? "" : direction ) + ")";

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

        /// <summary>
        /// Gets the parent RockBlock.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static Rock.Web.UI.RockBlock RockBlock( this System.Web.UI.Control control )
        {
            System.Web.UI.Control parentControl = control.Parent;
            while ( parentControl != null )
            {
                if ( parentControl is Rock.Web.UI.RockBlock )
                {
                    return (Rock.Web.UI.RockBlock)parentControl;
                }
                parentControl = parentControl.Parent;
            }
            return null;
        }

        /// <summary>
        /// Rocks the page.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static Rock.Web.UI.RockPage RockPage( this System.Web.UI.Control control )
        {
            System.Web.UI.Control parentControl = control.Parent;
            while ( parentControl != null )
            {
                if ( parentControl is Rock.Web.UI.RockPage )
                {
                    return (Rock.Web.UI.RockPage)parentControl;
                }
                parentControl = parentControl.Parent;
            }
            return null;
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

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                webControl.CssClass = Regex.Replace( css + " " + className, @"^\s+", "", RegexOptions.IgnoreCase );
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

        #region DropDownList/ListControl Extensions

        /// <summary>
        /// Try's to set the selected value, if the value does not exist, will set the first item in the list
        /// </summary>
        /// <param name="ddl">The DDL.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this DropDownList ddl, string value )
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

        /// <summary>
        /// Try's to set the selected value, if the value does not exist, will set the first item in the list
        /// </summary>
        /// <param name="ddl">The DDL.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this DropDownList ddl, int? value )
        {
            ddl.SetValue( value == null ? "0" : value.ToString() );
        }

        /// <summary>
        /// Binds to enum.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="enumType">Type of the enum.</param>
        public static void BindToEnum( this ListControl listControl, Type enumType )
        {
            var dictionary = new Dictionary<int, string>();
            foreach ( var value in Enum.GetValues( enumType ) )
            {
                dictionary.Add(Convert.ToInt32(value), Enum.GetName( enumType, value ).SplitCase() );
            }

            listControl.DataSource = dictionary;
            listControl.DataTextField = "Value";
            listControl.DataValueField = "Key";
            listControl.DataBind();
        }

        /// <summary>
        /// Binds to the values of a definedType
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="definedType">Type of the defined.</param>
        public static void BindToDefinedType( this ListControl listControl, Rock.Web.Cache.DefinedTypeCache definedType )
        {
            var ds = definedType.DefinedValues
                .Select( v => new
                {
                    v.Name,
                    v.Id
                } );

            listControl.DataSource = ds;
            listControl.DataTextField = "Name";
            listControl.DataValueField = "Id";
            listControl.DataBind();
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
        /// Converts to int.
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <returns></returns>
        public static int ConvertToInt( this Enum eff )
        {
            return Convert.ToInt32( eff );
        }

        /// <summary>
        /// Converts a string value to an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static T ConvertToEnum<T>( this String enumValue )
        {
            return (T)Enum.Parse( typeof( T ), enumValue.Replace( " ", "" ) );
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
        public static string AsDelimited<T>( this List<T> items, string delimiter )
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
        /// <param name="delimter">The delimter.</param>
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
            return (IOrderedQueryable<T>)result;
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

        #region IHasAttributes extensions

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity )
        {
            Rock.Attribute.Helper.LoadAttributes( entity );
        }

        #endregion

        #region Route Extensions

        /// <summary>
        /// Pages the id.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int PageId( this Route route )
        {
            if ( route.DataTokens != null )
            {
                return int.Parse( route.DataTokens["PageId"] as string );
            }

            return -1;
        }

        /// <summary>
        /// Routes the id.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int RouteId( this Route route )
        {
            if ( route.DataTokens != null )
            {
                return int.Parse( route.DataTokens["RouteId"] as string );
            }

            return -1;
        }

        /// <summary>
        /// Adds the page route.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="pageRoute">The page route.</param>
        public static void AddPageRoute( this Collection<RouteBase> routes, PageRoute pageRoute )
        {
            Route route = new Route( pageRoute.Route, new Rock.Web.RockRouteHandler() );
            route.DataTokens = new RouteValueDictionary();
            route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
            route.DataTokens.Add( "RouteId", pageRoute.Id.ToString() );
            routes.Add( route );
        }

        #endregion

        #region IEntity extensions
        
        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="id">The id.</param>
        public static void RemoveEntity<T>( this List<T> list, int id ) where T : Rock.Data.IEntity
        {
            var item = list.FirstOrDefault( a => a.Id.Equals( id ) );
            if ( item != null )
            {
                list.Remove( item );
            }
        }

        /// <summary>
        /// Removes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="guid">The GUID.</param>
        public static void RemoveEntity<T>( this List<T> list, Guid guid ) where T : Rock.Data.IEntity
        {
            var item = list.FirstOrDefault( a => a.Guid.Equals( guid ) );
            if ( item != null )
            {
                list.Remove( item );
            }

        }
        
        #endregion
    }
}