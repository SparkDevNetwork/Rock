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

/** The strings that can be emitted by gateway components. */
export const GatewayEmitStrings = {
    /** Indicates a successful submission, value is a string. */
    Success: "success",

    /** Indicates one or more validation errors, value is Record<string, string> */
    Validation: "validation",

    /** A serious error occurred that prevents the gateway from functioning. */
    Error: "error"
} as const;

/** The strings that can be emitted by gateway components. */
export type GatewayEmitStrings = typeof GatewayEmitStrings[keyof typeof GatewayEmitStrings];

