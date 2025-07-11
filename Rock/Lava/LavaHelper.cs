﻿// <copyright>
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI;

using UAParser;

namespace Rock.Lava
{
    /// <summary>
    ///
    /// </summary>
    public static class LavaHelper
    {
        /// <summary>
        /// The merge field prefix for internal merge fields. These merge fields
        /// will be marked as internal so they are not available to Lava template
        /// itself but can be used by filters and such.
        /// </summary>
        internal static readonly string InternalMergeFieldPrefix = "$_";

        /// <summary>
        /// This is used by <see cref="IsLavaProperty(PropertyInfo)"/> method
        /// to cache information calculated about a property. Since there is really
        /// no sane way for an existing type to have it's attributes modified
        /// at runtime, it is safe to cache this data and provides a 90% boost
        /// to the performance of accessing entity properties.
        /// </summary>
        private static readonly ConcurrentDictionary<PropertyInfo, bool> _isLavaPropertyCache = new ConcurrentDictionary<PropertyInfo, bool>();

        #region Constructors

        static LavaHelper()
        {
            InitializeLavaCommentsRegex();
        }

        #endregion

        /// <summary>
        /// Gets the current data context from the specified lava context or returns a new data context if either context does not exist.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static RockContext GetRockContextFromLavaContext( ILavaRenderContext context )
        {
            var rockContext = context?.GetInternalField( "rock_context", null ) as RockContext;

            if ( rockContext == null )
            {
                rockContext = new RockContext();
                if ( context != null )
                {
                    context.SetInternalField( "rock_context", rockContext );
                }
            }

            return rockContext;
        }

        /// <summary>
        /// Gets the common merge fields for Lava operations. By default it'll include CurrentPerson, Context, PageParameter, and Campuses
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetCommonMergeFields( RockPage rockPage, Person currentPerson = null, CommonMergeFieldsOptions options = null )
        {
            /*
                6/10/2024 - DSH

                If you make any changes here to add or remove common merge fields,
                you need to make the same changes in RockRequestContext.
            */
            var mergeFields = new Dictionary<string, object>();

            if ( rockPage == null && HttpContext.Current != null )
            {
                rockPage = HttpContext.Current.Handler as RockPage;
            }

            if ( options == null )
            {
                options = new CommonMergeFieldsOptions();
            }

            if ( rockPage == null )
            {
                var rockRequestContext = RockRequestContextAccessor.Current;
                if ( rockRequestContext != null )
                {
                    return rockRequestContext.GetCommonMergeFields( currentPerson, options );
                }
            }

            if ( currentPerson == null )
            {
                if ( rockPage != null )
                {
                    currentPerson = rockPage.CurrentPerson;
                }
                else if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                }
            }

            if ( options.GetPageContext && rockPage != null )
            {
                var contextObjects = rockPage.GetContextEntities();

                if ( contextObjects.Any() )
                {
                    mergeFields.Add( "Context", contextObjects );
                }
            }

            HttpRequest request = null;
            try
            {
                if ( rockPage != null )
                {
                    request = rockPage.Request;
                }
                else if ( HttpContext.Current != null )
                {
                    request = HttpContext.Current.Request;
                }
            }
            catch
            {
                // intentionally ignore exception (.Request will throw an exception instead of simply returning null if it isn't available)
            }

            if ( options.GetPageParameters && rockPage != null && request != null )
            {
                mergeFields.Add( "PageParameter", rockPage.PageParameters() );
            }

            if ( options.GetOSFamily || options.GetDeviceFamily )
            {
                if ( request != null && !string.IsNullOrEmpty( request.UserAgent ) )
                {
                    Parser uaParser = Parser.GetDefault();
                    ClientInfo client = uaParser.Parse( request.UserAgent );
                    if ( options.GetOSFamily )
                    {
                        mergeFields.Add( "OSFamily", client.OS.Family.ToLower() );
                    }

                    if ( options.GetDeviceFamily )
                    {
                        mergeFields.Add( "DeviceFamily", client.Device.Family );
                    }
                }
            }

            if ( options.GetCurrentPerson )
            {
                if ( currentPerson != null )
                {
                    mergeFields.Add( "CurrentPerson", currentPerson );
                }
            }

