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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid;
using Rock.Data;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Rock Lava related extensions
    /// </summary>
    public static class LavaExtensions
    {
        #region Lava Extensions

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
            if ( !( lavaObject is IDictionary<string, object> ) || !( (IDictionary<string, object>)lavaObject ).Keys.Contains( "GlobalAttribute" ) )
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

            // If the object is liquidable, get the object return by its ToLiquid() method.
            if ( myObject is DotLiquid.ILiquidizable )
            {
                myObject = ( (DotLiquid.ILiquidizable)myObject ).ToLiquid();
            }

            // If the object is null, return an empty string
            if ( myObject == null )
            {
                return string.Empty;
            }

            // If the object is a string, return its value converted to HTML and truncated
            if ( myObject is string )
            {
                return myObject.ToString().Truncate( 50 ).EncodeHtml();
            }

            // If the object is a guid, return its string representation
            if ( myObject is Guid )
            {
                return myObject.ToString();
            }

            // Get the object's type ( checking for a proxy object )
            Type entityType = myObject.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

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
                    // Ignore the person property of the person's primary alias (prevent unnecessary recursion) 
                    if ( key == "Person" && parentElement.Contains( ".PrimaryAlias" ) )
                    {
                        result.AddOrIgnore( key, string.Empty );
                    }
                    else
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
                        result.Add( string.Format( "Attributes <p class='attributes'>Below is a list of attributes that can be retrieved using <code>{{{{ {0} | Attribute:'[AttributeKey]' }}}}</code>.</p>", parentElement ), objAttrs );
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

            if ( myObject is Newtonsoft.Json.Linq.JObject )
            {
                var result = new Dictionary<string, object>();
                var jObject = myObject as Newtonsoft.Json.Linq.JObject;

                foreach ( var keyValue in jObject )
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

            if ( myObject is Newtonsoft.Json.Linq.JValue )
            {
                var jValue = ( myObject as Newtonsoft.Json.Linq.JValue );
                if ( jValue != null && jValue.Value != null )
                {
                    return jValue.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
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

            if ( liquidizedObject is IDictionary<string, object> )
            {
                var sb = new StringBuilder();

                bool isTopLevel = levelsDeep == 0;

                if ( !isTopLevel )
                {
                    sb.AppendFormat( "{0}<ul>{0}", Environment.NewLine );
                }

                foreach ( var keyVal in (IDictionary<string, object>)liquidizedObject )
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
        /// <param name="encodeStrings">if set to <c>true</c>, string values will be XML Encoded. For example, if you are creating an XML doc (not HTML), you probably want to set this to true.</param>
        /// <param name="throwExceptionOnErrors">if set to <c>true</c> [throw exception on errors].</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            try
            {
                if ( !content.HasMergeFields() )
                {
                    return content ?? string.Empty;
                }

                Template template = Template.Parse( content );

                // if encodeStrings = true, we want any string values to be XML Encoded ( 
                var stringTransformer = Template.GetValueTypeTransformer( typeof( string ) );
                if ( encodeStrings )
                {
                    // if the stringTransformer hasn't been registered yet, register it
                    if ( stringTransformer == null )
                    {
                        Template.RegisterValueTypeTransformer( typeof( string ), ( s ) =>
                        {
                            string val = ( s as string );
                            if ( val != null )
                            {
                                // we techically want to XML encode, but Html Encode does the trick
                                return val.EncodeHtml();
                            }
                            else
                            {
                                return s;
                            }
                        } );
                    }
                }
                else
                {
                    // if the stringTransformer is registered, un-register it
                    if ( stringTransformer != null )
                    {
                        Template.RegisterValueTypeTransformer( typeof( string ), null );
                    }
                }

                return template.Render( Hash.FromDictionary( mergeObjects ) );
            }
            catch ( Exception ex )
            {
                if ( throwExceptionOnErrors )
                {
                    throw ex;
                }
                else
                {
                    return "Error resolving Lava merge fields: " + ex.Message;
                }
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

        #endregion Lava Extensions

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
    }
}
