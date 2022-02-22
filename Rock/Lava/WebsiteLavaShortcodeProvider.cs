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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// Returns the definition of Lava shortcodes stored in the current Rock database.
    /// This implementation uses a website-based data caching model.
    /// </summary>
    public class WebsiteLavaShortcodeProvider
    {
        /// <summary>
        /// Register the specified shortcodes with the Lava Engine.
        /// </summary>
        /// <param name="engine"></param>
        public static void RegisterShortcodes( ILavaEngine engine )
        {
            ClearCache();

            // Register shortcodes defined in the code base.
            try
            {
                var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Select( a => a.Value ).ToList();

                foreach ( var shortcodeType in shortcodeTypes )
                {
                    engine.RegisterShortcode( shortcodeType.Name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                        return shortcode;
                    } );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            // Register shortcodes defined in the current database.
            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                engine.RegisterShortcode( shortcode.TagName, ( shortcodeName ) => GetShortcodeDefinition( shortcodeName ) );
            }
        }

        /// <summary>
        /// Register the specified shortcodes with the Lava Engine.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="shortcodeName"></param>
        public static void RegisterShortcode( ILavaEngine engine, string shortcodeName )
        {
            // Register a factory method that will retrieve the shortcde definition from the data store on demand.
            engine.RegisterShortcode( shortcodeName, ( name ) => GetShortcodeDefinition( name ) );
        }

        /// <summary>
        /// Clears all of the entries from the shortcode cache.
        /// </summary>
        public static void ClearCache()
        {
            LavaShortcodeCache.Clear();
        }

        /// <summary>
        /// Gets a shortcode definition for the specified shortcode name.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static DynamicShortcodeDefinition GetShortcodeDefinition( string shortcodeName )
        {
            DynamicShortcodeDefinition newShortcode = null;

            var shortcodeDefinition = LavaShortcodeCache.All().Where( c => c.TagName != null && c.TagName.Equals( shortcodeName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

            if ( shortcodeDefinition == null )
            {
                return null;
            }

            newShortcode = new DynamicShortcodeDefinition();

            newShortcode.Name = shortcodeDefinition.Name;
            newShortcode.TemplateMarkup = shortcodeDefinition.Markup;

            var parameters = RockSerializableDictionary.FromUriEncodedString( shortcodeDefinition.Parameters );

            newShortcode.Parameters = new Dictionary<string, string>( parameters.Dictionary );

            newShortcode.EnabledLavaCommands = shortcodeDefinition.EnabledLavaCommands.SplitDelimitedValues( "," ).ToList();

            if ( shortcodeDefinition.TagType == TagType.Block )
            {
                newShortcode.ElementType = LavaShortcodeTypeSpecifier.Block;
            }
            else
            {
                newShortcode.ElementType = LavaShortcodeTypeSpecifier.Inline;
            }

            return newShortcode;
        }
    }
}
