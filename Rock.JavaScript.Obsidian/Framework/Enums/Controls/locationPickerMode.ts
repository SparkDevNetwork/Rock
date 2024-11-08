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

/** This is used when specifying one of the 4 true LocationPickerModes */
export const SingleLocationPickerMode = {
    Address: 1,
    Named: 2,
    Point: 4,
    Polygon: 8,
} as const;

export type SingleLocationPickerMode = typeof SingleLocationPickerMode[keyof typeof SingleLocationPickerMode];

/** This is mostly to match the webforms version of LocationPickerMode and can be used for comparisons to determine which flags were given. */
export const LocationPickerMode = {
    ...SingleLocationPickerMode,
    None: 0,
    All: 15
} as const;

export type LocationPickerMode = typeof LocationPickerMode[keyof typeof LocationPickerMode];

/** This is used when specifying potentially multiple modes as flags, e.g. when specifying all available modes. */
export type LocationPickerModeFlag = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15;