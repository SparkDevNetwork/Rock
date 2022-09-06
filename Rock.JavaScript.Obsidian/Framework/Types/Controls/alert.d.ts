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

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export const enum AlertType {
    Default = "default",
    Success = "success",
    Info = "info",
    Danger = "danger",
    Warning = "warning",
    Primary = "primary",
    Validation = "validation"
}

/** The type that constraints a string to be one of the values of AlertType. */
export type AlertTypeValuesType = `${AlertType.Default}`
    | `${AlertType.Success}`
    | `${AlertType.Info}`
    | `${AlertType.Danger}`
    | `${AlertType.Warning}`
    | `${AlertType.Primary}`
    | `${AlertType.Validation}`;
