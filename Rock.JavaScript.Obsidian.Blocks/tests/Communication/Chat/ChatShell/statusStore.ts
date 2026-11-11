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
// The real status store as every face that draws a status meets it, for the specs of those faces:
// provided under the shell's key, written only as the platform writes it (readers, carried signals),
// with the person's own Busy and Appear offline as her settings hold them, a page that fires nothing
// and a session that never refreshes. A host spec mounts its face with this and asserts only what
// the face draws, never how the store decides it, so no stand-in answers whatever the spec wishes.
/* eslint-disable @typescript-eslint/naming-convention */
import { config } from "@vue/test-utils";
import { readFileSync } from "fs";
import { join } from "path";
import { nextTick, ref, Ref } from "vue";
import { createPresenceStatus, PresenceStatus, presenceStatusKey } from "../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import { readSettings } from "../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { busy, online, readerPerson, serverNowSeconds } from "./statusShapes";

/** The viewer in every host spec, by primary alias. */
export const viewer = "a0000007-0000-4000-8000-000000000000";

export type StatusStore = {
    store: PresenceStatus;

    /** Her own Busy and Appear offline as her settings hold them, which her own faces draw first. */
    mine: Ref<{ busy_until: string | null, appear_offline: boolean } | null>;

    /** Tells the store each person's status, as a reader answers them. */
    tell: (people: Record<string, Record<string, unknown>>) => void;

    /** The church's switch for online status, which the store watches. */
    isOn: Ref<boolean>;

    /** The people the viewer blocked, by lower-case alias. */
    blocked: Set<string>;

    /** What a mount provides, so a part's faces read this store. */
    provide: Record<symbol, unknown>;
};

/**
 * A store with the switch on and her settings read as Automatic.
 *
 * @param options.isSeeded Whether it is told the fixtures' people, Anna Online and Ben Busy with no end in that order and Carl nothing, or, where a spec tells it everyone itself, no one.
 * @param options.clock "fixtures" sets the server's clock at the fixtures' now, for the statuses of statusShapes; "page" keeps the page's clock, for statuses decided now by busyNow and onlineNow.
 */
export function createStatusStore({ isSeeded = true, clock = "fixtures" }: { isSeeded?: boolean, clock?: "fixtures" | "page" } = {}): StatusStore {
    const mine = ref<{ busy_until: string | null, appear_offline: boolean } | null>({ busy_until: null, appear_offline: false });
    const isOn = ref(true);
    const blocked = new Set<string>();
    const page = { visibilityState: "visible", addEventListener: () => undefined, removeEventListener: () => undefined };
    const store = createPresenceStatus({
        settings: () => readSettings({ flags: { online: isOn.value }, limits: { presence_idle_seconds: 600 } }),
        personAliasGuid: viewer,
        me: () => mine.value,
        ownInputAt: () => 0,
        noteInput: () => undefined,
        isBlocked: alias => blocked.has(alias.toLowerCase()),
        isStopped: () => false,
        targets: { document: page, window: page },
        leave: () => undefined,
        session: { refresh: async () => true, nextRefreshAt: () => null, tokenLifeSeconds: () => 300, inFlight: () => null }
    });
    if (clock === "fixtures") {
        store.clockFrom(serverNowSeconds);
    }
    const tell = (people: Record<string, Record<string, unknown>>): void => store.told(Object.entries(people).map(([alias, status]) => readerPerson(alias, status)));
    if (isSeeded) {
        tell({ [anna]: online() });
        tell({ [ben]: busy(null) });
    }

    return { store, mine, tell, isOn, blocked, provide: { [presenceStatusKey as symbol]: store } };
}

/**
 * Provides a store to every mount until the returned function is called, with Vue's real
 * transitions, so a badge's fade or its absence is what a page would draw.
 */
export function provideStatus(status: StatusStore): () => void {
    const provide = config.global.provide;
    const stubs = config.global.stubs;
    config.global.provide = { ...provide, [presenceStatusKey as symbol]: status.store };
    config.global.stubs = { ...stubs, transition: false };
    return () => {
        status.store.stop();
        config.global.provide = provide;
        config.global.stubs = stubs;
    };
}

/**
 * The bodies of the rules a stylesheet gives the selectors matching a pattern, comments dropped:
 * an .obs file's style block, or a whole .css file.
 *
 * @param path The file, from the chat's ChatShell folder or the block's style sheet.
 * @param selector A pattern a rule's selector list must match.
 */
