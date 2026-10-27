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

// A person as the platform sends one, made into the face the avatar draws. Here rather than in the
// avatar component because a component's setup script exports no functions, and the sidebar, the
// header and the @ picker all draw the platform's people.
import { ChatPerson, SidebarRow } from "../types.partial";
import type { AvatarPerson } from "./avatar.partial.obs";

/**
 * A person's name as the chat shows it: their first and last name, whichever they have.
 *
 * @param person The person as the platform sent them.
 *
 * @returns The name, or an empty string for someone with none.
 */
export function personName(person: ChatPerson): string {
    return [person.nick_name, person.last_name].filter(part => !!part).join(" ");
}

/**
 * A person's face: their photo, and their name for the initials drawn when they have none.
 *
 * @param person The person as the platform sent them.
 *
 * @returns The face the avatar draws.
 */
export function personFace(person: ChatPerson): AvatarPerson {
    return { name: personName(person), photoUrl: person.avatar_url ?? null };
}

/**
 * A direct message's other people as its sidebar row sends them. A platform older than the people
 * sends their names alone, which are drawn as people with no photo.
 *
 * @param row The sidebar row.
 *
 * @returns The people, empty for a room with none.
 */
export function rowPeople(row: Pick<SidebarRow, "dm_people" | "dm_names">): ChatPerson[] {
    return row.dm_people ?? (row.dm_names ?? []).map(name => ({ person_alias_guid: "", nick_name: name, last_name: null, avatar_url: null }));
}
