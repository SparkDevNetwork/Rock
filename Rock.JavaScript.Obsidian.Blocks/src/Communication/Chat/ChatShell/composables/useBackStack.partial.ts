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
// The one owner of the chat's history entries on a phone. History mirrors one number, how many
// steps deep the phone is: the frame's screens over the list, then every sheet, confirm, photo or
// turned page open over them. Each step is one entry on the page's own address, marked with its
// depth beside whatever state the entry carries; the address never changes, so a reload or a
// shared link is the page as it was. A popstate means only "be at this depth": the layers above it
// close newest first, each by its own owner, then the frame steps back the rest. A step closed by
// code (an item chosen, Escape, an on-screen Back) changes the number, and one flush after the
// patch goes back to match, so its popstate closes nothing more. Off a phone it writes nothing, so
// the browser's Back keeps its meaning on a desktop.

import { computed, nextTick, onScopeDispose, shallowReactive, shallowRef, watch } from "vue";
import { useChatSession } from "../chatSession.partial";

/** How a step back was made. */
export type BackStepOptions = {
    /** The browser drew the step itself, as an edge swipe does, so the chat draws it at once. */
    isDrawnByBrowser: boolean;
};

/** The one owner of the chat's history entries on a phone. */
export type BackStack = {
    /**
     * The frame's screens, registered once: how many steps deep they are, and how to step back.
     *
     * @param count How many screen steps are drawn over the list.
     * @param back Steps back by the count given.
     */
    screens: (count: () => number, back: (steps: number, options: BackStepOptions) => void) => void;

    /**
     * A layer opened over the screens, as a sheet or a turned page.
     *
     * @param back Asks the layer's owner to close it.
     *
     * @returns Lets the layer go, once it has closed.
     */
    hold: (back: () => void) => () => void;
};

/** What the back stack reads of the window, a fake for a spec. */
export type BackStackHost = {
    history: Pick<History, "state" | "pushState" | "replaceState" | "go">;
    addEventListener: (type: "popstate", listener: (event: PopStateEvent) => void) => void;
    removeEventListener: (type: "popstate", listener: (event: PopStateEvent) => void) => void;
    setTimeout: (run: () => void, ms: number) => number;
    clearTimeout: (id: number | undefined) => void;
};

/**
 * How long a trip back may wait for its popstate before history is read as it stands: a go past the
 * entries a browser kept brings none. One second, an estimate well past a browser's own delay.
 */
const tripBoundMs = 1000;

/** A layer held over the screens, by identity, so one let go twice goes once. */
type Layer = { back: () => void };

/** The frame's screens, once registered. */
type Screens = { count: () => number, back: (steps: number, options: BackStepOptions) => void };

/** The stack a part drawn alone reads: nothing is held and nothing is written. */
export const noBackStack: BackStack = {
    screens: () => undefined,
    hold: () => () => undefined
};

/**
 * The depth an entry's state marks, or null for an entry that is not the chat's.
 *
 * @param state The entry's state.
 *
 * @returns The depth, or null.
 */
function depthOf(state: unknown): number | null {
    const depth = (state as { rockChat?: { depth?: unknown } } | null)?.rockChat?.depth;
    return typeof depth === "number" ? depth : null;
}

/**
 * Creates the chat's back stack, once, in the shell's setup; its listener goes with that scope.
 *
 * @param isActive Whether the chat is in its phone layout now.
 * @param host The window, or a fake for a spec.
 *
 * @returns The stack.
 */
