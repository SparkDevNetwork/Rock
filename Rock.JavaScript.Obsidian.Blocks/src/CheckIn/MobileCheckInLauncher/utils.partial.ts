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

import { PositionResult } from "./types.partial";

/**
 * The browser waits forever by default, which leaves the individual with no
 * way forward.
 */
const positionTimeoutMs = 30000;

/**
 * Gets the browser's current position, prompting the individual for permission
 * if they have not been asked yet.
 *
 * @returns The position, or the reason the browser would not report one.
 */
export function getCurrentPosition(): Promise<PositionResult> {
    return new Promise<PositionResult>(resolve => {
        if (!navigator.geolocation) {
            resolve({ position: null, isPermissionDenied: false });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => resolve({ position, isPermissionDenied: false }),
            error => resolve({ position: null, isPermissionDenied: error.code === error.PERMISSION_DENIED }),
            {
                enableHighAccuracy: true,
                timeout: positionTimeoutMs
            });
    });
}
