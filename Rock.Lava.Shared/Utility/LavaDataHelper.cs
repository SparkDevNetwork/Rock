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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Rock.Lava
{
    /// <summary>
    /// Defines static helper functions for working with Lava data objects and types.
    /// </summary>
    public static class LavaDataHelper
    {
        private static Type _lavaDataObjectBaseType = typeof( LavaDataObject );

        /// <summary>
        /// Gets the properties of a class that are accessible in a Lava template.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>A collection of visible property names, or null if the type is not marked as [LavaType].</returns>
        public static LavaDataTypeInfo GetLavaTypeInfo( Type type )
        {
            var info = new LavaDataTypeInfo();

            info.IsPrimitiveType = type.IsPrimitive;

            if ( !info.IsPrimitiveType )
            {
                var attr = (LavaTypeAttribute)type.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).FirstOrDefault();

                info.HasLavaTypeAttribute = ( attr != null );

                // Get the list of included properties, then remove the ignored properties.
                List<PropertyInfo> includedProperties;
                if ( attr == null || attr.AllowedMembers == null || !attr.AllowedMembers.Any() )
                {
                    // No included properties have been specified, so assume all are included.
                    // Exclude properties that are defined on the LavaDataObject base type.
                    includedProperties = type.GetProperties()
                        .Where( a => a.DeclaringType != _lavaDataObjectBaseType )
                        .ToList();
                }
                else
                {
                    includedProperties = type.GetProperties()
                    .Where( x => attr.AllowedMembers.Contains( x.Name, StringComparer.OrdinalIgnoreCase ) )
                    .ToList();
                }

                var ignoredProperties = type.GetProperties().Where( x => x.GetCustomAttributes( typeof( LavaHiddenAttribute ), false ).Any() ).ToList();

                info.VisiblePropertyNames = includedProperties.Except( ignoredProperties ).Select( x => x.Name ).ToList();
            }
            else
            {
                info.VisiblePropertyNames = new List<string>();
            }

            return info;
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

        #region Lava Extensions

        /// <summary>
        /// Attempts to retrieve the <see cref="CultureInfo"/> from the <see cref="ILavaRenderContext"/>.
        /// or returns a the current culture if it does not exist.
        /// </summary>
        /// <param name="renderContext">The <see cref="ILavaRenderContext"/>.</param>
        /// <returns>The <see cref="CultureInfo"/> or null if not found.</returns>
        internal static CultureInfo GetCultureInfo( this ILavaRenderContext renderContext )
        {
            CultureInfo cultureInfo;
            if ( renderContext != null && renderContext.GetInternalField( "rock_culture" ) != null )
            {
                cultureInfo = ( CultureInfo ) renderContext.GetInternalField( "rock_culture" );
            }
            else
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }

            return cultureInfo;
        }

        #endregion Lava Extensions

    }
}