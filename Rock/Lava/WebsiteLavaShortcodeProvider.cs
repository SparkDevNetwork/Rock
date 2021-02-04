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
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// Returns the definition of a specified Lava shortcode that is stored in the current Rock database.
    /// This implementation uses a website-based data caching model.
    /// </summary>
    public static class WebsiteLavaShortcodeProvider
    {
        /// <summary>
        /// Gets a shortcode definition for the specified shortcode name.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static DynamicShortcodeDefinition GetShortcodeDefinition( string shortcodeName )
        {
            DynamicShortcodeDefinition newShortcode = null;

            var shortcodeDefinition = LavaShortcodeCache.All().Where( c => c.TagName != null && c.TagName.Equals( shortcodeName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

            if ( shortcodeDefinition != null )
            {
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
            }

            return newShortcode;
        }
    }
}