export function styleRules(path: string, selector: RegExp): string[] {
    const text = readFileSync(path, "utf8");
    const style = (path.endsWith(".obs") ? text.slice(text.indexOf(">", text.indexOf("<style")) + 1, text.indexOf("</style>")) : text).replace(/\/\*[\s\S]*?\*\//g, "");
    return Array.from(style.matchAll(/([^{}]+)\{([^{}]*)\}/g))
        .filter(match => match[1].split(",").some(part => selector.test(part.trim().replace(/^\.chat-shell\s+/, ""))))
        .map(match => match[2]);
}

/** Where the shell's parts and the block's style sheet are, for styleRules. */
export const shellFolder = join(__dirname, "../../../../src/Communication/Chat/ChatShell");
export const chatSheet = join(__dirname, "../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css");

/** The people moment 13 swaps between: two told a status, and one the store holds nothing for. */
export const anna = "a0000001-0000-4000-8000-000000000000";
export const ben = "a0000002-0000-4000-8000-000000000000";
export const carl = "a0000003-0000-4000-8000-000000000000";

/**
 * The three swaps of one face from one person to another in place: both with a status, to a
 * person with none, and from a person with none. Anna is told before Ben, so Ben's revision is
 * the later, which a badge kept from Anna would fade for.
 */
export const swaps: ReadonlyArray<[string, string, string]> = [
    ["both with a status", anna, ben],
    ["to a person with none", anna, carl],
    ["from a person with none", carl, anna]
];

/**
 * Every badge in an element, with what it shows and whether anything about it is moving: a
 * crossfade for a change since it was drawn, or a fade in or out. A face swapped to another
 * person is a new face, so after a swap nothing moves.
 */
export function badgesIn(root: Element): Array<{ state: string | null, isMoving: boolean }> {
    return Array.from(root.querySelectorAll("[data-testid='status-badge']")).map(badge => ({
        state: badge.getAttribute("data-state"),
        isMoving: badge.classList.contains("is-changing") || Array.from(badge.classList).some(name => /^chat-status-(enter|leave)/.test(name))
    }));
}

/** Lets a badge's fade end, as a browser would: Vue ends a transition in the frames after it starts. */
export async function afterFades(): Promise<void> {
    await nextTick();
    for (let i = 0; i < 3; i++) {
        await new Promise(resolve => requestAnimationFrame(resolve));
    }
    await nextTick();
}

/**
 * Whether the rules for a face's place set both of the badge's ring variables, so the 2px ring is
 * drawn in what the face sits on rather than in the badge's fallback.
 */
export function setsBothRings(path: string, selector: RegExp): boolean {
    const bodies = styleRules(path, selector);
    return bodies.some(body => /--chat-av-ring:\s*var\(--chat-[a-z-]+\)/.test(body))
        && bodies.some(body => /--chat-av-ring-over:\s*(var\(--chat-[a-z-]+\)|transparent)/.test(body));
}

/**
 * The fill each of a place's states draws, beside the ring-over it sets for a badge's ring: a
 * state whose background changes under a face must ring the badge in that same fill.
 *
 * @returns One pair a state, the ring-over transparent where the fill is none.
 */
export function fillsAndRings(path: string, selector: RegExp): Array<{ fill: string, ring: string | null }> {
    return styleRules(path, selector).map(body => {
        const fill = /(?:^|[;\s])background(?:-color)?:\s*([^;]+);/.exec(body)?.[1].trim() ?? "";
        return { fill: fill === "none" ? "transparent" : fill, ring: /--chat-av-ring-over:\s*([^;]+);/.exec(body)?.[1].trim() ?? null };
    }).filter(state => state.fill !== "");
}

/**
 * A status decided now by the page's clock, as a reader carries it: Busy until an end, or with no
 * end when it is null, its row live for ten minutes.
 *
 * @param end When the Busy ends, on the viewer's clock, or null for none.
 * @param msAfter How long after now it was decided, so a later one is newer.
 */
export function busyNow(end: Date | null, msAfter = 0): Record<string, unknown> {
    const now = Date.now() + msAfter;
    return {
        state: "busy",
        until: new Date(now + 600_000).toISOString(),
        ...(end ? { busy_until: end.toISOString() } : {}),
        as_of: now * 1000
    };
}

/** Online, decided now by the page's clock, its row live for ten minutes. */
export function onlineNow(msAfter = 0): Record<string, unknown> {
    const now = Date.now() + msAfter;
    return { state: "online", until: new Date(now + 600_000).toISOString(), as_of: now * 1000 };
}

/** A reader's person, carrying a status. */
export function told(alias: string, status: Record<string, unknown>, nick = "Cindy"): Record<string, unknown> {
    return readerPerson(alias, status, nick, "Ames");
}
