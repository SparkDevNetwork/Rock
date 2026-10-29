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

// The one owner of how a photo or a face arrives. An image stays hidden until the browser has
// decoded it, so a half-painted photo is never seen, then fades in; the images drawn together
// reveal together, once all are decoded or a short wait after the first, whichever is sooner; an
// image already in the cache shows at once. The avatar, the group mark, the profile card's photo,
// a mention's photo and a message's photos all draw through here.
//
// On the first load the page's reveal is the one owner of when the first screen appears: until it
// has released, every image waits for it, each one still to decode is reported to it so it can
// wait a short grace for them, and what has decoded by the release shows with it, with no fade of
// its own, since the page's crossfade is the fade. After the release the batch and the image's
// own fade apply as usual.
import { computed, Directive, inject, onBeforeUnmount, Ref, watch } from "vue";
import { tokenDuration } from "./brand.partial";
import { pageRevealKey } from "../pageLoad.partial";

/**
 * The class that keeps an image invisible until it is revealed. The box it sits in is already held
 * at the final size, so hiding the image moves nothing.
 */
export const imageHoldClass = "chat-img-hold";

/** The class that fades a revealed image in; a cached image is revealed without it. */
export const imageInClass = "chat-img-in";

/** How long the first decoded image of a batch waits for the rest when the sheet names no value. */
const fallbackBatchMs = 300;

// #region Types

/** One image the owner is tracking, and where it stands. */
type Member = {
    img: HTMLImageElement;
    owner: Reveal;
    batch: Batch | null;
    /** Decoded (or cached or broken) and ready to show, but the batch or the page has not let it through. */
    isReady: boolean;
    /** Shown without a fade, because it was already in the cache. */
    isCached: boolean;
    isRevealed: boolean;
    cleanup: (() => void) | null;
    /** Settles the decode reported to the page; null when none was reported or it has settled. */
    settle: (() => void) | null;
};

/** The images that start loading together: they wait for each other, up to a cap. */
type Batch = {
    members: Set<Member>;
    decoded: Set<Member>;
    timer: ReturnType<typeof setTimeout> | null;
    isFired: boolean;
};

/** What a component holds to have its images revealed. */
export type ImageReveal = {
    /** Hides the image and reveals it when decoded, with the images tracked beside it. */
    track(img: HTMLImageElement): void;

    /** Lets go of an image that left the page, so nothing is written to it. */
    untrack(img: HTMLImageElement): void;

    /** Refuses all further work; the component is gone. */
    stop(): void;
};

// #endregion Types

// #region Values

/**
 * Faces drawn in the same frame share one batch, so a screen of avatars arrives together. It is
 * closed when the frame's work is done, and the next face opens a new one.
 */
let frameBatch: Batch | null = null;

// #endregion Values

// #region Functions

function newBatch(): Batch {
    return { members: new Set(), decoded: new Set(), timer: null, isFired: false };
}

function sharedBatch(): Batch {
    if (!frameBatch) {
        const batch = newBatch();
        frameBatch = batch;

        // A microtask, not a timer: the whole frame's mounting runs in one synchronous pass, and
        // this leaves nothing pending to be torn down.
        void Promise.resolve().then(() => {
            if (frameBatch === batch) {
                frameBatch = null;
            }
        });
    }

    return frameBatch;
}

/**
 * Makes the image tracker for one component. Pass the page's hold signal to keep every image
 * hidden until it turns false.
 *
 * @param options.isShared True for faces, which batch with every other face drawn in the same
 * frame; false for a set of images that belong together, such as one message's photos.
 * @param options.hold The signal to wait on, or null to reveal as soon as images are ready.
 * @param options.reportDecode Called, while the hold is on, with the decode of each image that is
 * not already cached, so the page can wait for it; the promise settles on decode or on breaking.
 */
