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

// These types are all related to Google Maps and Geo Picker

/**
 * Type of shape to allow the user to draw on the map
 */
export type DrawingMode = "Polygon" | "Point";

/**
 * A tuple of latitude and longitude representing a single point on a map
 * [ latitude, longitude ]
 */
export type Coordinate = [number, number];

// The following interfaces and classes are based on/copied from Google's Map types to offer some abstraction away from Google Maps

export interface ILatLng {
    /**
     * Returns the latitude in degrees.
     */
    lat(): number;
    /**
     * Returns the longitude in degrees.
     */
    lng(): number;
    /**
     * Returns a string of the form &quot;lat,lng&quot; for this LatLng. We
     * round the lat/lng values to 6 decimal places by default.
     */
    toUrlValue(precision?: number): string;
}

export interface ILatLngLiteral {
    /**
     * Latitude in degrees.
     */
    lat: number;
    /**
     * Longitude in degrees.
     */
    lng: number;
}
