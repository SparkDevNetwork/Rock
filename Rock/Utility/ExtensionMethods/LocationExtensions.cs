﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Rock Location and Geography Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
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
    }
}
