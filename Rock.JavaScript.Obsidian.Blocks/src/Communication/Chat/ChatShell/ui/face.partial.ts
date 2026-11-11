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
import { startOfDay, timeLabel } from "../feedLayout.partial";
import { ChatPerson, FaceStatus, SidebarRow } from "../types.partial";
import type { AvatarPerson } from "./avatar.partial.obs";

/**
 * A person's name as the chat shows it: their first and last name, whichever they have.
 *
 * @param person The person as the platform sent them.
 *
 * @returns The name, or an empty string for someone with none.
 */
export function personName(person: Pick<ChatPerson, "nick_name" | "last_name">): string {
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

/**
 * A head count in words, "1 person" or "4 people", so the header and the details of a group read
 * the same count alike.
 *
 * @param count The number of people.
 *
 * @returns The count with its noun.
 */
export function peopleCountWords(count: number): string {
    return `${count.toLocaleString()} ${count === 1 ? "person" : "people"}`;
}

/** The photos already asked for on this page, so each is fetched once. */
const preloaded = new Set<string>();

/**
 * Starts fetching and decoding photos a part is about to draw, such as the faces of a card that
 * shows once the pointer has rested, so they draw whole the moment it shows rather than pop in.
 *
 * @param urls The photos; a null or one already asked for is skipped.
 */
export function preloadPhotos(urls: Array<string | null | undefined>): void {
    if (typeof Image === "undefined") {
        return;
    }

    for (const url of urls) {
        if (!url || preloaded.has(url)) {
            continue;
        }

        preloaded.add(url);
        const image = new Image();
        image.src = url;

        // A photo that fails to decode is drawn by the avatar as it would be anyway.
        image.decode?.().catch(() => undefined);
    }
}

// #region Status words

/** A state another person can be drawn in. */
type OthersState = Exclude<FaceStatus["state"], "hidden">;

/**
 * When a Busy ends, on the viewer's own clock: a time the same day, "tomorrow" and a time the next,
 * and a later day by its month and day, so the end reads at a glance without a date for today.
 */
function untilWords(at: Date, now: Date, locale?: string): string {
    // calendar days rather than elapsed hours, rounded so a daylight saving change is not a day
    const days = Math.round((startOfDay(at).getTime() - startOfDay(now).getTime()) / 86_400_000);
    const time = timeLabel(at, locale);

    if (days <= 0) {
        return time;
    }

    if (days === 1) {
        return `tomorrow, ${time}`;
    }

    return `${at.toLocaleDateString(locale, { month: "short", day: "numeric" })}, ${time}`;
}

/**
 * Someone else's status in words: what a screen reader hears after their name, and what the
 * tooltip and the profile card read. One function writes both, so they never disagree.
 *
 * @param state What is drawn.
 * @param busyUntil When a Busy ends, or null for no end and for any other state.
 * @param now The viewer's now.
 * @param locale The viewer's locale, or the browser's when not given.
 *
 * @returns The label, such as ", busy until 3:00 PM", and the tip, such as "Busy until 3:00 PM".
 */
export function statusWords(state: OthersState, busyUntil: Date | null, now: Date, locale?: string): Pick<FaceStatus, "label" | "tip"> {
    const tip = state === "online" ? "Online"
        : state === "away" ? "Away"
            : busyUntil ? `Busy until ${untilWords(busyUntil, now, locale)}` : "Busy";

    // the label continues the name it follows, so its first word is not capitalised
    return { label: `, ${tip.charAt(0).toLowerCase()}${tip.slice(1)}`, tip };
}

/**
 * What one face of a person shows of a status: all of it on a face the host marks as her own, and
 * on any other face what everyone else sees, so her Appear offline draws and says nothing there.
 * The one rule for the badge and the words a face's host puts beside it.
 *
 * @param status What the store answered for the person.
 * @param isOwn Whether the host marks this face as the person's own: her foot, her settings, her card.
 */
export function shownStatus(status: FaceStatus | null, isOwn: boolean): FaceStatus | null {
    return status && (status.state !== "hidden" || isOwn) ? status : null;
}

/**
 * The person's own status in words, for their own face's label, their own card and the Status
 * group: a Busy says what it does to them, since only they act on it.
 *
 * @param state Their state, hidden for Appear offline.
 * @param busyUntil When their Busy ends, or null for no end and for any other state.
 * @param now The viewer's now.
 * @param locale The viewer's locale, or the browser's when not given.
 *
 * @returns The words, such as "Busy, notifications paused until 3:00 PM".
 */
export function ownStatusWords(state: FaceStatus["state"], busyUntil: Date | null, now: Date, locale?: string): string {
    if (state === "busy") {
        return busyUntil ? `Busy, notifications paused until ${untilWords(busyUntil, now, locale)}` : "Busy, notifications paused";
    }

    return state === "online" ? "Online" : state === "away" ? "Away" : "Appear offline";
}

/**
 * The line above the box while the other person in a direct message is Busy. Its name and its end
 * are parts of their own, so a narrow line shortens the name first and then the time, and never
 * "is busy" or "won't be notified", which say what sending does.
 *
 * @param name The name the line opens with.
 * @param busyUntil When her Busy ends, or null for no end.
 * @param now The viewer's now.
 * @param locale The viewer's locale, or the browser's when not given.
 *
 * @returns The name, the end as " until 3:00 PM" or null for none, and the whole sentence.
 */
export function busyNoteWords(name: string, busyUntil: Date | null, now: Date, locale?: string): { until: string | null, name: string, sentence: string } {
    const until = busyUntil ? ` until ${untilWords(busyUntil, now, locale)}` : null;

    return { until, name, sentence: name + ` is busy${until ?? ""} and won't be notified.` };
}

// #endregion Status words
