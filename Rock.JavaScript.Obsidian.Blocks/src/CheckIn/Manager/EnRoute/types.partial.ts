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

/**
 * Shape of an attendee row in the En Route grid. Matches the field names
 * emitted by the server's GridBuilder.
 */
export type AttendeeRow = {
    personGuid?: string | null;
    attendanceIds?: number[] | null;
    photoImageTag?: string | null;
    nickName?: string | null;
    lastName?: string | null;
    fullName?: string | null;
    parentNames?: string | null;
    groupName?: string | null;
    groupPath?: string | null;
    serviceTimes?: string | null;
    roomName?: string | null;
};