export function createBackStack(isActive: () => boolean, host: BackStackHost = window): BackStack {
    const layers = shallowReactive<Layer[]>([]);
    const screenSource = shallowRef<Screens | null>(null);

    /** How deep history should be: the screens, then every layer over them; nothing off a phone. */
    const wanted = computed((): number => isActive() ? (screenSource.value?.count() ?? 0) + layers.length : 0);

    /** How deep history is, as last written or read. */
    let depth = 0;

    /** A trip back on its way, which pushes wait for, since browsers order its popstate and a push differently. */
    let tripTimer: number | null = null;

    let isFlushDue = false;

    /**
     * Every history call goes through here. WebKit throws past its rate limit and Chrome's
     * throttle drops a call without a word, so a call that throws reads as not made, and the
     * caller reads the entry back to know.
     */
    function write(call: () => void): boolean {
        try {
            call();
            return true;
        }
        catch {
            return false;
        }
    }

    /** The page's own entry is the list's, marked beside whatever it carries, before the first push. */
    function markBase(): void {
        const state = host.history.state as unknown;
        if (depthOf(state) === null) {
            const kept = typeof state === "object" && state !== null ? state : {};
            write(() => host.history.replaceState({ ...kept, rockChat: { depth: 0 } }, ""));
        }
    }

    function startTrip(steps: number): void {
        if (write(() => host.history.go(-steps))) {
            tripTimer = host.setTimeout(endTrip, tripBoundMs);
        }
    }

    /** A trip that brought no popstate in time ends on history as it stands. */
    function endTrip(): void {
        tripTimer = null;
        depth = depthOf(host.history.state) ?? 0;
        schedule();
    }

    /** Brings history to the wanted depth: a push for each step more, one trip back for any fewer. */
    function flush(): void {
        isFlushDue = false;
        if (tripTimer !== null) {
            return;
        }

        const target = wanted.value;
        if (depth < target) {
            markBase();
        }
        while (depth < target) {
            const next = depth + 1;
            const isWritten = write(() => host.history.pushState({ rockChat: { depth: next } }, ""))
                && depthOf(host.history.state) === next;
            // A refused push keeps the depth, so the next flush tries again.
            if (!isWritten) {
                return;
            }
            depth = next;
        }

        if (depth > target) {
            startTrip(depth - target);
        }
    }

    /** One flush after the patch, however many changes ask before it. */
    function schedule(): void {
        if (!isFlushDue) {
            isFlushDue = true;
            void nextTick(flush);
        }
    }

    function onPopState(event: PopStateEvent): void {
        const at = depthOf(event.state) ?? 0;

        // Our own trip has arrived.
        if (tripTimer !== null) {
            host.clearTimeout(tripTimer);
            tripTimer = null;
            depth = at;
            schedule();
            return;
        }

        const steps = depth - at;
        depth = at;
        if (isActive() && steps > 0) {
            // The layers above the depth close newest first, each by its owner, then the screens.
            const closing = layers.slice(-steps).reverse();
            closing.forEach(layer => layer.back());
            const rest = steps - closing.length;
            const isDrawnByBrowser = (event as PopStateEvent & { hasUAVisualTransition?: boolean }).hasUAVisualTransition === true;
            if (rest > 0) {
                screenSource.value?.back(rest, { isDrawnByBrowser });
            }
        }

        // After the closes' patch: a close refused, or made a tick late, leaves more wanted than
        // history holds and the entry comes back; Forward onto a chat entry goes back again.
        schedule();
    }

    function screens(count: () => number, back: (steps: number, options: BackStepOptions) => void): void {
        const source: Screens = { count, back };
        screenSource.value = source;
        onScopeDispose(() => {
            if (screenSource.value === source) {
                screenSource.value = null;
            }
        });
    }

    function hold(back: () => void): () => void {
        const layer: Layer = { back };
        layers.push(layer);
        return () => {
            const at = layers.indexOf(layer);
            if (at >= 0) {
                layers.splice(at, 1);
            }
        };
    }

    // A page reloaded at a depth from its last life goes back to its base before anything is
    // pushed, so no stale entry stays below the new ones for Forward to bounce off.
    const startDepth = depthOf(host.history.state);
    if (startDepth !== null && startDepth > 0) {
        depth = startDepth;
        startTrip(startDepth);
    }

    watch(wanted, schedule, { flush: "sync" });
    host.addEventListener("popstate", onPopState);
    onScopeDispose(() => {
        host.removeEventListener("popstate", onPopState);
        if (tripTimer !== null) {
            host.clearTimeout(tripTimer);
        }
    });

    return { screens, hold };
}

/**
 * Holds one history entry while a layer is open, released as it closes or as its part leaves the
 * page while open, so no owner can leave the depth high. Called in a part's setup; the stack itself
 * writes nothing off a phone.
 *
 * @param isOpen Whether the layer is open.
 * @param back Asks the layer's owner to close it, for the system Back.
 */
export function useBackLayer(isOpen: () => boolean, back: () => void): void {
    const stack = useChatSession().backStack;
    let release: (() => void) | null = null;

    watch(isOpen, open => {
        if (open && !release) {
            release = stack.hold(back);
        }
        else if (!open && release) {
            release();
            release = null;
        }
    }, { immediate: true, flush: "sync" });

    onScopeDispose(() => {
        release?.();
        release = null;
    });
}
