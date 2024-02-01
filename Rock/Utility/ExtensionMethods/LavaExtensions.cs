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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock
{

    /// <summary>
    /// Rock Lava related extensions
    /// </summary>
    public static partial class ExtensionMethods
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

            lavaDebugPanel.Append( "<p>Below is a listing of available merge fields for this block. Find out more on Lava at <a href='http://www.rockrms.com/lava' target='_blank' rel='noopener noreferrer'>rockrms.com/lava</a>." );


            int maxWaitMS = 10000;
            System.Web.HttpContext taskContext = System.Web.HttpContext.Current;
            var formatLavaTask = new Task( () =>
            {
                System.Web.HttpContext.Current = taskContext;
                lavaDebugPanel.Append( formatLavaDebugInfo( lavaObject.LiquidizeChildren( 0, rockContext ) ) );
            } );

            formatLavaTask.Start();

            if ( !formatLavaTask.Wait( maxWaitMS ) )
            {
                return "<div class='alert alert-warning lava-debug'>Warning: Timeout generating Lava Help</div>";
            }

            // Add a 'GlobalAttribute' entry if it wasn't part of the LavaObject
            if ( !( lavaObject is IDictionary<string, object> ) || !( (IDictionary<string, object>)lavaObject ).Keys.Contains( "GlobalAttribute" ) )
            {
                var globalAttributes = new Dictionary<string, object>();

                // Lava Help Text does special stuff for GlobalAttribute, but it still needs the list of possible Global Attribute MergeFields to generate the help text
                globalAttributes.Add( "GlobalAttribute", GlobalAttributesCache.GetLegacyMergeFields( null ) );
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
        /// <param name="entityHistory">The entity history.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <returns></returns>
        private static object LiquidizeChildren( this object myObject, int levelsDeep = 0, RockContext rockContext = null, Dictionary<int, List<int>> entityHistory = null, string parentElement = "" )
        {
            // Add protection for stack-overflow if property attributes are not set correctly resulting in child/parent objects being evaluated in loop
            levelsDeep++;
            if ( levelsDeep > 6)
            {
                return string.Empty;
            }

            // If the object is liquidable, get the object return by its ToLiquid() method.
            if ( myObject is ILavaDataDictionarySource )
            {
                myObject = ( (ILavaDataDictionarySource)myObject ).GetLavaDataDictionary();
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

            // If this is an IEntity, check to see if it's already been liquidized in prev hierarchy. If so, just return string indicating "--See Previous Entry--"
            if ( myObject is IEntity )
            {
                var entity = myObject as IEntity;
                var entityTypeCache = EntityTypeCache.Get( entityType, false, rockContext );
                if ( entity != null && entityTypeCache != null )
                {
                    if ( entityHistory == null )
                    {
                        entityHistory = new Dictionary<int, List<int>>();
                    }
                    entityHistory.AddOrIgnore( entityTypeCache.Id, new List<int>() );
                    if ( entityHistory[entityTypeCache.Id].Contains( entity.Id ) )
                    {
                        return "--See Previous Entry--";
                    }
                    else
                    {
                        entityHistory[entityTypeCache.Id].Add( entity.Id );
                    }
                }
            }

            // If the object has the [LavaType] attribute, enumerate the allowed properties and return a list of those properties
            if ( entityType.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).Any() )
            {
                var result = new Dictionary<string, object>();

                var attr = (LavaTypeAttribute)entityType.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).First();
                foreach ( string propName in attr.AllowedMembers )
                {
                    var propInfo = entityType.GetProperty( propName );
                    {
                        if ( propInfo != null )
                        {
                            try
                            {
                                result.Add( propInfo.Name, propInfo.GetValue( myObject, null ).LiquidizeChildren( levelsDeep, rockContext, entityHistory, parentElement + "." + propName ) );
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
            if ( myObject is ILavaDataDictionary )
            {
                var liquidObject = (ILavaDataDictionary)myObject;

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
                            object propValue = null;

                            // There may be multiple inherited properties with the same name but different return types, so select the first match.
                            var propType = entityType.GetProperties()
                                .Where( x => x.Name == key )
                                .Select( x => x.PropertyType )
                                .FirstOrDefault();

                            if ( propType?.Name == "ICollection`1" )
                            {
                                // if the property type is an ICollection, get the underlying query and just fetch one for an example (just in case there are 1000s of records)
                                var entityDbContext = GetDbContextFromEntity( myObject );
                                if ( entityDbContext != null )
                                {
                                    var entryCollection = entityDbContext.Entry( myObject )?.Collection( key );
                                    if ( entryCollection.EntityEntry.State == EntityState.Detached )
                                    {
                                        // create a sample since we can't fetch real data
                                        Type listOfType = propType.GenericTypeArguments[0];
                                        var sampleListType = typeof( List<> ).MakeGenericType( listOfType );
                                        var sampleList = Activator.CreateInstance(sampleListType) as IList;
                                        var sampleItem = Activator.CreateInstance( listOfType );
                                        sampleList.Add( sampleItem );
                                        propValue = sampleList;
                                    }
                                    else
                                    {
                                        if ( entryCollection.IsLoaded )
                                        {
                                            propValue = liquidObject.GetValue(key);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                var propQry = entryCollection.Query().Provider.CreateQuery<Rock.Data.IEntity>( entryCollection.Query().Expression );
                                                int propCollectionCount = propQry.Count();
                                                List<object> listSample = propQry.Take( 1 ).ToList().Cast<object>().ToList();
                                                if ( propCollectionCount > 1 )
                                                {
                                                    listSample.Add( $"({propCollectionCount - 1} more...)" );
                                                }

                                                propValue = listSample;
                                            }
                                            catch
                                            {
                                                // The Collection might be a database model that isn't an IEntity, so just do it the regular way
                                                propValue = liquidObject.GetValue(key);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                propValue = liquidObject.GetValue(key);
                            }

                            if ( propValue != null )
                            {
                                result.Add( key, propValue.LiquidizeChildren( levelsDeep, rockContext, entityHistory, parentElement + "." + key ) );
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
                if ( liquidObject is IHasAttributes )
                {
                    var objWithAttrs = (IHasAttributes)liquidObject;
                    if ( objWithAttrs.Attributes == null )
                    {
                        rockContext = rockContext ?? new RockContext();
                        objWithAttrs.LoadAttributes( rockContext );
                    }

                    var objAttrs = new Dictionary<string, object>();
                    foreach ( var objAttr in objWithAttrs.Attributes )
                    {
                        var attributeCache = objAttr.Value;
                        string value = attributeCache.FieldType.Field.FormatValue( null, attributeCache.EntityTypeId, objWithAttrs.Id, objWithAttrs.GetAttributeValue( attributeCache.Key ), attributeCache.QualifierValues, false );
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
                        var parentVariable = ( keyValue.Value?.GetType().GetInterface( "IList" ) != null ) ? keyValue.Key.ToLower().Singularize() : keyValue.Key;
                        result.Add( keyValue.Key, keyValue.Value?.LiquidizeChildren( levelsDeep, rockContext, entityHistory, parentVariable ) );
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
                        result.Add( keyValue.Key, keyValue.Value.LiquidizeChildren( levelsDeep, rockContext, entityHistory, keyValue.Key ) );
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

                // Only show first two items in an enumerable list
                int iEnumCount = 1;
                foreach ( var value in ( (IEnumerable)myObject ) )
                {
                    if ( iEnumCount > 2 )
                    {
                        result.Add( "..." );
                        break;
                    }
                    iEnumCount++;
                    try
                    {
                        result.Add( value.LiquidizeChildren( levelsDeep, rockContext, entityHistory, parentElement ) );
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

                            sb.Append( string.Format( "<div class='panel-heading clearfix collapsed' data-toggle='collapse' data-target='#collapse-{0}' onclick='$(\"#collapse-{0}\").collapse(\"toggle\"); event.stopPropagation();'>", panelId ) );
                            sb.Append( string.Format( "<h5 class='panel-title pull-left'>{0}</h5> <div class='pull-right'><i class='fa fa-chevron-up'></i></div>", keyVal.Key.SplitCase() ) );
                            sb.Append( "</div>" );

                            sb.Append( string.Format( "<div id='collapse-{0}' class='panel-collapse collapse'>", panelId ) );
                            sb.Append( "<div class='panel-body'>" );

                            if ( keyVal.Key == "GlobalAttribute" )
                            {
                                sb.Append( "<p>Global attributes should be accessed using <code>{{ 'Global' | Attribute:'[AttributeKey]' }}</code>. Find out more about using Global Attributes in Lava at <a href='http://www.rockrms.com/lava/globalattributes' target='_blank' rel='noopener noreferrer'>rockrms.com/lava/globalattributes</a>.</p>" );
                            }
                            else if ( keyVal.Value is List<object> )
                            {
                                sb.Append( string.Format( "<p>{0} properties can be accessed by <code>{{% for {2} in {1} %}}{{{{ {2}.[PropertyKey] }}}}{{% endfor %}}</code>.</p>", char.ToUpper( keyVal.Key[0] ) + keyVal.Key.Substring( 1 ), keyVal.Key, keyVal.Key.Singularize().ToLower() ) );
                            }
                            else if ( keyVal.Key == "CurrentPerson" )
                            {
                                sb.Append( string.Format( "<p>{0} properties can be accessed by <code>{{{{ {1}.[PropertyKey] }}}}</code>. Find out more about using 'Person' fields in Lava at <a href='http://www.rockrms.com/lava/person' target='_blank' rel='noopener noreferrer'>rockrms.com/lava/person</a>.</p>", char.ToUpper( keyVal.Key[0] ) + keyVal.Key.Substring( 1 ), keyVal.Key ) );
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
        /// Uses reflection to figure out the DbContext associated with the entity
        /// NOTE: the entity needs to be a DynamicProxy
        /// from https://stackoverflow.com/a/43667414/1755417
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private static Data.DbContext GetDbContextFromEntity( object entity )
        {
            FieldInfo entityWrapperField = entity.GetType().GetField( "_entityWrapper" );

            if ( entityWrapperField == null )
                return null;

            var entityWrapper = entityWrapperField.GetValue( entity );
            PropertyInfo entityWrapperContextProperty = entityWrapper.GetType().GetProperty( "Context" );
            var context = ( System.Data.Entity.Core.Objects.ObjectContext ) entityWrapperContextProperty.GetValue( entityWrapper, null );

            return context?.TransactionHandler?.DbContext as Data.DbContext;
        }

        /// <summary>
        /// Use Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride )
        {
            var enabledCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
            return content.ResolveMergeFields( mergeObjects, currentPersonOverride, enabledCommands );
        }

        /// <summary>
        /// Uses Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="encodeStrings">if set to <c>true</c>, string values will be XML Encoded. For example, if you are creating an XML doc (not HTML), you probably want to set this to true.</param>
        /// <param name="throwExceptionOnErrors">if set to <c>true</c> [throw exception on errors].</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            var enabledCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
            return content.ResolveMergeFields( mergeObjects, enabledCommands, encodeStrings, throwExceptionOnErrors );
        }

        /// <summary>
        /// Uses Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="encodeStrings">if set to <c>true</c> [encode strings].</param>
        /// <param name="throwExceptionOnErrors">if set to <c>true</c> [throw exception on errors].</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            return ResolveMergeFields( content, mergeObjects, enabledLavaCommands.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ), encodeStrings, throwExceptionOnErrors );
        }

        /// <summary>
        /// Use Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <param name="enabledLavaCommands">A comma-delimited list of the lava commands that are enabled for this template.</param>
        /// <returns>If lava present returns merged string, if no lava returns original string, if null returns empty string</returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride, string enabledLavaCommands )
        {
            return ResolveMergeFieldsInternal( content, mergeObjects, currentPersonOverride, enabledLavaCommands );
        }

        /// <summary>
        /// Uses Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="encodeStrings">if set to <c>true</c> [encode strings].</param>
        /// <param name="throwExceptionOnErrors">if set to <c>true</c> [throw exception on errors].</param>
        /// <returns></returns>
        public static string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, IEnumerable<string> enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            var enabledCommands = ( enabledLavaCommands == null ? string.Empty : enabledLavaCommands.JoinStrings( "," ) );

            return ResolveMergeFieldsInternal( content, mergeObjects, null, enabledCommands, encodeStrings, throwExceptionOnErrors );
        }

        private static readonly Regex _regexRemoveWhitespace = new Regex( @"\s+", RegexOptions.Compiled );

        private static string ResolveMergeFieldsInternal( this string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride, string enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            try
            {
                if ( !content.IsLavaTemplate() ) 
                {
                    return content ?? string.Empty;
                }

                if ( mergeObjects == null )
                {
                    mergeObjects = new Dictionary<string, object>();
                }

                if ( LavaService.RockLiquidIsEnabled )
                {
                    // If RockLiquid mode is enabled, try to render uncached templates using the current Lava engine and record any errors that occur.
                    // Render the final output using the RockLiquid legacy code.
                    var engine = LavaService.GetCurrentEngine();

                    string lavaEngineOutput = null;
                    string rockLiquidOutput;

                    if ( engine != null )
                    {
                        var isCached = false;
                        string cacheKey = null;
                        if ( engine.TemplateCacheService != null )
                        {
                            cacheKey = engine.TemplateCacheService.GetCacheKeyForTemplate( content );
                            isCached = engine.TemplateCacheService.ContainsKey( cacheKey );
                        }

                        if ( !isCached )
                        {
                            bool addCachePlaceholder = false;

                            // Verify the template, unless it contains tokens that are non-idempotent.
                            // These templates cause non-repeatable state change, so multiple executions may produce unwanted results.
                            var canVerify = !_nonIdempotentLavaTokens.IsMatch( content );
                            if ( canVerify )
                            {
                                // Verify the Lava template using the current LavaEngine.
                                // Although it would improve performance, we can't execute this task on a background thread because some Lava filters require access to the current HttpRequest.
                                try
                                {
                                    var result = ResolveMergeFieldsWithCurrentLavaEngine( content, mergeObjects, currentPersonOverride, enabledLavaCommands, encodeStrings );

                                    // If an exception occurred during the render process, make sure it will be caught and logged.
                                    if ( result.HasErrors
                                         && engine.ExceptionHandlingStrategy != ExceptionHandlingStrategySpecifier.Throw )
                                    {
                                        throw result.Error;
                                    }

                                    lavaEngineOutput = result.Text;
                                }
                                catch ( System.Threading.ThreadAbortException )
                                {
                                    // Ignore abort error caused by Lava PageRedirect filter.
                                }
                                catch ( Exception ex )
                                {
                                    // Log the exception and continue, because the final render will be performed by RockLiquid.
                                    ExceptionLogService.LogException( new LavaException( "Lava Verification Error: Parse template failed.", ex ), System.Web.HttpContext.Current );
                                    addCachePlaceholder = true;
                                }
                            }
                            else
                            {
                                addCachePlaceholder = true;
                            }

                            if ( addCachePlaceholder )
                            {
                                if ( engine.TemplateCacheService != null )
                                {
                                    // Add a placeholder in the cache to prevent this template from being recompiled.
                                    var emptyTemplate = engine.ParseTemplate( string.Empty ).Template;
                                    engine.TemplateCacheService.AddTemplate( emptyTemplate, cacheKey );
                                }
                            }
                        }
                    }

#pragma warning disable CS0618 // Type or member is obsolete
                    rockLiquidOutput = ResolveMergeFieldsForRockLiquid( content, mergeObjects, currentPersonOverride, enabledLavaCommands, encodeStrings, throwExceptionOnErrors );
#pragma warning restore CS0618 // Type or member is obsolete

                    if ( lavaEngineOutput != null )
                    {
                        // Compare output to check for a possible mismatch, ignoring whitespace.
                        // This is a simplified content check, but we can't require an exact match because output may contain unique Guid and DateTime values.
                        var compareRockLiquid = _regexRemoveWhitespace.Replace( rockLiquidOutput, string.Empty );
                        var compareLavaEngine = _regexRemoveWhitespace.Replace( lavaEngineOutput, string.Empty );

                        if ( compareLavaEngine.Length != compareRockLiquid.Length )
                        {
                            // The LavaEngine has produced different output from RockLiquid.
                            // Log the exception, with a simple top-level exception containing a warning message.
                            var renderException = new LavaRenderException( LavaService.CurrentEngineName, content, $"Lava engine render output is unexpected.\n[Expected={rockLiquidOutput},\nActual={lavaEngineOutput}]" );

                            ExceptionLogService.LogException( new LavaException( "Lava Verification Warning: Render output mismatch.", renderException ), System.Web.HttpContext.Current );
                        }
                    }

                    return rockLiquidOutput;
                }
                else
                {
                    var result = ResolveMergeFieldsWithCurrentLavaEngine( content, mergeObjects, currentPersonOverride, enabledLavaCommands, encodeStrings );

                    return result.Text;
                }
            }
            catch ( System.Threading.ThreadAbortException )
            {
                // Ignore abort errors that may be caused by previous implementations of the Lava PageRedirect filter.
                return string.Empty;
            }
            catch ( Exception ex )
            {
                if ( throwExceptionOnErrors )
                {
                    throw;
                }
                else
                {
                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                    return "Error resolving Lava merge fields: " + ex.Message + "\n[Engine: DotLiquid]";
                }
            }
        }

        [RockObsolete( "1.13" )]
        [Obsolete( "This method is only required for the DotLiquid Lava implementation." )]
        private static string ResolveMergeFieldsForRockLiquid( this string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride, string enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            var template = GetTemplate( content );

            string result;
            List<Exception> errors;

            var renderParameters = new RenderParameters();
            renderParameters.LocalVariables = Hash.FromDictionary( mergeObjects );
            renderParameters.Registers = new Hash();

            if ( string.IsNullOrWhiteSpace( enabledLavaCommands ) )
            {
                if ( template.Registers.ContainsKey( "EnabledCommands" ) )
                {
                    renderParameters.Registers.AddOrReplace( "EnabledCommands", template.Registers["EnabledCommands"].ToStringSafe() );
                }
            }
            else
            {
                renderParameters.Registers.AddOrReplace( "EnabledCommands", enabledLavaCommands );
            }
            if ( currentPersonOverride != null )
            {
                renderParameters.Registers.AddOrReplace( "CurrentPerson", currentPersonOverride );
            }

            if ( encodeStrings )
            {
                renderParameters.ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
                renderParameters.ValueTypeTransformers[typeof( string )] = EncodeStringTransformer;
            }

            using ( TextWriter writer = new StringWriter() )
            {
                template.Render( writer, renderParameters, out errors );
                result = writer.ToString();
            }

            if ( throwExceptionOnErrors && errors.Any() )
            {
                if ( errors.Count == 1 )
                {
                    throw errors[0];
                }
                else
                {
                    throw new AggregateException( errors );
                }
            }

            return result;
        }

        /// <summary>
        /// Use Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <param name="enabledLavaCommands">A comma-delimited list of the lava commands that are enabled for this template.</param>
        /// <param name="encodeStringOutput">if set to <c>true</c> [encode string output].</param>
        /// <returns>If lava present returns merged string, if no lava returns original string, if null returns empty string</returns>
        private static LavaRenderResult ResolveMergeFieldsWithCurrentLavaEngine( string content, IDictionary<string, object> mergeObjects, Person currentPersonOverride, string enabledLavaCommands, bool encodeStringOutput )
        {
            // If there have not been any EnabledLavaCommands explicitly set, then use the global defaults.
            if ( enabledLavaCommands == null )
            {
                enabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" );
            }

            var context = LavaService.NewRenderContext();

            context.SetEnabledCommands( enabledLavaCommands, "," );

            if ( currentPersonOverride != null )
            {
                context.SetInternalField( "CurrentPerson", currentPersonOverride );
            }

            foreach ( var kvp in mergeObjects )
            {
                if ( kvp.Key.StartsWith( LavaHelper.InternalMergeFieldPrefix ) )
                {
                    context.SetInternalField( kvp.Key.Substring( LavaHelper.InternalMergeFieldPrefix.Length ), kvp.Value );
                }
                else
                {
                    context.SetMergeField( kvp.Key, kvp.Value );
                }
            }

            var parameters = LavaRenderParameters.WithContext( context );
            parameters.ShouldEncodeStringsAsXml = encodeStringOutput;

            var result = LavaService.RenderTemplate( content, parameters );

            if ( result.HasErrors
                 && LavaService.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
            {
                // If the result is an error, encode the error message to prevent any part of it from appearing as rendered content, and then add markup for line breaks.
                result.Text = result.Text.EncodeHtml().ConvertCrLfToHtmlBr();
            }

            return result;
        }

        /// <summary>
        /// HTML Encodes string values that are processed by a lava filter
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private static object EncodeStringTransformer( object s )
        {
            string val = ( s as string );
            if ( !string.IsNullOrEmpty( val ) )
            {
                // we technically want to XML encode, but Html Encode does the trick
                return val.EncodeHtml();
            }
            else
            {
                return s;
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
        /// Compiled RegEx for detecting if a string has Lava merge fields
        /// regex from some ideas in
        ///  http://stackoverflow.com/a/16538131/1755417
        ///  http://stackoverflow.com/a/25776530/1755417
        /// </summary>
        private static Regex hasLavaMergeFields = new Regex( @"(?<=\{).+(?<=\})", RegexOptions.Compiled );

        /// <summary>
        /// Compiled RegEx for detecting if a string has Lava {% %} command fields
        /// </summary>
        private static Regex hasLavaCommandFields = new Regex( @"(?<=\{%).+(?<=%\})", RegexOptions.Compiled );

        /// <summary>
        /// Compiled RegEx for detecting if a string uses the Legacy "GlobalAttribute." syntax
        /// </summary>
        private static Regex hasLegacyGlobalAttributeLavaMergeFields = new Regex( @"(?<=\{).+GlobalAttribute.+(?<=\})", RegexOptions.Compiled );

        /// <summary>
        /// Compiled RegEx for finding Lava tokens that produce unique output or side effects.
        /// </summary>
        private static Regex _nonIdempotentLavaTokens = new Regex( @"{%[^\S\r\n]*((webrequest|workflowactivate)[^\S\r\n]+|(.+\|[^\S\r\n]*PersonTokenCreate)).*%}", RegexOptions.Compiled );

        /// <summary>
        /// Determines whether the string potentially has lava merge fields in it.
        /// NOTE: Might return true even though it doesn't really have merge fields, but something like looks like it. For example '{56408602-5E41-4D66-98C7-BD361CD93AED}'
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        [Obsolete("Use the LavaHelper.IsLavaTemplate method instead.")]
        [RockObsolete("1.13.3")]
        public static bool HasMergeFields( this string content )
        {
            if ( content == null )
            {
                return false;
            }

            // If there are no lava codes, return false
            if ( !hasLavaMergeFields.IsMatch( content ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the string potentially has lava command {% %} fields in it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool HasLavaCommandFields( this string content )
        {
            if ( content == null )
            {
                return false;
            }

            // If there are no lava command fields, return false
            if ( !hasLavaCommandFields.IsMatch( content ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Indicates if the target string contains any elements of a Lava template.
        /// NOTE: This function may return a false positive if the target string contains anything that resembles a Lava element, perhaps contained in a string literal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool IsLavaTemplate( this string content )
        {
            return LavaHelper.IsLavaTemplate( content );
        }

        /// <summary>
        /// Indicates if the target string contains any elements of a Lava template.
        /// This is a much stricter check as it specifically looks for {{...}}, {%...%}
        /// and {[...]}. This should reduce the risk of false positives at the expense
        /// of a slightly longer check time.
        /// </summary>
        /// <param name="content">The content to be checked.</param>
        /// <returns><c>true</c> if the content contains lava tags; otherwise <c>false</c>.</returns>
        internal static bool IsStrictLavaTemplate( this string content )
        {
            return LavaHelper.IsStrictLavaTemplate( content );
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="RockRequestContext"/> from the <see cref="ILavaRenderContext"/>.
        /// </summary>
        /// <param name="renderContext">The <see cref="ILavaRenderContext"/>.</param>
        /// <returns>The <see cref="RockRequestContext"/> or null if not found.</returns>
        internal static RockRequestContext GetRockRequestContext( this ILavaRenderContext renderContext )
        {
            if ( renderContext == null )
            {
                return null;
            }

            var rockRequestContext = renderContext.GetInternalField( "RockRequestContext" ) as RockRequestContext;

            return rockRequestContext;
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

        #region Lava Legacy code

        /// <summary>
        /// Create a parsed Lava template or retrieve it from the cache.
        /// </summary>
        /// <param name="content">The content of the template.</param>
        /// <returns></returns>
        [RockObsolete("1.13")]
        [Obsolete("This method is only required for the DotLiquid Lava implementation.")]
        private static Template GetTemplate( string content )
        {
            const int hashLength = 10;
            string templateKey;

            if ( string.IsNullOrEmpty( content ) )
            {
                /* [2020-08-01] DJL - Cache the null template specifically, but process other whitespace templates individually
                 * to ensure that the format of the final output is preserved.
                 */
                templateKey = string.Empty;
            }
            else if ( content.Length <= hashLength )
            {
                // If the content is less than the size of the MD5 hash,
                // simply use the content as the key to save processing time.
                templateKey = content;
            }
            else
            {
                // Calculate a hash of the content using xxHash.
                templateKey = content.XxHash();
            }

            var template = LavaTemplateCache.Get( templateKey, content ).Template as Template;

            // Clear any previous errors from the template.
            template.Errors.Clear();

            return template;
        }

        #endregion
    }
}