export function createImageReveal(options: {
    isShared?: boolean,
    hold?: Readonly<Ref<boolean>> | null,
    reportDecode?: ((decoded: Promise<void>) => void) | null
} = {}): ImageReveal {
    const members = new Map<HTMLImageElement, Member>();
    let ownBatch: Batch | null = null;
    let isStopped = false;

    const isHeld = (): boolean => options.hold?.value === true;

    /**
     * Shows one member now if nothing holds it; otherwise it waits for the hold to release.
     *
     * @param hasFade Whether the image fades in; the release of the page's hold shows with no
     * fade, since the page's own crossfade is the fade.
     */
    function show(member: Member, hasFade: boolean = !member.isCached): void {
        if (isStopped || member.isRevealed) {
            return;
        }

        member.isReady = true;

        if (isHeld()) {
            return;
        }

        member.isRevealed = true;
        member.img.classList.remove(imageHoldClass);

        if (hasFade) {
            member.img.classList.add(imageInClass);
        }
    }

    /** The batch's wait is over: everything decoded so far shows together. */
    function fire(batch: Batch): void {
        if (batch.timer !== null) {
            clearTimeout(batch.timer);
            batch.timer = null;
        }

        batch.isFired = true;

        for (const member of batch.decoded) {
            member.owner.showMember(member);
        }
    }

    /** A member left its batch for good: if it was the one the others waited on, they go now. */
    function leave(member: Member): void {
        const batch = member.batch;
        member.batch = null;

        if (!batch) {
            return;
        }

        batch.members.delete(member);
        batch.decoded.delete(member);

        if (batch.members.size === 0 && batch.timer !== null) {
            clearTimeout(batch.timer);
            batch.timer = null;
        }
        else if (!batch.isFired && batch.decoded.size > 0 && batch.decoded.size === batch.members.size) {
            fire(batch);
        }
    }

    /** The member's image is decoded: it waits for its batch, or shows alone if the batch already went. */
    function onDecoded(member: Member): void {
        member.settle?.();

        // While the page holds, a decode is marked at once and never waits on the batch: the
        // release shows every decoded image together, which is the first screen's reveal.
        if (isHeld()) {
            member.isReady = true;
            return;
        }

        const batch = member.batch;

        if (!batch || batch.isFired) {
            show(member);
            return;
        }

        batch.decoded.add(member);

        if (batch.decoded.size === batch.members.size) {
            fire(batch);
        }
        else if (batch.timer === null) {
            batch.timer = setTimeout(() => fire(batch), tokenDuration(member.img, "--chat-d-img-batch", fallbackBatchMs));
        }
    }

    /** A broken image is shown at once, as the browser draws one, and never holds the others. */
    function onBroken(member: Member): void {
        member.settle?.();
        leave(member);
        show(member);
    }

    /** Waits for the browser to have the image ready: decode where it exists, the load event otherwise. */
    function watchLoad(member: Member): void {
        const img = member.img;
        const onLoad = (): void => onDecoded(member);
        const onError = (): void => onBroken(member);
        let isWatching = true;

        const listen = (): void => {
            img.addEventListener("load", onLoad, { once: true });
            img.addEventListener("error", onError, { once: true });
        };

        member.cleanup = (): void => {
            isWatching = false;
            img.removeEventListener("load", onLoad);
            img.removeEventListener("error", onError);
        };

        if (typeof img.decode !== "function") {
            listen();
            return;
        }

        img.decode().then(() => {
            if (isWatching) {
                onDecoded(member);
            }
        }, () => {
            if (!isWatching) {
                return;
            }

            // decode() can reject on an image that is fine, such as one not yet fetched, so the
            // browser's own load and error say which it is.
            if (img.complete) {
                (img.naturalWidth > 0 ? onLoad : onError)();
            }
            else {
                listen();
            }
        });
    }

    const reveal: Reveal = {
        showMember: show,

        track(img: HTMLImageElement): void {
            if (isStopped || members.has(img)) {
                return;
            }

            const member: Member = { img, owner: reveal, batch: null, isReady: false, isCached: false, isRevealed: false, cleanup: null, settle: null };
            members.set(img, member);
            img.classList.add(imageHoldClass);

            // Already fetched and decoded: nothing to wait for, so it shows at once with no fade.
            if (img.complete && img.naturalWidth > 0) {
                member.isCached = true;
                show(member);
                return;
            }

            if (options.isShared) {
                member.batch = sharedBatch();
            }
            else {
                ownBatch ??= newBatch();
                member.batch = ownBatch;
            }

            member.batch.members.add(member);

            // The page waits for what is still decoding, up to its grace.
            if (isHeld() && options.reportDecode) {
                options.reportDecode(new Promise<void>(resolve => {
                    member.settle = (): void => {
                        member.settle = null;
                        resolve();
                    };
                }));
            }

            watchLoad(member);
        },

        untrack(img: HTMLImageElement): void {
            const member = members.get(img);

            if (member) {
                member.settle?.();
                member.cleanup?.();
                leave(member);
                members.delete(img);
            }
        },

        stop(): void {
            isStopped = true;
            stopHold();

            for (const img of [...members.keys()]) {
                reveal.untrack(img);
            }
        }
    };

    // When the page's reveal runs, whatever was ready and waiting shows with it and with no fade of
    // its own, and no longer waits on its batch.
    const stopHold = options.hold
        ? watch(options.hold, held => {
            if (!held) {
                for (const member of members.values()) {
                    if (member.isReady) {
                        leave(member);
                        show(member, false);
                    }
                }
            }
        }, { flush: "sync" })
        : (): void => undefined;

    return reveal;
}

/** The tracker's own surface plus the one call a batch makes on its members' owner. */
type Reveal = ImageReveal & { showMember(member: Member): void };

/**
 * The tracker for a component's setup: it reads the page's hold signal where the shell provides
 * one and stops with the component, so an unmounted component writes nothing.
 *
 * @param options.isShared True for faces, which reveal with every face drawn in the same frame.
 */
export function useImageReveal(options: { isShared?: boolean } = {}): ImageReveal {
    // The page's reveal is read through its getter, so a rebuilt shell is followed.
    const page = inject(pageRevealKey, null);
    const reveal = createImageReveal({
        isShared: options.isShared,
        hold: page ? computed(() => !page.state.isReleased) : null,
        reportDecode: page ? decoded => page.holdFace(decoded) : null
    });

    onBeforeUnmount(() => reveal.stop());

    return reveal;
}

/** `v-reveal="reveal"` on an `<img>`: hides it until decoded, then reveals it with the others. */
export const vReveal: Directive<HTMLImageElement, ImageReveal> = {
    mounted(img, binding): void {
        binding.value?.track(img);
    },

    beforeUnmount(img, binding): void {
        binding.value?.untrack(img);
    }
};

// #endregion Functions
