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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Routing;
using System.Web.UI.WebControls;
using DotLiquid;
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
        #region Json Extensions

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJson( this object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// Attempts to deserialize a json string into T.  If it can't be deserialized, returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrNull<T>( this string val )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( val ) )
                {
                    return default( T );
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>( val );
                }
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

        #region Object Extensions

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="rootObj">The root obj.</param>
        /// <param name="propertyPathName">Name of the property path.</param>
        /// <returns></returns>
        public static object GetPropertyValue( this object rootObj, string propertyPathName )
        {
            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object obj = rootObj;
            Type objType = rootObj.GetType();

            while ( propPath.Any() && obj != null )
            {
                PropertyInfo property = objType.GetProperty( propPath.First() );
                if ( property != null )
                {
                    obj = property.GetValue( obj );
                    objType = property.PropertyType;
                    propPath = propPath.Skip( 1 ).ToList();
                }
                else
                {
                    obj = null;
                }
            }

            return obj;
        }

        /// <summary>
        /// Safely ToString() this item, even if it's null.
        /// </summary>
        /// <param name="obj">an object</param>
        /// <returns>The ToString or the empty string if the item is null.</returns>
        public static string ToStringSafe( this object obj )
        {
            if ( obj != null )
            {
                return obj.ToString();
            }
            return String.Empty;
        }

        /// <summary>
        /// Returns an html representation of object that is available to lava.
        /// </summary>
        /// <param name="lavaObject">The liquid object.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="preText">The pre text.</param>
        /// <param name="postText">The post text.</param>
        /// <returns></returns>
        public static string lavaDebugInfo( this object lavaObject, RockContext rockContext = null, string preText = "", string postText = "" )
        {
            //return liquidObject.LiquidizeChildren( 0, rockContext ).ToJson();
            StringBuilder lavaDebugPanel = new StringBuilder();
            lavaDebugPanel.Append( "<div class='alert alert-info lava-debug'><h4>Lava Debug Info</h4>" );

            lavaDebugPanel.Append( preText );

            lavaDebugPanel.Append( "<p>Below is a listing of available merge fields for this block. Find out more on Lava at <a href='http://www.rockrms.com/lava' target='_blank'>rockrms.com/lava</a>." );

            lavaDebugPanel.Append( formatLavaDebugInfo( lavaObject.LiquidizeChildren( 0, rockContext ) ) );

            // Add a 'GlobalAttribute' entry if it wasn't part of the LavaObject
            if ( !( lavaObject is Dictionary<string, object> ) || !( (Dictionary<string, object>)lavaObject ).Keys.Contains( "GlobalAttribute" ) )
            {
                var globalAttributes = new Dictionary<string, object>();
                globalAttributes.Add( "GlobalAttribute", Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null ) );
                lavaDebugPanel.Append( formatLavaDebugInfo( globalAttributes.LiquidizeChildren( 0, rockContext ) ) );
            }

            lavaDebugPanel.Append( postText );

            lavaDebugPanel.Append( "</div>" );

            return lavaDebugPanel.ToString();
        }

        /// <summary>
        /// Liquidizes the child properties of an object for displaying debug information about fields available for lava templates
        /// </summary>
        /// <param name="myObject">an object.</param>
        /// <param name="levelsDeep">The levels deep.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <returns></returns>
        private static object LiquidizeChildren( this object myObject, int levelsDeep = 0, RockContext rockContext = null, string parentElement = "" )
        {
            // Add protection for stack-overflow if property attributes are not set correctly resulting in child/parent objects being evaluated in loop
            levelsDeep++;
            if ( levelsDeep > 10 )
            {
                return string.Empty;
            }

            // If the object is liquidable, get the object return by it's ToLiquid() method.
            if ( myObject is DotLiquid.ILiquidizable )
            {
                myObject = ( (DotLiquid.ILiquidizable)myObject ).ToLiquid();
            }

            // If the object is null, return an empty string
            if ( myObject == null )
            {
                return string.Empty;
            }

            // If the object is a string, return it's value converted to HTML and truncated
            if ( myObject is string )
            {
                return myObject.ToString().Truncate( 50 ).EncodeHtml();
            }

            // If the object is a guid, return it's string representation
            if ( myObject is Guid )
            {
                return myObject.ToString();
            }

            // Get the object's type ( checking for a proxy object )
            Type entityType = myObject.GetType();
            if ( entityType.Namespace == "System.Data.Entity.DynamicProxies" )
                entityType = entityType.BaseType;

            // If the object is a Liquid Drop object, return a list of all of the object's properties
            if ( myObject is Drop )
            {
                var result = new Dictionary<string, object>();

                foreach ( var propInfo in entityType.GetProperties(
                          BindingFlags.Public |
                          BindingFlags.Instance |
                          BindingFlags.DeclaredOnly ) )
                {
                    if ( propInfo != null )
                    {
                        try
                        {
                            result.Add( propInfo.Name, propInfo.GetValue( myObject, null ).LiquidizeChildren( levelsDeep, rockContext ) );
                        }
                        catch ( Exception ex )
                        {
                            result.Add( propInfo.Name, ex.ToString() );
                        }
                    }
                }

                return result;
            }

            // If the object has the [LiquidType] attribute, enumerate the allowed properties and return a list of those properties
            if ( entityType.GetCustomAttributes( typeof( LiquidTypeAttribute ), false ).Any() )
            {
                var result = new Dictionary<string, object>();

                var attr = (LiquidTypeAttribute)entityType.GetCustomAttributes( typeof( LiquidTypeAttribute ), false ).First();
                foreach ( string propName in attr.AllowedMembers )
                {
                    var propInfo = entityType.GetProperty( propName );
                    {
                        if ( propInfo != null )
                        {
                            try
                            {
                                result.Add( propInfo.Name, propInfo.GetValue( myObject, null ).LiquidizeChildren( levelsDeep, rockContext, parentElement + "." + propName ) );
                            }
                            catch ( Exception ex )
                            {
                                result.Add( propInfo.Name, ex.ToString() );
                            }
                        }
                    }
                }

                return result;
            }

            // If the object is a Rock Liquidizable object, call the object's AvailableKeys method to determine the properties available.
            if ( myObject is Lava.ILiquidizable )
            {
                var liquidObject = (Lava.ILiquidizable)myObject;

                var result = new Dictionary<string, object>();

                foreach ( var key in liquidObject.AvailableKeys )
                {
                    try
                    {
                        object propValue = liquidObject[key];
                        if ( propValue != null )
                        {
                            result.Add( key, propValue.LiquidizeChildren( levelsDeep, rockContext, parentElement + "." + key ) );
                        }
                        else
                        {
                            result.AddOrIgnore( key, string.Empty );
                        }
                    }
                    catch ( Exception ex )
                    {
                        result.AddOrIgnore( key, ex.ToString() );
                    }
                }

                // Add the attributes if this object has attributes
                if ( liquidObject is Rock.Attribute.IHasAttributes )
                {
                    var objWithAttrs = (Rock.Attribute.IHasAttributes)liquidObject;
                    if ( objWithAttrs.Attributes == null )
                    {
                        rockContext = rockContext ?? new RockContext();
                        objWithAttrs.LoadAttributes( rockContext );
                    }

                    var objAttrs = new Dictionary<string, object>();
                    foreach ( var objAttr in objWithAttrs.Attributes )
                    {
                        var attributeCache = objAttr.Value;
                        string value = attributeCache.FieldType.Field.FormatValue( null, objWithAttrs.GetAttributeValue( attributeCache.Key ), attributeCache.QualifierValues, false );
                        objAttrs.Add( attributeCache.Key, value.Truncate( 50 ).EncodeHtml() );
                    }

                    if ( objAttrs.Any() )
                    {
                        result.Add( string.Format( "Attributes <p class='attributes'>Below is a list of attributes that can be retrieved using <code>{{ {0} | Attribute:'[AttributeKey]' }}</code>.</p>", parentElement ), objAttrs );
                    }
                }

                return result;
            }

            if ( myObject is IDictionary<string, object> )
            {
                var result = new Dictionary<string, object>();

                foreach ( var keyValue in ( (IDictionary<string, object>)myObject ) )
                {
                    try
                    {
                        result.Add( keyValue.Key, keyValue.Value.LiquidizeChildren( levelsDeep, rockContext, keyValue.Key ) );
                    }
                    catch ( Exception ex )
                    {
                        result.Add( keyValue.Key, ex.ToString() );
                    }
                }

                return result;
            }

            if ( myObject is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( (IEnumerable)myObject ) )
                {
                    try
                    {
                        result.Add( value.LiquidizeChildren( levelsDeep, rockContext, parentElement ) );
                    }
                    catch { }
                }

                return result;
            }

            return myObject.ToStringSafe();
        }

        private static string formatLavaDebugInfo( object liquidizedObject, int levelsDeep = 0, string parents = "" )
        {
            if ( liquidizedObject is string )
            {
                return string.Format( "<span class='lava-debug-value'> - {0}</span>", liquidizedObject.ToString() );
            }

            if ( liquidizedObject is Dictionary<string, object> )
            {
                var sb = new StringBuilder();

                bool isTopLevel = levelsDeep == 0;

                if ( !isTopLevel )
                {
                    sb.AppendFormat( "{0}<ul>{0}", Environment.NewLine );
                }

                foreach ( var keyVal in (Dictionary<string, object>)liquidizedObject )
                {
                    if ( isTopLevel )
                    {
                        if ( keyVal.Value is string )
                        {
                            // item is a root level property
                            sb.Append( string.Format( "<ul><li><span class='lava-debug-key'>{0}</span> - {1}</li></ul>{2}", keyVal.Key, keyVal.Value.ToString(), Environment.NewLine ) );
                        }
                        else
                        {
                            // item is a root level object

                            string panelId = Guid.NewGuid().ToString();

                            sb.Append( "<div class='panel panel-default panel-lavadebug'>" );

                            sb.Append( string.Format( "<div class='panel-heading clearfix collapsed' data-toggle='collapse' data-target='#collapse-{0}'>", panelId ) );
                            sb.Append( string.Format( "<h5 class='panel-title pull-left'>{0}</h5> <div class='pull-right'><i class='fa fa-chevron-up'></i></div>", keyVal.Key.SplitCase() ) );
                            sb.Append( "</div>" );

                            sb.Append( string.Format( "<div id='collapse-{0}' class='panel-collapse collapse'>", panelId ) );
                            sb.Append( "<div class='panel-body'>" );

                            if ( keyVal.Key == "GlobalAttribute" )
                            {
                                sb.Append( "<p>Global attributes should be accessed using <code>{{ 'Global' | Attribute:'[AttributeKey]' }}</code>. Find out more about using Global Attributes in Lava at <a href='http://www.rockrms.com/lava/globalattributes' target='_blank'>rockrms.com/lava/globalattributes</a>.</p>" );
                            }
                            else if ( keyVal.Value is List<object> )
                            {
                                sb.Append( string.Format( "<p>{0} properties can be accessed by <code>{{% for {2} in {1} %}}{{{{ {2}.[PropertyKey] }}}}{{% endfor %}}</code>.</p>", char.ToUpper( keyVal.Key[0] ) + keyVal.Key.Substring( 1 ), keyVal.Key, keyVal.Key.Singularize().ToLower() ) );
                            }
                            else if ( keyVal.Key == "CurrentPerson" )
                            {
                                sb.Append( string.Format( "<p>{0} properties can be accessed by <code>{{{{ {1}.[PropertyKey] }}}}</code>. Find out more about using 'Person' fields in Lava at <a href='http://www.rockrms.com/lava/person' target='_blank'>rockrms.com/lava/person</a>.</p>", char.ToUpper( keyVal.Key[0] ) + keyVal.Key.Substring( 1 ), keyVal.Key ) );
                            }
                            else
                            {
                                sb.Append( string.Format( "<p>{0} properties can be accessed by <code>{{{{ {1}.[PropertyKey] }}}}</code>.</p>", char.ToUpper( keyVal.Key[0] ) + keyVal.Key.Substring( 1 ), keyVal.Key ) );
                            }

                            string value = formatLavaDebugInfo( keyVal.Value, 1, keyVal.Key );
                            sb.Append( value );

                            sb.Append( "</div>" );
                            sb.Append( "</div>" );
                            sb.Append( "</div>" );
                        }
                    }
                    else
                    {
                        string section = ( keyVal.Value is string ) ? "" : string.Format( " lava-debug-section level-{0}", levelsDeep );
                        string value = formatLavaDebugInfo( keyVal.Value, levelsDeep + 1, parents + "." + keyVal.Key );
                        sb.AppendFormat( "<li><span class='lava-debug-key{0}'>{1}</span> {2}</li>{3}", section, keyVal.Key, value, Environment.NewLine );
                    }
                }

                if ( !isTopLevel )
                {
                    sb.AppendLine( "</ul>" );
                }

                return sb.ToString();
            }

            if ( liquidizedObject is List<object> )
            {
                var sb = new StringBuilder();
                sb.AppendFormat( "{0}{{<ul>{0}", Environment.NewLine );

                int i = 0;

                foreach ( var obj in (List<object>)liquidizedObject )
                {
                    string value = formatLavaDebugInfo( obj, 1, parents );
                    sb.AppendFormat( "<li>[{0}] {1}</li>{2}", i.ToString(), value, Environment.NewLine );
                    i++;
                }
                sb.AppendLine( "</ul>}" );
                return sb.ToString();
            }

            return string.Empty;
        }

        #endregion Object Extensions

        #region Type Extensions

        /// <summary>
        /// Gets the name of the friendly type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetFriendlyTypeName( this Type type )
        {
            if ( type.Namespace == null )
            {
                // Anonymous types will not have a namespace
                return "Item";
            }

            if ( type.Namespace.Equals( "System.Data.Entity.DynamicProxies" ) )
            {
                type = type.BaseType;
            }

            if ( type.Namespace.Equals( "Rock.Model" ) )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Read( type, false );
                if ( entityType != null && entityType.FriendlyName != null )
                {
                    return entityType.FriendlyName;
                }
                else
                {
                    return SplitCase( type.Name );
                }
            }
            else
            {
                return SplitCase( type.Name );
            }
        }

        #endregion Type Extensions

        #region String Extensions

        /// <summary>
        /// Removed special characters from strings.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters( this string str )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( char c in str )
            {
                if ( ( c >= '0' && c <= '9' ) || ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) || c == '.' || c == '_' )
                {
                    sb.Append( c );
                }
            }
            return sb.ToString();
        }

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
        /// Returns a string array that contains the substrings in this string that are delimited by any combination of whitespace, comma, semi-colon, or pipe characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="whitespace">if set to <c>true</c> whitespace will be treated as a delimiter</param>
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
        /// Returns a List of ListItems that contains the values/text in this string that are formatted as either 'value1,value2,value3,...' or 'value1^text1,value2^text2,value3^text3,...'.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static List<ListItem> GetListItems( this string str )
        {
            var result = new List<ListItem>();
            foreach ( string keyvalue in str.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( keyValueArray.Length > 0 )
                {
                    var listItem = new ListItem();
                    listItem.Value = keyValueArray[0].Trim();
                    listItem.Text = keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim();
                    result.Add( listItem );
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces every instance of oldValue (regardless of case) with the newValue.
        /// (from http://www.codeproject.com/Articles/10890/Fastest-C-Case-Insenstive-String-Replace)
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
        /// Replaces every instance of oldValue with newValue.  Will continue to replace
        /// values after each replace until the oldValue does not exist.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The value to insert.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceWhileExists( this string str, string oldValue, string newValue )
        {
            string newstr = str;

            if ( oldValue != newValue )
            {
                while ( newstr.Contains( oldValue ) )
                {
                    newstr = newstr.Replace( oldValue, newValue );
                }
            }

            return newstr;
        }

        /// <summary>
        /// Adds escape character for quotes in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeQuotes( this string str )
        {
            if ( str == null )
                return null;

            return str.Replace( "'", "\\'" ).Replace( "\"", "\\\"" );
        }

        /// <summary>
        /// Adds Quotes around the specified string and escapes any quotes that are already in the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="QuoteChar">The quote character.</param>
        /// <returns></returns>
        public static string Quoted( this string str, string QuoteChar = "'" )
        {
            var result = QuoteChar + str.EscapeQuotes() + QuoteChar;
            return result;
        }

        /// <summary>
        /// Returns the specified number of characters, starting at the left side of the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The desired length.</param>
        /// <returns></returns>
        public static string Left( this string str, int length )
        {
            if ( str.Length <= length )
            {
                return str;
            }
            else
            {
                return str.Substring( 0, length );
            }
        }

        /// <summary>
        /// Truncates a string after a max length and adds ellipsis.  Truncation will occur at first space prior to maxLength.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate( this string str, int maxLength )
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
        /// Pluralizes the specified string.
        /// </summary>
        /// <param name="str">The string to pluralize.</param>
        /// <returns></returns>
        public static string Pluralize( this string str )
        {
            // Pluralization services handles most words, but there are some exceptions (i.e. campus)
            switch ( str )
            {
                case "Campus":
                case "campus":
                    return str + "es";

                case "CAMPUS":
                    return str + "ES";

                default:
                    var pluralizationService = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService( new System.Globalization.CultureInfo( "en-US" ) );
                    return pluralizationService.Pluralize( str );
            }
        }

        /// <summary>
        /// Singularizes the specified string.
        /// </summary>
        /// <param name="str">The string to singularize.</param>
        /// <returns></returns>
        public static string Singularize( this string str )
        {
            var pluralizationService = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService( new System.Globalization.CultureInfo( "en-US" ) );
            return pluralizationService.Singularize( str );
        }

        /// <summary>
        /// Removes any non-numeric characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AsNumeric( this string str )
        {
            return Regex.Replace( str, @"[^0-9]", "" );
        }

        /// <summary>
        /// Replaces the last occurrence of a given string with a new value
        /// </summary>
        /// <param name="Source">The string.</param>
        /// <param name="Find">The search parameter.</param>
        /// <param name="Replace">The replacement parameter.</param>
        /// <returns></returns>
        public static string ReplaceLastOccurrence( this string Source, string Find, string Replace )
        {
            int Place = Source.LastIndexOf( Find );
            string result = Source.Remove( Place, Find.Length ).Insert( Place, Replace );
            return result;
        }

        /// <summary>
        /// The true strings for AsBoolean and AsBooleanOrNull.
        /// </summary>
        private static string[] trueStrings = new string[] { "true", "yes", "t", "y", "1" };

        /// <summary>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive).
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="resultIfNullOrEmpty">if set to <c>true</c> [result if null or empty].</param>
        /// <returns></returns>
        public static bool AsBoolean( this string str, bool resultIfNullOrEmpty = false )
        {
            if ( string.IsNullOrWhiteSpace( str ) )
            {
                return resultIfNullOrEmpty;
            }

            return trueStrings.Contains( str.ToLower() );
        }

        /// <summary>
        /// Returns True for 'True', 'Yes', 'T', 'Y', '1' (case-insensitive), null for emptystring/null.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool? AsBooleanOrNull( this string str )
        {
            string[] trueStrings = new string[] { "true", "yes", "t", "y", "1" };

            if ( string.IsNullOrWhiteSpace( str ) )
            {
                return null;
            }

            return trueStrings.Contains( str.ToLower() );
        }

        /// <summary>
        /// Attempts to convert string to integer.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static int AsInteger( this string str )
        {
            return str.AsIntegerOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to an integer.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int? AsIntegerOrNull( this string str )
        {
            int value;
            if ( int.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns Guid.Empty if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static Guid AsGuid( this string str )
        {
            return str.AsGuidOrNull() ?? Guid.Empty;
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static Guid? AsGuidOrNull( this string str )
        {
            Guid value;
            if ( Guid.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified unique identifier is Guid.Empty.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static bool IsEmpty( this Guid guid )
        {
            return guid.Equals( Guid.Empty );
        }

        /// <summary>
        /// Attempts to convert string to decimal.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal AsDecimal( this string str )
        {
            return str.AsDecimalOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to decimal.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static decimal? AsDecimalOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters (for example, currency symbols)
                str = Regex.Replace( str, @"[^0-9\.-]", "" );
            }

            decimal value;
            if ( decimal.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to double.  Returns 0 if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static double AsDouble( this string str )
        {
            return str.AsDoubleOrNull() ?? 0;
        }

        /// <summary>
        /// Attempts to convert string to double.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static double? AsDoubleOrNull( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // strip off non numeric and characters (for example, currency symbols)
                str = Regex.Replace( str, @"[^0-9\.-]", "" );
            }

            double value;
            if ( double.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to DateTime.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static DateTime? AsDateTime( this string str )
        {
            DateTime value;
            if ( DateTime.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert string to TimeSpan.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static TimeSpan? AsTimeSpan( this string str )
        {
            TimeSpan value;
            if ( TimeSpan.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Use DotLiquid to resolve any merge codes within the content using the values
        /// in the mergeObjects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride )
        {
            try
            {
                if ( !content.HasMergeFields() )
                {
                    return content ?? string.Empty;
                }

                Template template = Template.Parse( content );
                template.InstanceAssigns.Add( "CurrentPerson", currentPersonOverride );
                return template.Render( Hash.FromDictionary( mergeObjects ) );
            }
            catch ( Exception ex )
            {
                return "Error resolving Lava merge fields: " + ex.Message;
            }
        }

        /// <summary>
        /// Use DotLiquid to resolve any merge codes within the content using the values
        /// in the mergeObjects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects )
        {
            try
            {
                if ( !content.HasMergeFields() )
                {
                    return content ?? string.Empty;
                }

                Template template = Template.Parse( content );
                return template.Render( Hash.FromDictionary( mergeObjects ) );
            }
            catch ( Exception ex )
            {
                return "Error resolving Lava merge fields: " + ex.Message;
            }
        }

        /// <summary>
        /// Resolve any client ids in the string. This is used with Lava when writing out postback commands.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public static string ResolveClientIds( this string content, string clientId )
        {
            return content.Replace( "[ClientId]", clientId );
        }

        /// <summary>
        /// Determines whether string has merge fields in it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool HasMergeFields( this string content )
        {
            if ( content == null )
                return false;

            // If there's no merge codes, just return the content
            if ( !Regex.IsMatch( content, @".*\{.+\}.*" ) )
                return false;

            return true;
        }

        /// <summary>
        /// Converts string to a HTML title "<span class='first-word'>first-word</span> rest of string".
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string FormatAsHtmlTitle( this string str )
        {
            if ( !string.IsNullOrWhiteSpace( str ) )
            {
                // Remove any HTML
                string encodedStr = System.Web.HttpUtility.HtmlEncode( str );

                // split first word from rest of string
                int endOfFirstWord = encodedStr.IndexOf( " " );

                if ( endOfFirstWord != -1 )
                {
                    return "<span class='first-word'>" + encodedStr.Substring( 0, endOfFirstWord ) + " </span> " + encodedStr.Substring( endOfFirstWord, encodedStr.Length - endOfFirstWord );
                }
                else
                {
                    return "<span class='first-word'>" + encodedStr + " </span>";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts CR (carriage return) LF (line feed) to non-encoded html breaks (br).
        /// </summary>
        /// <param name="str">a string that contains CR LF</param>
        /// <returns>a string with CRLF replaced with html <code>br</code></returns>
        public static string ConvertCrLfToHtmlBr( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            return str.Replace( Environment.NewLine, "<br/>" ).Replace( "\x0A", "<br/>" );
        }

        /// <summary>
        /// Converts the HTML br to cr lf.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ConvertBrToCrLf( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            return str.Replace( "<br/>", Environment.NewLine ).Replace( "<br>", Environment.NewLine );
        }

        /// <summary>
        /// HTML Encodes the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string EncodeHtml( this string str )
        {
            return System.Web.HttpUtility.HtmlEncode( str );
        }

        /// <summary>
        /// Sanitizes the HTML by removing tags.  If strict is true, all html tags will be removed, if false, only a blacklist of specific XSS dangerous tags and attribute values are removed.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="strict">if set to <c>true</c> [strict].</param>
        /// <returns></returns>
        public static string SanitizeHtml( this string html, bool strict = true )
        {
            if ( strict )
            {
                // from http://stackoverflow.com/a/18154152/
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml( html );
                return doc.DocumentNode.InnerText;
            }
            else
            {
                return Rock.Web.Utilities.HtmlSanitizer.SanitizeHtml( html );
            }
        }

        /// <summary>
        /// Scrubs any html from the string but converts carriage returns into html &lt;br/&gt; suitable for web display.
        /// </summary>
        /// <param name="str">a string that may contain unsanitized html and carriage returns</param>
        /// <returns>a string that has been scrubbed of any html with carriage returns converted to html br</returns>
        public static string ScrubHtmlAndConvertCrLfToBr( this string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            // Note: \u00A7 is the section symbol

            // First we convert newlines and carriage returns to a character that can
            // pass through the Sanitizer.
            str = str.Replace( Environment.NewLine, "\u00A7" ).Replace( "\x0A", "\u00A7" );

            // Now we pass it to sanitizer and then convert those section-symbols to <br/>
            return str.SanitizeHtml().Replace( "\u00A7", "<br/>" );
        }

        /// <summary>
        /// Returns true if the given string is a valid email address.
        /// </summary>
        /// <param name="email">The string to validate</param>
        /// <returns>true if valid email, false otherwise</returns>
        public static bool IsValidEmail( this string email )
        {
            return Regex.IsMatch( email, @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+" );
        }

        /// <summary>
        /// Converts the value to Type, or if unsuccessful, returns the default value of Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T AsType<T>( this string value )
        {
            var converter = TypeDescriptor.GetConverter( typeof( T ) );
            return converter.IsValid( value )
                ? (T)converter.ConvertFrom( value )
                : default( T );
        }

        /// <summary>
        /// Masks the specified value if greater than 4 characters (such as a credit card number).
        /// For example, the return string becomes "************6789".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Masked( this string value )
        {
            if ( value.Length > 4 )
            {
                return string.Concat( new string( '*', 12 ), value.Substring( value.Length - 4 ) );
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Ensures the trailing backslash. Handy when combining folder paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingBackslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "\\";
        }

        /// <summary>
        /// Ensures the trailing forward slash. Handy when combining url paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingForwardslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "/";
        }

        /// <summary>
        /// Evaluates string, and if null or empty, returns nullValue instead.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="nullValue">The null value.</param>
        /// <returns></returns>
        public static string IfEmpty( this string value, string nullValue )
        {
            return !string.IsNullOrWhiteSpace( value ) ? value : nullValue;
        }

        /// <summary>
        /// Compares to the given value returning true if comparable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="compareValue">The compare value.</param>
        /// <param name="compareType">Type of the compare.</param>
        /// <returns></returns>
        public static bool CompareTo( this string value, string compareValue, ComparisonType compareType )
        {
            if ( compareType == ComparisonType.Contains )
            {
                return value.Contains( compareValue );
            }

            if ( compareType == ComparisonType.DoesNotContain )
            {
                return !value.Contains( compareValue );
            }

            if ( compareType == ComparisonType.EndsWith )
            {
                return value.EndsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.EqualTo )
            {
                return value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.GreaterThan )
            {
                return value.CompareTo( compareValue ) > 0;
            }

            if ( compareType == ComparisonType.GreaterThanOrEqualTo )
            {
                return value.CompareTo( compareValue ) >= 0;
            }

            if ( compareType == ComparisonType.IsBlank )
            {
                return string.IsNullOrWhiteSpace( value );
            }

            if ( compareType == ComparisonType.IsNotBlank )
            {
                return !string.IsNullOrWhiteSpace( value );
            }

            if ( compareType == ComparisonType.LessThan )
            {
                return value.CompareTo( compareValue ) < 0;
            }

            if ( compareType == ComparisonType.LessThanOrEqualTo )
            {
                return value.CompareTo( compareValue ) <= 0;
            }

            if ( compareType == ComparisonType.NotEqualTo )
            {
                return !value.Equals( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            if ( compareType == ComparisonType.StartsWith )
            {
                return value.StartsWith( compareValue, StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }

        /// <summary>
        /// Replaces special Microsoft Word chars with standard chars
        /// For example, smart quotes will be replaced with apostrophes
        /// from http://www.andornot.com/blog/post/Replace-MS-Word-special-characters-in-javascript-and-C.aspx
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ReplaceWordChars( this string text )
        {
            var s = text;
            // smart single quotes and apostrophe
            s = Regex.Replace( s, "[\u2018\u2019\u201A]", "'" );
            // smart double quotes
            s = Regex.Replace( s, "[\u201C\u201D\u201E]", "\"" );
            // ellipsis
            s = Regex.Replace( s, "\u2026", "..." );
            // dashes
            s = Regex.Replace( s, "[\u2013\u2014]", "-" );
            // circumflex
            s = Regex.Replace( s, "\u02C6", "^" );
            // open angle bracket
            s = Regex.Replace( s, "\u2039", "<" );
            // close angle bracket
            s = Regex.Replace( s, "\u203A", ">" );
            // spaces
            s = Regex.Replace( s, "[\u02DC\u00A0]", " " );

            return s;
        }

        #endregion String Extensions

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
                return definedValue.Value;
            else
                return string.Empty;
        }

        #endregion Int Extensions

        #region Boolean Extensions

        /// <summary>
        /// Returns a numeric 1 (if true) or 0 (if false).
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static int Bit( this Boolean field )
        {
            return field ? 1 : 0;
        }

        /// <summary>
        /// Returns either "Yes" or "No".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToYesNo( this bool value )
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// Returns the string "True" or "False".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToTrueFalse( this bool value )
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// Use AsBoolean() instead.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Obsolete( "Use AsBoolean() instead" )]
        public static bool FromTrueFalse( this string value )
        {
            return value.Equals( "True" );
        }

        #endregion Boolean Extensions

        #region DateTime Extensions

        /// <summary>
        /// Returns the age at the current date.
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
        /// Returns the age at the current date.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int Age( this DateTime start )
        {
            var now = RockDateTime.Today;
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
            DateTime end = RockDateTime.Now;

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
                else if ( timeSpan.TotalMinutes < 2 )
                    duration = string.Format( "1{0}", condensed ? "min" : " Minute" );
                else if ( timeSpan.TotalMinutes < 60 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalMinutes ), condensed ? "min" : " Minutes" );
                else if ( timeSpan.TotalHours < 2 )
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

            return duration + ( condensed ? "" : direction );
        }

        /// <summary>
        /// Returns a string in FB style relative format (x seconds ago, x minutes ago, about an hour ago, etc.).
        /// or if max days has already passed in FB datetime format (February 13 at 11:28am or November 5, 2011 at 1:57pm).
        /// </summary>
        /// <param name="dateTime">the datetime to convert to relative time.</param>
        /// <param name="maxDays">maximum number of days before formatting in FB date-time format (ex. November 5, 2011 at 1:57pm)</param>
        /// <returns></returns>
        public static string ToRelativeDateString( this DateTime? dateTime, int? maxDays = null )
        {
            if ( dateTime.HasValue )
            {
                return dateTime.Value.ToRelativeDateString( maxDays );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a string in relative format (x seconds ago, x minutes ago, about an hour ago, in x seconds,
        /// in x minutes, in about an hour, etc.) or if time difference is greater than max days in long format (February
        /// 13 at 11:28am or November 5, 2011 at 1:57pm).
        /// </summary>
        /// <param name="dateTime">the datetime to convert to relative time.</param>
        /// <param name="maxDays">maximum number of days before formatting in long format (ex. November 5, 2011 at 1:57pm) </param>
        /// <returns></returns>
        public static string ToRelativeDateString( this DateTime dateTime, int? maxDays = null )
        {
            try
            {
                DateTime now = RockDateTime.Now;

                string nowText = "just now";
                string format = "{0} ago"; ;
                TimeSpan timeSpan = now - dateTime;
                if ( dateTime > now )
                {
                    nowText = "now";
                    format = "in {0}";
                    timeSpan = dateTime - now;
                }

                double seconds = timeSpan.TotalSeconds;
                double minutes = timeSpan.TotalMinutes;
                double hours = timeSpan.TotalHours;
                double days = timeSpan.TotalDays;
                double weeks = days / 7;
                double months = days / 30;
                double years = days / 365;

                // Just return in long format if max days has passed.
                if ( maxDays.HasValue && days > maxDays )
                {
                    if ( now.Year == dateTime.Year )
                    {
                        return dateTime.ToString( @"MMMM d a\t h:mm tt" );
                    }
                    else
                    {
                        return dateTime.ToString( @"MMMM d, yyyy a\t h:mm tt" );
                    }
                }

                if ( Math.Round( seconds ) < 5 )
                {
                    return nowText;
                }
                else if ( minutes < 1.0 )
                {
                    return string.Format( format, Math.Floor( seconds ) + " seconds" );
                }
                else if ( Math.Floor( minutes ) == 1 )
                {
                    return string.Format( format, "1 minute" );
                }
                else if ( hours < 1.0 )
                {
                    return string.Format( format, Math.Floor( minutes ) + " minutes" );
                }
                else if ( Math.Floor( hours ) == 1 )
                {
                    return string.Format( format, "about an hour" );
                }
                else if ( days < 1.0 )
                {
                    return string.Format( format, Math.Floor( hours ) + " hours" );
                }
                else if ( Math.Floor( days ) == 1 )
                {
                    return string.Format( format, "1 day" );
                }
                else if ( weeks < 1 )
                {
                    return string.Format( format, Math.Floor( days ) + " days" );
                }
                else if ( Math.Floor( weeks ) == 1 )
                {
                    return string.Format( format, "1 week" );
                }
                else if ( months < 3 )
                {
                    return string.Format( format, Math.Floor( weeks ) + " weeks" );
                }
                else if ( months <= 12 )
                {
                    return string.Format( format, Math.Floor( months ) + " months" );
                }
                else if ( Math.Floor( years ) <= 1 )
                {
                    return string.Format( format, "1 year" );
                }
                else
                {
                    return string.Format( format, Math.Floor( years ) + " years" );
                }
            }
            catch ( Exception )
            {
            }
            return "";
        }

        /// <summary>
        /// Converts the date to an Epoch of milliseconds since 1970/1/1.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long ToJavascriptMilliseconds( this DateTime dateTime )
        {
            return (long)( dateTime.ToUniversalTime() - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
        }

        /// <summary>
        /// Converts the date to a string containing month and day values ( culture-specific ).
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static string ToMonthDayString( this DateTime dateTime )
        {
            var dtf = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            string mdp = dtf.ShortDatePattern;
            mdp = mdp.Replace( dtf.DateSeparator + "yyyy", "" ).Replace( "yyyy" + dtf.DateSeparator, "" );
            return dateTime.ToString( mdp );
        }

        #endregion DateTime Extensions

        #region TimeSpan Extensions

        /// <summary>
        /// Returns a TimeSpan as h:mm AM/PM (culture invariant)
        /// Examples: 1:45 PM, 12:01 AM
        /// </summary>
        /// <param name="timespan">The timespan.</param>
        /// <returns></returns>
        public static string ToTimeString( this TimeSpan timespan )
        {
            // since the comments on this say HH:MM AM/PM, make sure to return the time in that format
            return RockDateTime.Today.Add( timespan ).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion TimeSpan Extensions

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
        /// Returns the parent update panel for the given control (or null if none is found).
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static System.Web.UI.UpdatePanel ParentUpdatePanel( this System.Web.UI.Control control )
        {
            System.Web.UI.Control parentControl = control.Parent;
            while ( parentControl != null )
            {
                if ( parentControl is System.Web.UI.UpdatePanel )
                {
                    return (System.Web.UI.UpdatePanel)parentControl;
                }
                parentControl = parentControl.Parent;
            }
            return null;
        }

        /// <summary>
        /// Gets all controls of Type recursively.
        /// http://stackoverflow.com/questions/7362482/c-sharp-get-all-web-controls-on-page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlCollection">The control collection.</param>
        /// <param name="resultCollection">The result collection.</param>
        private static void GetControlListRecursive<T>( this System.Web.UI.ControlCollection controlCollection, List<T> resultCollection ) where T : System.Web.UI.Control
        {
            foreach ( System.Web.UI.Control control in controlCollection )
            {
                if ( control is T )
                {
                    resultCollection.Add( (T)control );
                }

                if ( control.HasControls() )
                {
                    GetControlListRecursive( control.Controls, resultCollection );
                }
            }
        }

        /// <summary>
        /// Gets all controls of Type recursively.
        /// http://stackoverflow.com/questions/7362482/c-sharp-get-all-web-controls-on-page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static List<T> ControlsOfTypeRecursive<T>( this System.Web.UI.Control control ) where T : System.Web.UI.Control
        {
            List<T> result = new List<T>();
            GetControlListRecursive<T>( control.Controls, result );
            return result;
        }

        #endregion Control Extensions

        #region WebControl Extensions

        /// <summary>
        /// Adds a CSS class name to a web control.
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static WebControl AddCssClass( this WebControl webControl, string className )
        {
            string match = @"(^|\s+)" + className + @"($|\s+)";
            string css = webControl.CssClass;

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
            {
                css += " " + className;
            }

            webControl.CssClass = css.Trim();

            return webControl;
        }

        /// <summary>
        /// Removes a CSS class name from a web control.
        /// </summary>
        /// <param name="webControl">The web control.</param>
        /// <param name="className">Name of the class.</param>
        public static WebControl RemoveCssClass( this WebControl webControl, string className )
        {
            string match = @"(^|\s+)" + className + @"($|\s+)";
            string css = webControl.CssClass;

            while ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
            {
                css = Regex.Replace( css, match, " ", RegexOptions.IgnoreCase );
            }

            webControl.CssClass = css.Trim();

            return webControl;
        }

        #endregion WebControl Extensions

        #region HtmlControl Extensions

        /// <summary>
        /// Adds a CSS class name to an html control.
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

        #endregion HtmlControl Extensions

        #region CheckBoxList Extensions

        /// <summary>
        /// Sets the Selected property of each item to true for each given matching string values.
        /// </summary>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="values">The values.</param>
        public static void SetValues( this CheckBoxList checkBoxList, List<string> values )
        {
            foreach ( ListItem item in checkBoxList.Items )
            {
                item.Selected = values.Contains( item.Value, StringComparer.OrdinalIgnoreCase );
            }
        }

        /// <summary>
        /// Sets the Selected property of each item to true for each given matching int values.
        /// </summary>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="values">The values.</param>
        public static void SetValues( this CheckBoxList checkBoxList, List<int> values )
        {
            foreach ( ListItem item in checkBoxList.Items )
            {
                int numValue = int.MinValue;
                item.Selected = int.TryParse( item.Value, out numValue ) && values.Contains( numValue );
            }
        }

        #endregion CheckBoxList Extensions

        #region ListControl Extensions

        /// <summary>
        /// Tries to set the selected value, if the value does not exist, it will attempt to set the value to defaultValue (if specified), 
        /// otherwise it will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        public static void SetValue( this ListControl listControl, string value, string defaultValue = null )
        {
            try
            {
                var valueItem = listControl.Items.FindByValue( value );
                if (valueItem == null && defaultValue != null)
                {
                    valueItem = listControl.Items.FindByValue( defaultValue );
                }

                if ( valueItem != null )
                {
                    listControl.SelectedValue = valueItem.Value;
                }
                else
                {
                    if ( listControl.Items.Count > 0 )
                    {
                        listControl.SelectedIndex = 0;
                    }
                }
            }
            catch
            {
                if ( listControl.Items.Count > 0 )
                {
                    listControl.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Sets the read only value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetReadOnlyValue( this ListControl listControl, string value )
        {
            listControl.Items.Clear();
            listControl.Items.Add( value );
        }

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this ListControl listControl, int? value )
        {
            listControl.SetValue( value == null ? "0" : value.ToString() );
        }

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this ListControl listControl, Guid? value )
        {
            listControl.SetValue( value == null ? "" : value.ToString() );
        }

        /// <summary>
        /// Sets the value to the entity's id value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="entity">The entity.</param>
        public static void SetValue( this ListControl listControl, IEntity entity )
        {
            listControl.SetValue( entity == null ? "0" : entity.Id.ToString() );
        }

        /// <summary>
        /// Binds to enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listControl">The list control.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        public static void BindToEnum<T>( this ListControl listControl, bool insertBlankOption = false )
        {
            var enumType = typeof( T );
            var dictionary = new Dictionary<int, string>();
            foreach ( var value in Enum.GetValues( enumType ) )
            {
                dictionary.Add( Convert.ToInt32( value ), Enum.GetName( enumType, value ).SplitCase() );
            }

            listControl.DataSource = dictionary;
            listControl.DataTextField = "Value";
            listControl.DataValueField = "Key";
            listControl.DataBind();

            if ( insertBlankOption )
            {
                listControl.Items.Insert( 0, new ListItem() );
            }
        }

        /// <summary>
        /// Binds to the values of a definedType.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="definedType">Type of the defined.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        /// <param name="useDescriptionAsText">if set to <c>true</c> [use description as text].</param>
        public static void BindToDefinedType( this ListControl listControl, Rock.Web.Cache.DefinedTypeCache definedType, bool insertBlankOption = false, bool useDescriptionAsText = false )
        {
            var ds = definedType.DefinedValues
                .Select( v => new
                {
                    Name = v.Value,
                    v.Description,
                    v.Id
                } );

            listControl.SelectedIndex = -1;
            listControl.DataSource = ds;
            listControl.DataTextField = useDescriptionAsText ? "Description" : "Name";
            listControl.DataValueField = "Id";
            listControl.DataBind();

            if ( insertBlankOption )
            {
                listControl.Items.Insert( 0, new ListItem() );
            }
        }

        /// <summary>
        /// Returns the Value as Int or null if Value is <see cref="T:Rock.Constants.None"/>.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="NoneAsNull">if set to <c>true</c>, will return Null if SelectedValue = <see cref="T:Rock.Constants.None" /> </param>
        /// <returns></returns>
        public static int? SelectedValueAsInt( this ListControl listControl, bool NoneAsNull = true )
        {
            if ( NoneAsNull )
            {
                if ( listControl.SelectedValue.Equals( Rock.Constants.None.Id.ToString() ) )
                {
                    return null;
                }
            }

            if ( string.IsNullOrWhiteSpace( listControl.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return int.Parse( listControl.SelectedValue );
            }
        }

        /// <summary>
        /// Returns the value of the currently selected item.
        /// It will return NULL if either <see cref="T:Rock.Constants.None"/> or <see cref="T:Rock.Constants.All"/> is selected.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public static int? SelectedValueAsId( this ListControl listControl )
        {
            int? result = SelectedValueAsInt( listControl );

            if ( result == Rock.Constants.All.Id )
            {
                return null;
            }

            if ( result == Rock.Constants.None.Id )
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Selecteds the value as enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T SelectedValueAsEnum<T>( this ListControl listControl, T? defaultValue = null ) where T : struct
        {
            return listControl.SelectedValue.ConvertToEnum<T>( defaultValue );
        }

        /// <summary>
        /// Selecteds the value as enum or null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listControl">The list control.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T? SelectedValueAsEnumOrNull<T>( this ListControl listControl, T? defaultValue = null ) where T : struct
        {
            return listControl.SelectedValue.ConvertToEnumOrNull<T>( defaultValue );
        }

        /// <summary>
        /// Selecteds the value as unique identifier.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public static Guid? SelectedValueAsGuid( this ListControl listControl )
        {
            if ( string.IsNullOrWhiteSpace( listControl.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return listControl.SelectedValue.AsGuidOrNull();
            }
        }

        #endregion ListControl Extensions

        #region Enum Extensions

        /// <summary>
        /// Converts to the enum value to it's string value.
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <param name="SplitCase">if set to <c>true</c> [split case].</param>
        /// <returns></returns>
        public static String ConvertToString( this Enum eff, bool SplitCase = true )
        {
            if ( SplitCase )
            {
                return Enum.GetName( eff.GetType(), eff ).SplitCase();
            }
            else
            {
                return Enum.GetName( eff.GetType(), eff );
            }
        }

        /// <summary>
        /// Gets the enum description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetDescription( this Enum value )
        {
            var type = value.GetType();
            string name = Enum.GetName( type, value );
            if ( name != null )
            {
                System.Reflection.FieldInfo field = type.GetField( name );
                if ( field != null )
                {
                    var attr = System.Attribute.GetCustomAttribute( field,
                        typeof( DescriptionAttribute ) ) as DescriptionAttribute;
                    if ( attr != null )
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
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
        /// <param name="defaultValue">The default value to use if the value cannot be parsed. Leave null to throw an exception if the value cannot be parsed. </param>
        /// <returns></returns>
        public static T ConvertToEnum<T>( this string enumValue, T? defaultValue = null ) where T : struct // actually limited to enum, but struct is the closest we can do
        {
            T? result = ConvertToEnumOrNull<T>( enumValue, defaultValue );
            if ( result.HasValue )
            {
                return result.Value;
            }
            else
            {
                throw new Exception( string.Format( "'{0}' is not a member of the {1} enumeration.", enumValue, typeof( T ).Name ) );
            }
        }

        /// <summary>
        /// Converts to enum or null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T? ConvertToEnumOrNull<T>( this string enumValue, T? defaultValue = null ) where T : struct // actually limited to enum, but struct is the closest we can do
        {
            T result;
            if ( Enum.TryParse<T>( ( enumValue ?? "" ).Replace( " ", "" ), out result ) && Enum.IsDefined( typeof( T ), result ) )
            {
                return result;
            }
            else
            {
                if ( defaultValue.HasValue )
                {
                    return defaultValue.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion Enum Extensions

        #region GenericCollection Extensions

        /// <summary>
        /// Concatonate the items.
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
        /// Converts a List&lt;string&gt; to List&lt;guid&gt; only returning items that could be converted to a guid.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<Guid> AsGuidList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsGuidOrNull() ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
        }

        /// <summary>
        /// Converts a List&lt;string&gt; to List&lt;guid&gt; return a null for items that could not be converted to a guid
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<Guid?> AsGuidOrNullList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsGuidOrNull() ).ToList();
        }

        /// <summary>
        /// Converts a List&lt;string&gt; to List&lt;int&gt; only returning items that could be converted to a int.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<int> AsIntegerList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsIntegerOrNull() ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
        }

        /// <summary>
        /// Joins a dictionary of items.
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

        /// <summary>
        /// Recursively flattens the specified source.
        /// http://stackoverflow.com/questions/5422735/how-do-i-select-recursive-nested-entities-using-linq-to-entity/5423024#5423024
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="childSelector">The child selector.</param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>( this IEnumerable<T> source, Func<T, IEnumerable<T>> childSelector )
        {
            // Flatten all the items without recursion or potential memory overflow
            var itemStack = new Stack<T>( source );
            while ( itemStack.Count > 0 )
            {
                T item = itemStack.Pop();
                yield return item;

                foreach ( T child in childSelector( item ) )
                {
                    itemStack.Push( child );
                }
            }
        }

        #endregion GenericCollection Extensions

        #region IQueryable extensions

        #region Where

        /// <summary>
        /// Queries a list of items that match the specified expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Where( queryable, parameterExpression, whereExpression, null, null );
        }

        /// <summary>
        /// Queries a list of items that match the specified expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            return Where( queryable, parameterExpression, whereExpression, sortProperty, null );
        }

        /// <summary>
        /// Queries the specified parameter expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="fetchTop">The fetch top.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty, int? fetchTop = null )
        {
            if ( parameterExpression != null && whereExpression != null )
            {
                var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
                queryable = queryable.Where( lambda );
            }

            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }

            if ( fetchTop.HasValue )
            {
                queryable = queryable.Take( fetchTop.Value );
            }

            return queryable;
        }

        #endregion Where

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
        /// Orders the list by the name of a property in descending order.
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
        /// Then Orders the list by a a property in descending order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>( this IOrderedQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "ThenByDescending" );
        }

        private static IOrderedQueryable<T> ApplyOrder<T>( IQueryable<T> source, string property, string methodName )
        {
            string[] props = property.Split( '.' );
            Type type = typeof( T );
            ParameterExpression arg = Expression.Parameter( type, "x" );
            Expression expr = arg;
            foreach ( string prop in props )
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty( prop );
                if ( pi != null )
                {
                    expr = Expression.Property( expr, pi );
                    type = pi.PropertyType;
                }
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
            string[] columns = sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            IOrderedQueryable<T> qry = null;

            for ( int columnIndex = 0; columnIndex < columns.Length; columnIndex++ )
            {
                string column = columns[columnIndex].Trim();

                var direction = sortProperty.Direction;
                if ( column.ToLower().EndsWith( " desc" ) )
                {
                    column = column.Left( column.Length - 5 );
                    direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                }

                if ( direction == SortDirection.Ascending )
                {
                    qry = ( columnIndex == 0 ) ? source.OrderBy( column ) : qry.ThenBy( column );
                }
                else
                {
                    qry = ( columnIndex == 0 ) ? source.OrderByDescending( column ) : qry.ThenByDescending( column );
                }
            }

            return qry;
        }

        /// <summary>
        /// Filters a Query to rows that have matching attribute value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        /// <example>
        /// var test = new PersonService( rockContext ).Queryable().Where( a =&gt; a.FirstName == "Bob" ).WhereAttributeValue( rockContext, "BaptizedHere", "True" ).ToList();
        ///   </example>
        public static IQueryable<T> WhereAttributeValue<T>( this IQueryable<T> source, RockContext rockContext, string attributeKey, string attributeValue ) where T : Rock.Data.Model<T>, new()
        {
            int entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( typeof( T ) ) ?? 0;

            var avs = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == attributeKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                .Where( a => a.Value == attributeValue )
                .Select( a => a.EntityId );

            var result = source.Where( a => avs.Contains( ( a as T ).Id ) );
            return result;
        }

        #endregion IQueryable extensions

        #region IHasAttributes extensions

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this Rock.Attribute.IHasAttributes entity, RockContext rockContext = null )
        {
            Rock.Attribute.Helper.LoadAttributes( entity, rockContext );
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this Rock.Attribute.IHasAttributes entity, RockContext rockContext = null )
        {
            Rock.Attribute.Helper.SaveAttributeValues( entity, rockContext );
        }

        /// <summary>
        /// Copies the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="source">The source.</param>
        public static void CopyAttributesFrom( this Rock.Attribute.IHasAttributes entity, Rock.Attribute.IHasAttributes source )
        {
            Rock.Attribute.Helper.CopyAttributes( source, entity );
        }

        #endregion IHasAttributes extensions

        #region Route Extensions

        /// <summary>
        /// Returns the page Id of the route or -1 if not found.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int PageId( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["PageId"] != null  )
            {
                return ( route.DataTokens["PageId"] as string ).AsIntegerOrNull() ?? -1;
            }

            return -1;
        }

        /// <summary>
        /// Returns the route Id of the route or -1 if not found.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public static int RouteId( this Route route )
        {
            if ( route.DataTokens != null && route.DataTokens["RouteId"] != null )
            {
                return ( route.DataTokens["RouteId"] as string ).AsIntegerOrNull() ?? -1;
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

        #endregion Route Extensions

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

        #endregion IEntity extensions

        #region HiddenField Extensions

        /// <summary>
        /// Returns the values as an int or 0 if not found or not an int.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <returns></returns>
        public static int ValueAsInt( this HiddenField hiddenField )
        {
            int intValue = 0;
            if ( int.TryParse( hiddenField.Value, out intValue ) )
            {
                return intValue;
            }

            return 0;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( this HiddenField hiddenField, int value )
        {
            hiddenField.Value = value.ToString();
        }

        /// <summary>
        /// Determines whether the specified hidden field is zero.
        /// </summary>
        /// <param name="hiddenField">The hidden field.</param>
        /// <returns>
        ///   <c>true</c> if the specified hidden field is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero( this HiddenField hiddenField )
        {
            return hiddenField.Value.Equals( "0" );
        }

        #endregion HiddenField Extensions

        #region Dictionary<string, object> (liquid) extension methods

        /// <summary>
        /// Adds a new key/value to dictionary or if key already exists will update existing value.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void Update( this Dictionary<string, object> dictionary, string key, object value )
        {
            if ( dictionary != null )
            {
                if ( dictionary.ContainsKey( key ) )
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add( key, value );
                }
            }
        }

        #endregion Dictionary<string, object> (liquid) extension methods

        #region Dictionary<TKey, TValue> extension methods

        /// <summary>
        /// Adds or replaces an item in a Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrReplace<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
            else
            {
                dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an item to a Dictionary if it doesn't already exist in Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrIgnore<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
        }

        /// <summary>
        /// Gets value for the specified key, or null if the dictionary doesn't contain the key
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TValue GetValueOrNull<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return default(TValue);
            }
        }

        /// <summary>
        /// Gets ConfigurationValue's Value for the specified key, or null if the dictionary doesn't contain the key or the ConfigurationValue is null
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValueOrNull<TKey>( this Dictionary<TKey, Rock.Field.ConfigurationValue> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) && dictionary[key] != null )
            {
                return dictionary[key].Value;
            }
            else
            {
                return null;
            }
        }

        #endregion Dictionary<TKey, TValue> extension methods

        #region Geography extension methods

        /// <summary>
        /// Coordinateses the specified geography.
        /// </summary>
        /// <param name="geography">The geography.</param>
        /// <returns></returns>
        public static List<MapCoordinate> Coordinates( this System.Data.Entity.Spatial.DbGeography geography )
        {
            var coordinates = new List<MapCoordinate>();

            var match = Regex.Match( geography.AsText(), @"(?<=POLYGON \(\()[^\)]*(?=\)\))" );
            if ( match.Success )
            {
                string[] longSpaceLat = match.ToString().Split( ',' );

                for ( int i = 0; i < longSpaceLat.Length; i++ )
                {
                    string[] longLat = longSpaceLat[i].Trim().Split( ' ' );
                    if ( longLat.Length == 2 )
                    {
                        double? lat = longLat[1].AsDoubleOrNull();
                        double? lon = longLat[0].AsDoubleOrNull();
                        if ( lat.HasValue && lon.HasValue )
                        {
                            coordinates.Add( new MapCoordinate( lat, lon ) );
                        }
                    }
                }
            }

            return coordinates;
        }

        #endregion Geography extension methods

        #region Stream extension methods

        /// <summary>
        /// Reads entire stream and converts to byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static byte[] ReadBytesToEnd( this System.IO.Stream stream )
        {
            long originalPosition = 0;

            if ( stream.CanSeek )
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ( ( bytesRead = stream.Read( readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead ) ) > 0 )
                {
                    totalBytesRead += bytesRead;

                    if ( totalBytesRead == readBuffer.Length )
                    {
                        int nextByte = stream.ReadByte();
                        if ( nextByte != -1 )
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy( readBuffer, 0, temp, 0, readBuffer.Length );
                            Buffer.SetByte( temp, totalBytesRead, (byte)nextByte );
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if ( readBuffer.Length != totalBytesRead )
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy( readBuffer, 0, buffer, 0, totalBytesRead );
                }
                return buffer;
            }
            finally
            {
                if ( stream.CanSeek )
                {
                    stream.Position = originalPosition;
                }
            }
        }

        #endregion Stream extension methods
    }
}