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

/* global google */

import { DrawingMode, Coordinate, ILatLng, ILatLngLiteral } from "@Obsidian/Types/Controls/geo";
import { GeoPickerSettingsBag } from "@Obsidian/ViewModels/Rest/Controls/geoPickerSettingsBag";
import { GeoPickerGetSettingsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/geoPickerGetSettingsOptionsBag";
import { GeoPickerGoogleMapSettingsBag } from "@Obsidian/ViewModels/Rest/Controls/geoPickerGoogleMapSettingsBag";
import { emptyGuid } from "./guid";
import { post } from "./http";
import { loadJavaScriptAsync } from "./page";

/**
 * Converts a LatLng object, "lat,lng" coordinate string, or WellKnown "lng lat" coordinate string to a Coordinate array
 * @param coord Either a string in "lat,lng" format or a LatLng object from Google Maps
 * @param isWellKnown True if is "lng lat" format, false if it is "lat, lng"
 *
 * @returns Coordinate: Tuple with a Latitude number and Longitude number as the elements
 */
export function toCoordinate(coord: string | ILatLng, isWellKnown: boolean = false): Coordinate {
    if (typeof coord == "string") {
        // WellKnown string format
        if (isWellKnown) {
            return coord.split(" ").reverse().map(val => parseFloat(val)) as Coordinate;
        }
        // Google Maps URL string format
        else {
            return coord.split(",").map(val => parseFloat(val)) as Coordinate;
        }
    }
    else {
        return [coord.lat(), coord.lng()];
    }
}

/**
 * Takes a Well Known Text value and converts it into a Coordinate array
 */
export function wellKnownToCoordinates(wellKnownText: string, type: DrawingMode): Coordinate[] {
    if (wellKnownText == "") {
        return [];
    }
    if (type == "Point") {
        // From this format: POINT (-112.130946 33.600114)
        return [toCoordinate(wellKnownText.replace(/(POINT *\( *)|( *\) *)/ig, ""), true)];
    }
    else {
        // From this format: POLYGON ((-112.157058 33.598563, -112.092341 33.595132, -112.117061 33.608715, -112.124957 33.609286, -112.157058 33.598563))
        return wellKnownText.replace(/(POLYGON *\(+ *)|( *\)+ *)/ig, "").split(/ *, */).map((coord) => toCoordinate(coord, true));
    }
}

/**
 * Takes a Well Known Text value and converts it into a Coordinate array
 */
export function coordinatesToWellKnown(coordinates: Coordinate[], type: DrawingMode): string {
    if (coordinates.length == 0) {
        return "";
    }
    else if (type == "Point") {
        return `POINT(${coordinates[0].reverse().join(" ")})`;
    }
    else {
        // DB doesn't work well with the points of a polygon specified in clockwise order for some reason
        if (isClockwisePolygon(coordinates)) {
            coordinates.reverse();
        }

        const coordinateString = coordinates.map(coords => coords.reverse().join(" ")).join(", ");
        return `POLYGON((${coordinateString}))`;
    }
}

/**
 * Takes a Coordinate and uses Geocoding to get nearest address
 */
export function nearAddressForCoordinate(coordinate: Coordinate): Promise<string> {
    return new Promise(resolve => {
        // only try if google is loaded
        if (window.google) {
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ location: new google.maps.LatLng(...coordinate) }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK && results?.[0]) {
                    resolve("near " + results[0].formatted_address);
                }
                else {
                    console.log("Geocoder failed due to: " + status);
                    resolve("");
                }
            });
        }
        else {
            resolve("");
        }
    });
}

/**
 * Takes a Coordinate array and uses Geocoding to get nearest address for the first point
 */
export function nearAddressForCoordinates(coordinates: Coordinate[]): Promise<string> {
    if (!coordinates || coordinates.length == 0) {
        return Promise.resolve("");
    }
    return nearAddressForCoordinate(coordinates[0]);
}

/**
 * Determine whether the polygon's coordinates are drawn in clockwise order
 * Thank you dominoc!
 * http://dominoc925.blogspot.com/2012/03/c-code-to-determine-if-polygon-vertices.html
 */
export function isClockwisePolygon(polygon: number[][]): boolean {
    let sum = 0;

    for (let i = 0; i < polygon.length - 1; i++) {
        sum += (Math.abs(polygon[i + 1][0]) - Math.abs(polygon[i][0])) * (Math.abs(polygon[i + 1][1]) + Math.abs(polygon[i][1]));
    }

    return sum > 0;
}

/**
 * Download the necessary resources to run the maps and return the map settings from the API
 *
 * @param options Options for which data to get from the API
 *
 * @return Promise with the map settings retrieved from the API
 */
export async function loadMapResources(options: GeoPickerGetSettingsOptionsBag = { mapStyleValueGuid: emptyGuid }): Promise<GeoPickerSettingsBag> {
    const response = await post<GeoPickerGoogleMapSettingsBag>("/api/v2/Controls/GeoPickerGetGoogleMapSettings", undefined, options);
    const googleMapSettings = response.data ?? {};

    let keyParam = "";

    if (googleMapSettings.googleApiKey) {
        keyParam = `key=${googleMapSettings.googleApiKey}&`;
    }

    await loadJavaScriptAsync(`https://maps.googleapis.com/maps/api/js?${keyParam}libraries=drawing,visualization,geometry`, () => typeof (google) != "undefined" && typeof (google.maps) != "undefined", {}, false);

    return googleMapSettings;
}

/**
 * Creates a ILatLng object
 */
export function createLatLng(latOrLatLngOrLatLngLiteral: number | ILatLngLiteral | ILatLng, lngOrNoClampNoWrap?: number | boolean | null, noClampNoWrap?: boolean): ILatLng {
    return new google.maps.LatLng(latOrLatLngOrLatLngLiteral as number, lngOrNoClampNoWrap, noClampNoWrap);
}