            if ( options.GetCurrentVisitor && rockPage != null )
            {
                var currentVisitor = rockPage.CurrentVisitor ?? rockPage.CurrentPersonAlias;
                mergeFields.Add( "CurrentVisitor", currentVisitor );
            }

            if ( options.GetCampuses )
            {
                mergeFields.Add( "Campuses", CampusCache.All() );
            }

            // Add client information 
            if ( rockPage != null )
            {
                mergeFields.Add( "Geolocation", rockPage.RequestContext?.ClientInformation?.Geolocation );
            }

            mergeFields.Add( "IsChatEnabled", ChatHelper.IsChatEnabled );

            return mergeFields;
        }

        /// <summary>
        /// Gets a list of custom lava commands.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLavaCommands()
        {
            var lavaCommands = new List<string>();

            try
            {
                var commandTypes = Rock.Reflection.FindTypes( typeof( Rock.Lava.ILavaSecured ) );

                foreach ( var kvp in commandTypes )
                {
                    var component = Activator.CreateInstance( kvp.Value ) as ILavaSecured;

                    lavaCommands.Add( component.RequiredPermissionKey );
                }

                lavaCommands.Sort();
            }
            catch { }

            return lavaCommands;
        }

        /// <summary>
        /// Determines whether the property is available to Lava
        /// </summary>
        /// <param name="propInfo">The property information.</param>
        /// <returns>
        ///   <c>true</c> if [is lava property] [the specified property information]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLavaProperty( PropertyInfo propInfo )
        {
            return _isLavaPropertyCache.GetOrAdd( propInfo, pi =>
            {
                // If property has a [LavaHidden] attribute return false
                if ( pi.GetCustomAttributes( typeof( LavaHiddenAttribute ) ).Count() > 0 )
                {
                    return false;
                }

                // If property has a [LavaVisible] attribute return true
                if ( pi.GetCustomAttributes( typeof( LavaVisibleAttribute ) ).Count() > 0 )
                {
                    return true;
                }

                // If property has a [DataMember] attribute return true
                if ( pi.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
                {
                    return true;
                }

#pragma warning disable CS0618 // Type or member is obsolete
                if ( pi.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Count() > 0 )
                {
                    return false;
                }
                if ( pi.GetCustomAttributes( typeof( LavaIncludeAttribute ) ).Count() > 0 )
                {
                    return true;
                }
#pragma warning restore CS0618 // Type or member is obsolete

                // otherwise return false
                return false;
            } );
        }

        /// <summary>
        /// Gets the list of properties that are available to Lava
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetLavaProperties( Type type )
        {
            return type.GetProperties().Where( p => IsLavaProperty( p ) ).ToList();
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The current person or null if not found.</returns>
        /// <exception cref="ArgumentNullException">context</exception>
        public static Person GetCurrentPerson( ILavaRenderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            string currentPersonKey = "CurrentPerson";
            Person currentPerson = null;

            // First, check for a person override value included in the lava context.
            currentPerson = context.GetMergeField( currentPersonKey, null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( currentPersonKey ) )
                {
                    currentPerson = httpContext.Items[currentPersonKey] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Gets the primary person alias identifier for the provided person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>The person's primary alias identifier or null if not found.</returns>
        public static int? GetPrimaryPersonAliasId( Person person )
        {
            if ( person == null )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                return new PersonAliasService( rockContext ).GetPrimaryAliasId( person.Guid );
            }
        }

        /// <summary>
        /// Gets a Person object from a Lava input parameter containing a person reference.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Person GetPersonFromInputParameter( object input, ILavaRenderContext context )
        {
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Parse the input object for a Person.
            if ( input is Person p )
            {
                return p;
            }
            else if ( input is PersonAlias pa )
            {
                return pa?.Person;
            }
            else if ( input is string s )
            {
                var inputAsGuid = s.AsGuidOrNull();
                if ( inputAsGuid != null )
                {
                    // If the input is a Guid, retrieve the corresponding Person.
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( inputAsGuid.Value );
                    return person;
                }

                var inputAsInt = s.AsIntegerOrNull();
                if ( inputAsInt != null )
                {
                    // If the input is an integer, retrieve the corresponding Person.
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( inputAsInt.Value );
                    return person;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a DataView object from a Lava input parameter containing a Data View reference.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        [RockObsolete("1.17")]
        [Obsolete( "Use GetDataViewDefinitionFromInputParameter( object ) instead." )]
        public static DataView GetDataViewFromInputParameter( object input, RockContext rockContext )
        {
            DataView dataView = null;

            // Parse the input object for a dataView.
            if ( input is DataView dv )
            {
                dataView = dv;
            }
            else if ( input is string s )
            {
                var dataViewService = new DataViewService( rockContext );

                var inputAsGuid = s.AsGuidOrNull();
                if ( inputAsGuid != null )
                {
                    // If the input is a Guid, retrieve the corresponding DataView.
                    dataView = dataViewService.Get( inputAsGuid.Value );
                }
                else
                {
                    var inputAsInt = s.AsIntegerOrNull();
                    if ( inputAsInt != null )
                    {
                        // If the input is an integer, retrieve the corresponding dataView.
                        dataView = dataViewService.Get( inputAsInt.Value );
                    }
                    else
                    {
                        // If the input is a string, retrieve by name.
                        var inputAsString = s.ToStringSafe().Trim();
                        dataView = dataViewService.Queryable()
                            .FirstOrDefault( d => d.Name != null && d.Name.Equals( inputAsString ) );
                    }
                }
            }
            return dataView;
        }

        /// <summary>
        /// Gets a DataView object from a Lava input parameter containing a Data View reference.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static IDataViewDefinition GetDataViewDefinitionFromInputParameter( object input, RockContext rockContext )
        {
            IDataViewDefinition dataView = null;

            // Parse the input object for a dataView.
            if ( input is IDataViewDefinition dv )
            {
                dataView = dv;
            }
            else if ( input is string s )
            {
                var inputAsGuid = s.AsGuidOrNull();
                if ( inputAsGuid != null )
                {
                    // If the input is a Guid, retrieve the corresponding DataView.
                    dataView = DataViewCache.Get( inputAsGuid.Value );
                }
                else
                {
                    var inputAsInt = s.AsIntegerOrNull();
                    if ( inputAsInt != null )
                    {
                        // If the input is an integer, retrieve the corresponding dataView.
                        dataView = DataViewCache.Get( inputAsInt.Value );
                    }
                    else
                    {
                        // If the input is a string, retrieve by name.
                        var dataViewService = new DataViewService( rockContext );

                        var inputAsString = s.ToStringSafe().Trim();
                        var dataViewId = dataViewService.Queryable()
                            .Where( d => d.Name != null && d.Name.Equals( inputAsString ) )
                            .Select( d => d.Id )
                            .FirstOrDefault();

                        dataView = DataViewCache.Get( dataViewId );
                    }
                }
            }
            return dataView;
        }

        /// <summary>
        /// Gets a PersonAlias representing the current visitor for whom a request is being processed.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <returns></returns>
        public static PersonAlias GetCurrentVisitorInContext( ILavaRenderContext context )
        {
            // If an override value is available in the Lava context, use it.
            var currentVisitor = context.GetMergeField( "CurrentVisitor", null ) as PersonAlias;

            // ... or try to get a value from the current HttpRequest.
            if ( currentVisitor == null )
            {
                var httpContext = System.Web.HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentVisitor" ) )
                {
                    currentVisitor = httpContext.Items["CurrentVisitor"] as PersonAlias;
                }
            }

            // ... or use the primary alias of the current person.
            if ( currentVisitor == null )
            {
                var person = GetCurrentPerson( context );
                currentVisitor = person?.PrimaryAlias;
            }

            return currentVisitor;
        }

        /// <summary>
        /// Parses the Lava Command markup, first resolving merge fields and then harvesting any provided parameters.
        /// </summary>
        /// <param name="markup">The Lava Command markup.</param>
        /// <param name="context">The DotLiquid context.</param>
        /// <param name="parms">
        /// A dictionary into which any parameters discovered within the <paramref name="markup"/> will be added or replaced.
        /// Default values may be pre-loaded into this collection, and will be overwritten if a matching key is present within the <paramref name="markup"/>.
        /// Note that parameter keys should be added in lower case.
        /// <para>
        /// When searching the <paramref name="markup"/> for key/value parameter pairs, the following <see cref="Regex"/> pattern will be used: @"\S+:('[^']+'|\d+)".
        /// This means that the following patterns will be matched: "key:'value'" OR "key:integer". While this should work for most - if not all - Lava Command parameters,
        /// you can always choose to not use this helper method and instead roll your own implementation.
        /// </para>
        /// </param>
        public static void ParseCommandMarkup( string markup, ILavaRenderContext context, Dictionary<string, string> parms )
        {
            if ( markup == null )
            {
                return;
            }

            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            if ( parms == null )
            {
                throw new ArgumentNullException( nameof( parms ) );
            }

            var mergeFields = context.GetMergeFields();

            // Resolve merge fields.
            var resolvedMarkup = markup.ResolveMergeFields( mergeFields );

            // Harvest parameters.
            var markupParms = Regex.Matches( resolvedMarkup, @"\S+:('[^']+'|\d+)" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var parm in markupParms )
            {
                var itemParts = parm.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    var key = itemParts[0].Trim().ToLower();
                    var value = itemParts[1].Trim();

                    if ( value[0] == '\'' )
                    {
                        // key:'value'
                        parms.AddOrReplace( key, value.Substring( 1, value.Length - 2 ) );
                    }
                    else
                    {
                        // key:integer
                        parms.AddOrReplace( key, value );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified command is authorized within the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        ///   <c>true</c> if the specified command is authorized; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAuthorized( ILavaRenderContext context, string command )
        {
            return LavaSecurityHelper.IsAuthorized( context, command );
        }

        /// <summary>
        /// Returns a flag indicating if the target object is capable of being used as a data source in a Lava template.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsLavaDataObject( object obj )
        {
            if ( obj == null )
            {
                return false;
            }

            if ( obj is ILavaDataDictionary || obj is ILavaDataDictionarySource )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts an object that came from JavaScript into an object that
        /// can be used with Lava. Because <paramref name="input"/> is an unknown
        /// we can't just pass it to Lava otherwise it will deny access to
        /// the object values. This also converts any camelCase keys into PascalCase.
        /// </summary>
        /// <param name="input">The input object to be converted.</param>
        /// <returns>A new object that represents <paramref name="input"/>.</returns>
        internal static object JavaScriptObjectToLavaObject( object input )
        {
            if ( input == null )
            {
                return null;
            }

            // Even though we expect the input parameter to be a JToken, we don't
            // know that for sure. So if it isn't then serialize and deserialize it
            // so it is forced into a known format.
            if ( !( input is JToken rootToken ) )
            {
                rootToken = JsonConvert.DeserializeObject<JToken>( input.ToJson() );
            }

            return JavaScriptObjectToLavaObjectInternal( rootToken );
        }

        /// <summary>
        /// Internal converter to take an object that came from JavaScript and
        /// coerce it into something that can be used in Lava.
        /// </summary>
        /// <param name="input">The input object to be converted.</param>
        /// <returns>A new object that represents the <paramref name="input"/>.</returns>
        private static object JavaScriptObjectToLavaObjectInternal( JToken input )
        {
            switch ( input )
            {
                case JObject jObject:
                    var dict = new Dictionary<string, object>();
                    foreach ( var item in jObject )
                    {
                        // Force the key to be uppercase since JavaScript probably
                        // gave us a camelCase string.
                        var key = $"{item.Key.Substring( 0, 1 ).ToUpper()}{item.Key.Substring( 1 )}";
                        var value = JavaScriptObjectToLavaObjectInternal( item.Value );

                        dict.AddOrReplace( key, value );
                    }
                    return dict;

                case JArray jArray:
                    return jArray.Select( JavaScriptObjectToLavaObjectInternal ).ToList();

                case JValue jValue:
                    return jValue.Value;

                default:
                    return input;
            }
        }

        #region Lava Comments

        private static string LavaTokenBlockCommentStart = @"/-";
        private static string LavaTokenBlockCommentEnd = @"-/";
        private static string LavaTokenLineComment = @"//-";

        private static Regex _lavaCommentMatchGroupsRegex = null;
        private static Regex _lavaLineCommentRegex = null;

        /// <summary>
        /// Build the regular expression that will be used to remove Lava-style comments from the template.
        /// </summary>
        private static void InitializeLavaCommentsRegex()
        {
            const string doubleQuotedString = @"(""[^""]*"")+";
            const string singleQuotedString = @"('[^']*')+";

            var lineCommentElement = LavaTokenLineComment + @"(.*?)\r?\n";
            var blockCommentElement = @"(?<!/)" + Regex.Escape( LavaTokenBlockCommentStart ) + @"(.*?)" + Regex.Escape( LavaTokenBlockCommentEnd ) + @"( *)([\r\n]*)";
            var rawBlock = @"\{%\sraw\s%\}(.*?)\{%\sendraw\s%\}";

            var templateElementMatchGroups = rawBlock + "|" + singleQuotedString + "|" + doubleQuotedString + "|" + blockCommentElement + "|" + lineCommentElement;

            // Create and compile the Regex, because it will be used very frequently.
            _lavaLineCommentRegex = new Regex( lineCommentElement, RegexOptions.Compiled | RegexOptions.Singleline );
            _lavaCommentMatchGroupsRegex = new Regex( templateElementMatchGroups, RegexOptions.Compiled | RegexOptions.Singleline );
        }

        /// <summary>
        /// Remove Lava-style comments from a Lava template.
        /// Lava comments provide a shorthand alternative to the Liquid {% comment %}{% endcomment %} block,
        /// and can can be in one of the following forms:
        ///
        /// /- This Lava block comment style...
        ///    ... can span multiple lines -/
        ///
        /// //- This Lava line comment style can be appended to any single line.
        ///
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <returns></returns>
        public static string RemoveLavaComments( string lavaTemplate )
        {
            if ( string.IsNullOrEmpty( lavaTemplate ) )
            {
                return string.Empty;
            }

            // Remove comments from the lava template text.
            // This is achieved using a RegEx replace operation as follows:
            // 1. Identify and ignore content enclosed in a "{% raw %}" tag.
            // 2. Identify any text enclosed in quotes (single or double).
            //    If the quoted text spans multiple lines and contains a single-line comment, remove the comment.
            // 4. Identify and remove any short-form comments not enclosed in quotes.
            // 5. Leave all other text unchanged.
            var lavaWithoutComments = _lavaCommentMatchGroupsRegex.Replace( lavaTemplate,
                me =>
                {
                    if ( me.Value.StartsWith( LavaTokenLineComment ) )
                    {
                        // If the match is a line comment, retain the end-of-line marker.
                        return Environment.NewLine;
                    }
                    else if ( me.Value.StartsWith( LavaTokenBlockCommentStart ) )
                    {
                        return string.Empty;
                    }
                    else if ( me.Value.StartsWith( "'" ) || me.Value.StartsWith( "\"" ) )
                    {
                        // If the match is a quoted string, remove any single-line comments.
                        // This may cause unexpected behavior in some literal text, but it ensures that
                        // these comments are never unintentionally exposed as output.
                        // (refer https://github.com/SparkDevNetwork/Rock/issues/4975)
                        return _lavaLineCommentRegex.Replace( me.Value, Environment.NewLine );
                    }

                    // If the match group is not a comment, return a literal string.
                    return me.Value;
                } );

            return lavaWithoutComments;
        }

        /// <summary>
        /// Indicates if the target string contains any Lava-specific comment elements.
        /// Liquid {% comment %} tags are not classified as Lava-specific comment syntax, and
        /// comments contained in quoted strings and {% raw %} tags are ignored.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool ContainsLavaComments( string content )
        {
            if ( string.IsNullOrEmpty( content ) )
            {
                return false;
            }

            int searchStartIndex = 0;
            Match match = null;

            while ( match == null || match.Success )
            {
                match = _lavaCommentMatchGroupsRegex.Match( content, searchStartIndex );

                if ( match.Value.StartsWith( LavaTokenBlockCommentStart ) || match.Value.StartsWith( LavaTokenLineComment ) )
                {
                    return true;
                }

                searchStartIndex = match.Index + match.Length;
            }

            return false;
        }

        #endregion

        #region Contains

        /// <summary>
        /// Indicates if the target string contains any elements of a Lava template.
        /// NOTE: This function may return a false positive if the target string contains anything that resembles a Lava element, perhaps contained in a string literal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool IsLavaTemplate( this string content )
        {
            if ( content == null )
            {
                return false;
            }

            // If the input string contains any Lava tags, consider it as a template.
            if ( ContainsLavaTags( content ) )
            {
                return true;
            }

            // If the input string contains any Lava-style comments, consider it as a template.
            if ( ContainsLavaComments( content ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Compiled Regex for detecting if a string has lava tags. This is
        /// a more strict version that should prevent false positives.
        /// </summary>
        private static readonly Regex _hasStrictLavaTags = new Regex( @"{{.*}}|{%.*%}|{\[.*\]}", RegexOptions.Compiled );

        /// <summary>
        /// Indicates if the target string contains any elements of a Lava template.
        /// This is a much stricter check as it specifically looks for {{...}}, {%...%}
        /// and {[...]}. This should reduce the risk of false positives at the expense
        /// of a slightly longer check time.
        /// </summary>
        /// <param name="content">The content to be checked.</param>
        /// <returns><c>true</c> if the content contains lava tags; otherwise <c>false</c>.</returns>
        internal static bool IsStrictLavaTemplate( string content )
        {
            if ( content.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // On a 3KB test string with a single lava merge field at the end, this
            // took 0.002ms on the development machine.
            return _hasStrictLavaTags.IsMatch( content );
        }

        /// <summary>
        /// Compiled RegEx for detecting if a string has Lava tags
        /// regex from some ideas in
        ///  http://stackoverflow.com/a/16538131/1755417
        ///  http://stackoverflow.com/a/25776530/1755417
        /// </summary>
        private static Regex _hasLavaTags = new Regex( @"(?<=\{)[\S\s]+(?<=\})", RegexOptions.Compiled );

        /// <summary>
        /// Determines whether a string potentially contains Lava tags.
        /// NOTE: Might return true even though it doesn't really have merge fields, but something like looks like it. For example '{56408602-5E41-4D66-98C7-BD361CD93AED}'
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool ContainsLavaTags( this string content )
        {
            if ( content == null )
            {
                return false;
            }

            if ( !_hasLavaTags.IsMatch( content ) )
            {
                return false;
            }

            return true;
        }

        #endregion

        #region RockLiquid Lava Code

        /// <summary>
        /// Determines whether the specified command is authorized within the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        ///   <c>true</c> if the specified command is authorized; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static bool IsAuthorized( DotLiquid.Context context, string command )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The current person or null if not found.</returns>
        /// <exception cref="ArgumentNullException">context</exception>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static Person GetCurrentPerson( DotLiquid.Context context )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        /// <summary>
        /// Parses the Lava Command markup, first resolving merge fields and then harvesting any provided parameters.
        /// </summary>
        /// <param name="markup">The Lava Command markup.</param>
        /// <param name="context">The DotLiquid context.</param>
        /// <param name="parms">
        /// A dictionary into which any parameters discovered within the <paramref name="markup"/> will be added or replaced.
        /// Default values may be pre-loaded into this collection, and will be overwritten if a matching key is present within the <paramref name="markup"/>.
        /// Note that parameter keys should be added in lower case.
        /// <para>
        /// When searching the <paramref name="markup"/> for key/value parameter pairs, the following <see cref="Regex"/> pattern will be used: @"\S+:('[^']+'|\d+)".
        /// This means that the following patterns will be matched: "key:'value'" OR "key:integer". While this should work for most - if not all - Lava Command parameters,
        /// you can always choose to not use this helper method and instead roll your own implementation.
        /// </para>
        /// </param>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static void ParseCommandMarkup( string markup, DotLiquid.Context context, Dictionary<string, string> parms )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        /// <summary>
        /// Parse the provided Lava template using the current Lava engine, and write any errors to the exception log.
        /// </summary>
        /// <param name="content"></param>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static void VerifyParseTemplateForCurrentEngine( string content )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        /// <summary>
        /// Wrap an existing Exception if it is not a LavaException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static LavaException ConvertToLavaException( Exception ex )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        /// <summary>
        /// Create a DotLiquid Template object from a string.
        /// </summary>
        /// <param name="templateString"></param>
        /// <returns></returns>
        [Obsolete( "This method was for DotLiquid which is no longer supported." )]
        [RockObsolete( "18.0" )]
        public static DotLiquid.Template CreateDotLiquidTemplate( string templateString )
        {
            throw new NotSupportedException( "DotLiquid is no longer supported." );
        }

        #endregion
    }
}