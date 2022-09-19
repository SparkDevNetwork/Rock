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
using System.Collections.Generic;

namespace Rock.Model
{
    #region Map Helper Classes

    /// <summary>
    /// 
    /// </summary>
    public class MapItem
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the point.
        /// </summary>
        /// <value>
        /// The point.
        /// </value>
        public MapCoordinate Point { get; set; }

        /// <summary>
        /// Gets or sets the polygon points.
        /// </summary>
        /// <value>
        /// The polygon points.
        /// </value>
        public List<MapCoordinate> PolygonPoints { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItem"/> class.
        /// </summary>
        public MapItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItem"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public MapItem( Location location )
        {
            PolygonPoints = new List<MapCoordinate>();

            if ( location != null )
            {
                LocationId = location.Id;
                if ( location.GeoPoint != null )
                {
                    Point = new MapCoordinate( location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                }

                if ( location.GeoFence != null )
                {
                    PolygonPoints = location.GeoFence.Coordinates();
                }
            }
        }
    }

    #endregion Map Helper Classes
}
