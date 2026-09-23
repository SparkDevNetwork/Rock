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
// One place that turns every kind of failure into a stable code and a severity, so the rest of
// the shell branches on codes and never on a vendor's message text.
import { ChatError } from "./types.partial";

/** An error as the platform's API returns it: our code in message, the reason in details. */
export type PlatformErrorLike = {
    message?: string | null;
    code?: string | null;
    details?: string | null;
    status?: number | null;
};

/**
 * Classifies a refusal or failure from a platform call.
 *
 * @param error The error the platform client returned, or what a fetch threw.
 *
 * @returns The code and severity.
 */
export function classifyPlatformError(_error: PlatformErrorLike | unknown): ChatError {
    throw new Error("not implemented");
}

/**
 * Classifies a failed call to one of the block's own actions on the Rock server.
 *
 * @param statusCode The HTTP status the action returned.
 *
 * @returns The code and severity.
 */
export function classifyActionFailure(_statusCode: number): ChatError {
    throw new Error("not implemented");
}

/**
 * Classifies a live channel's status once it is no longer joined.
 *
 * @param status The status the realtime client reported.
 *
 * @returns The code and severity.
 */
export function classifyRealtimeStatus(_status: string): ChatError {
    throw new Error("not implemented");
}
