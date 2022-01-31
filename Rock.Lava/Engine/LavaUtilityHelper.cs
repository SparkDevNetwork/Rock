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

namespace Rock.Lava
{
    public static class LavaUtilityHelper
    {
        /// <summary>
        /// Get a valid Liquid document element name from a Rock shortcode.
        /// Applies decorations to the shortcode name to prevent naming collisions with other custom tags and blocks.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static string GetContextKeyFromType( Type objectType )
        {
            if ( objectType == null )
            {
                return null;
            }

            return GetContextKeyFromTypeName( objectType.FullName );
        }

        /// <summary>
        /// Get a valid Liquid document element name from a Rock shortcode.
        /// Applies decorations to the shortcode name to prevent naming collisions with other custom tags and blocks.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static string GetContextKeyFromTypeName( string typeName )
        {
            if ( string.IsNullOrWhiteSpace( typeName ) )
            {
                return null;
            }

            var key = $"type:{typeName.Trim()}";

            return key;
        }

        /// <summary>
        /// Get a valid Liquid document element name from a Rock shortcode.
        /// Applies decorations to the shortcode name to prevent naming collisions with other custom tags and blocks.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static string GetTypeNameFromContextKey( string key )
        {
            if ( key == null )
            {
                return null;
            }

            var typeName = key.Trim();

            if ( typeName.StartsWith("type:") )
            {
                typeName = typeName.Substring( "type:".Length );
            }

            return typeName;
        }

        /// <summary>
        /// Get a valid Liquid document element name from a Rock shortcode.
        /// Applies decorations to the shortcode name to prevent naming collisions with other custom tags and blocks.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static string GetLiquidElementNameFromShortcodeName( string shortcodeName )
        {
            // Note that Liquid names are case-s ensitive, so we need to preserve the casing of the element name.
            var internalName = shortcodeName.Trim() + Constants.ShortcodeInternalNameSuffix;

            return internalName;
        }

        /// <summary>
        /// Get a valid Rock Shortcode name from a Liquid document element name.
        /// Removes the decoration applied to the shortcode name to prevent naming collisions with other custom tags and blocks.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        public static string GetShortcodeNameFromLiquidElementName( string shortcodeName )
        {
            if ( shortcodeName != null
                 && shortcodeName.EndsWith( Constants.ShortcodeInternalNameSuffix ) )
            {
                return shortcodeName.Substring( 0, shortcodeName.Length - Constants.ShortcodeInternalNameSuffix.Length );
            }

            return shortcodeName;
        }
    }
}