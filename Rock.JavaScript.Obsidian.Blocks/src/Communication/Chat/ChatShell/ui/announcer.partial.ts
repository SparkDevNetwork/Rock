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

// The chat's one voice for a screen reader: what a part says without moving focus, such as how
// many results came back or that a list is up to date. One pair of live regions, polite and
// assertive, in the chat's root, so two parts never talk over each other from regions of their own.
//
// A screen reader listens only inside an open modal dialog, so while one is open the announcer
// speaks from a pair of regions inside it instead; the modal surface registers itself as it opens.

import { inject, InjectionKey, onScopeDispose, provide, Ref, watch } from "vue";
import { SurfaceElement } from "./focusTrap.partial";

/** How urgently a screen reader is to speak: polite waits its turn, assertive interrupts, for a failure. */
export type Politeness = "polite" | "assertive";

/** What a part says through. */
export type Announcer = {
    announce: (text: string, politeness?: Politeness) => void;
};

/** Where an announcement is spoken from: a pair of live regions in one element. */
type Voice = {
    host: HTMLElement;
    regions: Partial<Record<Politeness, HTMLElement>>;
};

/** The announcer's whole state: the root's voice, and the voices of the modal surfaces open on it, newest last. */
type AnnouncerHost = {
    root: () => HTMLElement | null;
    rootVoice: Voice | null;
    modals: Voice[];
};

const announcerKey: InjectionKey<AnnouncerHost> = Symbol("chat-announcer");

/** The announcer a part drawn alone speaks through, from the page's body. */
const pageHost: AnnouncerHost = { root: () => document.body, rootVoice: null, modals: [] };

/** A live region of one politeness, read by a screen reader and never seen. */
function regionIn(voice: Voice, politeness: Politeness): HTMLElement {
    const held = voice.regions[politeness];
    if (held?.isConnected) {
        return held;
    }

    const region = document.createElement("span");
    region.className = "chat-sr-only";
    region.setAttribute("aria-live", politeness);
    region.setAttribute("aria-atomic", "true");
    region.dataset.testid = `announcer-${politeness}`;
    voice.host.appendChild(region);
    voice.regions[politeness] = region;
    return region;
}

/** The voice an announcement is spoken in now: the newest open modal surface's, else the root's. */
function voiceOf(host: AnnouncerHost): Voice | null {
    const modal = host.modals[host.modals.length - 1];
    if (modal) {
        return modal;
    }

    const root = host.root();
    if (!root) {
        return null;
    }

    if (host.rootVoice?.host !== root) {
        host.rootVoice = { host: root, regions: {} };
    }

    return host.rootVoice;
}

function announcerOf(host: AnnouncerHost): Announcer {
    return {
        announce(text: string, politeness: Politeness = "polite"): void {
            const voice = voiceOf(host);
            if (!voice) {
                return;
            }

            const region = regionIn(voice, politeness);

            // The same words again are cleared first and written a moment later, so a screen
            // reader hears a change rather than nothing.
            if (region.textContent === text) {
                region.textContent = "";
                window.setTimeout(() => {
                    region.textContent = text;
                }, 0);
                return;
            }

            region.textContent = text;
        }
    };
}

/**
 * Provides the announcer to every part below, speaking from the chat's root. Called once, by the shell.
 *
 * @param root The chat's root element.
 */
export function provideAnnouncer(root: Ref<HTMLElement | undefined>): void {
    provide(announcerKey, { root: () => root.value ?? null, rootVoice: null, modals: [] });
}

/**
 * The announcer a part speaks through.
 *
 * @returns The announcer.
 */
export function useAnnouncer(): Announcer {
    return announcerOf(inject(announcerKey, pageHost));
}

/**
 * Makes an open modal surface the place announcements are spoken from, for as long as its element
 * is drawn. Called by the modal surfaces themselves: the sheet and the dialog.
 *
 * @param element The modal surface's element, while drawn.
 */
export function useAnnouncerModal(element: SurfaceElement): void {
    const host = inject(announcerKey, pageHost);
    let voice: Voice | null = null;

    function leave(): void {
        if (!voice) {
            return;
        }

        Object.values(voice.regions).forEach(region => region?.remove());
        host.modals.splice(host.modals.indexOf(voice), 1);
        voice = null;
    }

    watch(() => (typeof element === "function" ? element() : element.value) ?? undefined, surface => {
        leave();
        if (surface) {
            voice = { host: surface, regions: {} };
            host.modals.push(voice);

            // made at once, so a region is on the page before the first words arrive in it
            regionIn(voice, "polite");
        }
    }, { immediate: true, flush: "post" });

    onScopeDispose(leave);
}
