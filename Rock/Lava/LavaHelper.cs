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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

using Rock.Data;
using Rock.Model;
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
        /// Gets the common merge fields for Lava operations. By default it'll include CurrentPerson, Context, PageParameter, and Campuses
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetCommonMergeFields( RockPage rockPage, Person currentPerson = null, CommonMergeFieldsOptions options = null )
        {
            var mergeFields = new Dictionary<string, object>();

            if ( rockPage == null && HttpContext.Current != null )
            {
                rockPage = HttpContext.Current.Handler as RockPage;
            }

            if ( options == null )
            {
                options = new CommonMergeFieldsOptions();
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

            if ( options.GetLegacyGlobalMergeFields )
            {
                var globalAttributes = GlobalAttributesCache.Get();
                if ( globalAttributes.LavaSupportLevel != Lava.LavaSupportLevel.NoLegacy )
                {
                    var legacyGlobalAttributeMergeFields = GlobalAttributesCache.GetLegacyMergeFields( currentPerson );
                    foreach ( var legacyGlobalAttributeMergeField in legacyGlobalAttributeMergeFields )
                    {
                        mergeFields.Add( legacyGlobalAttributeMergeField.Key, legacyGlobalAttributeMergeField.Value );
                    }
                }
            }

            if ( options.GetPageContext && rockPage != null )
            {
                var contextObjects = new Dictionary<string, object>();
                foreach ( var contextEntityType in rockPage.GetContextEntityTypes() )
                {
                    var contextEntity = rockPage.GetCurrentContext( contextEntityType );
                    if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                    {
                        var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                        if ( type != null )
                        {
                            contextObjects.Add( type.Name, contextEntity );
                        }
                    }
                }

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

            if ( options.GetCampuses )
            {
                mergeFields.Add( "Campuses", CampusCache.All() );
            }

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
                /*
                    7/6/2020 - JH
                    Some Lava Commands don't require a closing tag, and therefore inherit from DotLiquid.Tag instead of RockLavaBlockBase.
                    In order to include these self-closing Lava Commands in the returned list, a new interface - IRockLavaBlock - was introduced.
                    We'll also leave the RockLavaBlockBase check in place below, in case any plugins have been developed that add Commands
                    inheriting from the RockLavaBlockBase class.
                */
                foreach ( var blockType in Rock.Reflection.FindTypes( typeof( Rock.Lava.Blocks.IRockLavaBlock ) )
                    .Union( Rock.Reflection.FindTypes( typeof( Rock.Lava.Blocks.RockLavaBlockBase ) ) )
                    .Select( a => a.Value )
                    .OrderBy( a => a.Name )
                    .ToList() )
                {
                    lavaCommands.Add( blockType.Name );
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
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
            // If property has a [LavaIgnore] attribute return false
            if ( propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() > 0 )
            {
                return false;
            }

            // If property has a [DataMember] attribute return true
            if ( propInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
            {
                return true;
            }

            // If property has a [LavaInclude] attribute return true
            if ( propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIncludeAttribute ) ).Count() > 0 )
            {
                return true;
            }

            // otherwise return false
            return false;
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
        public static Person GetCurrentPerson( DotLiquid.Context context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            string currentPersonKey = "CurrentPerson";
            Person currentPerson = null;

            // First, check for a person override value included in the lava context.
            if ( context.Scopes != null )
            {
                foreach ( var scope in context.Scopes )
                {
                    if ( scope.ContainsKey( currentPersonKey ) )
                    {
                        currentPerson = scope[currentPersonKey] as Person;
                    }
                }
            }

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
        public static void ParseCommandMarkup( string markup, DotLiquid.Context context, Dictionary<string, string> parms )
        {
            if ( markup.IsNull() )
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

            var mergeFields = new Dictionary<string, object>();

            // Get variables defined in the lava context.
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    mergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // Get merge fields loaded by the block or container.
            foreach ( var environment in context.Environments )
            {
                foreach ( var item in environment )
                {
                    mergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

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
        public static bool IsAuthorized( DotLiquid.Context context, string command )
        {
            if ( context?.Registers?.ContainsKey( "EnabledCommands" ) == true && command.IsNotNullOrWhiteSpace() )
            {
                var enabledCommands = context.Registers["EnabledCommands"].ToString().Split( ',' ).ToList();

                if ( enabledCommands.Contains( "All" ) || enabledCommands.Contains( command ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}