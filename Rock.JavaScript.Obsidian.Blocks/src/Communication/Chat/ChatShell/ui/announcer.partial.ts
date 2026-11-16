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

// The chat's one voice for a screen reader, so two parts never talk over each other from live
// regions of their own. A screen reader listens only inside an open modal surface, so while one is
// open the voice speaks from inside it.

import { inject, InjectionKey, onScopeDispose, provide, Ref, watch } from "vue";
import { SurfaceElement } from "./focusTrap.partial";

/** Assertive interrupts, for a failure. */
export type Politeness = "polite" | "assertive";

export type Announcer = {
    announce: (text: string, politeness?: Politeness) => void;
};

type Region = {
    element: HTMLElement;

    /**
     * Words given in this task: a screen reader is handed them only once the task ends, so a
     * region taken off the page before then takes them with it unheard.
     */
    unheard: string | null;

    timer?: number;
};

type Voice = {
    host: HTMLElement;
    regions: Partial<Record<Politeness, Region>>;
};

/** The root's voice, and the open modal surfaces' voices, newest last. */
type AnnouncerHost = {
    root: () => HTMLElement | null;
    rootVoice: Voice | null;
    modals: Voice[];
};

const announcerKey: InjectionKey<AnnouncerHost> = Symbol("chat-announcer");

/** For a part drawn alone, with no shell to provide a voice. */
const pageHost: AnnouncerHost = { root: () => document.body, rootVoice: null, modals: [] };

function regionIn(voice: Voice, politeness: Politeness): Region {
    const held = voice.regions[politeness];
    if (held?.element.isConnected) {
        return held;
    }

    const element = document.createElement("span");
    element.className = "chat-sr-only";
    element.setAttribute("aria-live", politeness);
    element.setAttribute("aria-atomic", "true");
    element.dataset.testid = `announcer-${politeness}`;
    voice.host.appendChild(element);
    const region: Region = { element, unheard: null };
    voice.regions[politeness] = region;
    return region;
}

/**
 * The same words again are cleared and written in the next task, so a screen reader hears a change
 * rather than nothing; newer words cancel that step, so a late rewrite never lands over them.
 */
function write(region: Region, text: string): void {
    window.clearTimeout(region.timer);
    region.unheard = text;
    const isRepeat = region.element.textContent === text;
    region.element.textContent = isRepeat ? "" : text;
    region.timer = window.setTimeout(() => {
        if (!isRepeat) {
            region.unheard = null;
            return;
        }

        region.element.textContent = text;
        region.timer = window.setTimeout(() => {
            region.unheard = null;
        }, 0);
    }, 0);
}

// Both regions are made at once: one added together with its first words is often not heard.
function voiceIn(element: HTMLElement): Voice {
    const voice: Voice = { host: element, regions: {} };
    regionIn(voice, "polite");
    regionIn(voice, "assertive");
    return voice;
}

function rootVoiceOf(host: AnnouncerHost): Voice | null {
    const root = host.root();
    if (!root) {
        return null;
    }

    if (host.rootVoice?.host !== root) {
        host.rootVoice = voiceIn(root);
    }

    return host.rootVoice;
}

function voiceOf(host: AnnouncerHost): Voice | null {
    return host.modals[host.modals.length - 1] ?? rootVoiceOf(host);
}

function announcerOf(host: AnnouncerHost): Announcer {
    return {
        announce(text: string, politeness: Politeness = "polite"): void {
            const voice = voiceOf(host);
            if (!voice) {
                return;
            }

            write(regionIn(voice, politeness), text);
        }
    };
}

/**
 * Provides the announcer to every part below. Called once, by the shell.
 *
 * @param root The chat's root element.
 *
 * @returns The same announcer, for the shell's own modules, which a part's inject cannot reach.
 */
export function provideAnnouncer(root: Ref<HTMLElement | undefined>): Announcer {
    const host: AnnouncerHost = { root: () => root.value ?? null, rootVoice: null, modals: [] };
    provide(announcerKey, host);

    watch(root, () => rootVoiceOf(host), { immediate: true, flush: "post" });

    return announcerOf(host);
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
 * Makes a modal surface the place announcements are spoken from while its element is drawn.
 * Called by the sheet and the dialog.
 *
 * @param element The modal surface's element, while drawn.
 */
export function useAnnouncerModal(element: SurfaceElement): void {
    const host = inject(announcerKey, pageHost);
    let voice: Voice | null = null;

    // A choice that closes the surface, as Leave closes the details, is said in the task the
    // surface goes, so its unheard words are said again in the voice beneath; heard ones are not.
    function leave(): void {
        if (!voice) {
            return;
        }

        const left = voice;
        voice = null;
        const at = host.modals.indexOf(left);
        if (at >= 0) {
            host.modals.splice(at, 1);
        }

        const announcer = announcerOf(host);
        (["polite", "assertive"] as const).forEach(politeness => {
            const region = left.regions[politeness];
            if (!region) {
                return;
            }

            window.clearTimeout(region.timer);
            region.element.remove();
            if (region.unheard) {
                announcer.announce(region.unheard, politeness);
            }
        });
    }

    watch(() => (typeof element === "function" ? element() : element.value) ?? undefined, surface => {
        leave();
        if (surface) {
            voice = voiceIn(surface);
            host.modals.push(voice);
        }
    }, { immediate: true, flush: "post" });

    onScopeDispose(leave);
}
