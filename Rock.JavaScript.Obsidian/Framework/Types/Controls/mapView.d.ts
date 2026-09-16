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

/** A single point plotted on the MapView control. */
export type MapViewMarker = {
    /** A stable identifier the caller uses to correlate the marker with its own data (e.g. a card). */
    id: string;

    /** The latitude the marker is plotted at. */
    latitude: number;

    /** The longitude the marker is plotted at. */
    longitude: number;

    /**
     * The radius, in meters, of a circle drawn around the marker. When set, the marker
     * represents an approximate area rather than a precise point.
     */
    circleRadiusMeters?: number | null;
};
