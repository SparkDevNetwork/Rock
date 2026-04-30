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

export const enum PageParameterKey {
    GroupId = "GroupId",
    OccurrenceId = "OccurrenceId",
    OccurrenceDate = "OccurrenceDate"
}

/**
 * Friendly RSVP status values shared between the attendee grid dropdown and the
 * SaveAttendee request payload. Values must stay in sync with the AttendeeStatus
 * constants in Rock.Blocks/Rsvp/RsvpDetail.cs (the C# block is the source of truth).
 */
export const enum AttendeeStatus {
    Accept = "Accept",
    Decline = "Decline",
    NoResponse = "No Response"
}
