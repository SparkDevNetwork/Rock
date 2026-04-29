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

export const enum NavigationUrlKey {
    ParentPage = "ParentPage",
    ScheduleBuilderPage = "ScheduleBuilderPage"
}

export const enum CheckInType {
    Individual = "0",
    Family = "1"
}

export const CheckInTypeDescription: Record<number, string> = {
    0: "Individual",
    1: "Family"
};

/**
 * Display mode for a registration field. Mirrors the literal string values
 * stored on the core_checkin_registration_Display* GroupType attributes.
 */
export const enum RegistrationFieldDisplay {
    Hide = "Hide",
    Optional = "Optional",
    Required = "Required"
}
