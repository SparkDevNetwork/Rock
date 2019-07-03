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
using System.Web;

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

            if ( options.GetPageParameters && rockPage != null && request != null)
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
        /// Gets the page properties merge object.
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete("Just use the PageCache of the CurrentPage instead", true )]
        public static Dictionary<string, object> GetPagePropertiesMergeObject( RockPage rockPage )
        {
            Dictionary<string, object> pageProperties = new Dictionary<string, object>();
            pageProperties.Add( "Id", rockPage.PageId.ToString() );
            pageProperties.Add( "BrowserTitle", rockPage.BrowserTitle );
            pageProperties.Add( "PageTitle", rockPage.PageTitle );
            pageProperties.Add( "Site", rockPage.Site.Name );
            pageProperties.Add( "SiteId", rockPage.Site.Id.ToString() );
            pageProperties.Add( "LayoutId", rockPage.Layout.Id.ToString() );
            pageProperties.Add( "Layout", rockPage.Layout.Name );
            pageProperties.Add( "SiteTheme", rockPage.Site.Theme );
            pageProperties.Add( "PageIcon", rockPage.PageIcon );
            pageProperties.Add( "Description", rockPage.MetaDescription );
            return pageProperties;
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
                foreach ( var blockType in Rock.Reflection.FindTypes( typeof( Rock.Lava.Blocks.RockLavaBlockBase ) ).Select( a => a.Value ).ToList() )
                {
                    lavaCommands.Add( blockType.Name );
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
            }
            catch { }

            return lavaCommands;
        }
    }
}