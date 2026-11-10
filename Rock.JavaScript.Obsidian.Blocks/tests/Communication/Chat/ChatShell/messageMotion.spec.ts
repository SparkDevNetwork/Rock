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
// The record of what happened to each row: a send keeps its first arrival, a view claims each
// arrival once, two writes in one turn wake a row once, what no open view can play is let go, and
// once stopped it records nothing. It keeps no time, so it schedules nothing.
import { nextTick, watch } from "vue";
import { createMessageMotion } from "../../../../src/Communication/Chat/ChatShell/messageMotion.partial";

const mine = { id: 2, client_key: "K1" };
const annas = { id: 3, client_key: null };

describe("the record", () => {
    test("a live copy of a message sent here keeps the send's arrival, so it never rises as heard", () => {
        const motion = createMessageMotion();
        const scope = motion.scope();

        motion.sent("k1");
        motion.arrived(mine);

        expect(motion.of("k:k1")?.arrival?.how).toBe("sent");
        expect(scope.arrives("k:k1")).toBe("rise");
        expect(scope.arrives("k:k1")).toBeNull();
    });

    test("each view claims an arrival once, and a view opened after it claims none", () => {
        const motion = createMessageMotion();
        const feed = motion.scope();
        const drawer = motion.scope();

        motion.arrived(annas);
        const later = motion.scope();

        expect(feed.arrives("k:3")).toBe("rise");
        expect(feed.arrives("k:3")).toBeNull();
        expect(drawer.arrives("k:3")).toBe("rise");
        expect(later.arrives("k:3")).toBeNull();
        expect(feed.arrives("k:4")).toBeNull();
    });

    test("two changes to a row in one turn wake what reads it once", async () => {
        const motion = createMessageMotion();
        const woken = jest.fn();
        watch(() => motion.of("k:3")?.content, woken);

        motion.changed(annas, "message.edited");
        motion.changed(annas, "message.edited");
        await nextTick();

        expect(woken).toHaveBeenCalledTimes(1);
        expect(motion.of("k:3")?.content?.isEdit).toBe(true);
    });

    test("a send's mark is kept until its live copy is heard, then let go as a view goes like any other", () => {
        const motion = createMessageMotion();
        const feed = motion.scope();
        motion.sent("k1");
        motion.sent("k2");
        // the live copy of the first, its key in another case as the platform may write it back
        motion.arrived({ id: 2, client_key: "K1" });

        feed.dispose();

        expect(motion.of("k:k1")).toBeUndefined();
        expect(motion.of("k:k2")?.arrival?.how).toBe("sent");
        motion.arrived({ id: 3, client_key: "k2" });
        expect(motion.of("k:k2")?.arrival?.how).toBe("sent");
        motion.scope().dispose();
        expect(motion.of("k:k2")).toBeUndefined();
    });

    test("a view gone lets go of what no open view could play, and keeps what one could and every send not yet heard", () => {
        const motion = createMessageMotion();
        const old = motion.scope();
        motion.sent("k1");
        motion.arrived(annas);
        const open = motion.scope();
        motion.changed({ id: 4, client_key: null }, "message.deleted");

        old.dispose();

        expect(motion.of("k:k1")).toBeDefined();
        expect(motion.of("k:3")).toBeUndefined();
        expect(motion.of("k:4")).toBeDefined();
        open.dispose();
        expect(motion.of("k:4")).toBeUndefined();
        expect(motion.of("k:k1")).toBeDefined();
    });

    test("once stopped nothing is recorded, nothing rises, and nothing throws", () => {
        const motion = createMessageMotion();
        const scope = motion.scope();
        motion.arrived(annas);
        motion.stop();

        motion.sent("k9");
        motion.arrived({ id: 5, client_key: null });
        motion.changed(annas, "message.edited");
        motion.reacted(annas, "heart", "pop");
        motion.threadStarted(annas);

        expect(motion.of("k:k9")).toBeUndefined();
        expect(motion.of("k:3")).toBeUndefined();
        expect(scope.arrives("k:3")).toBeNull();
        expect(motion.scope().arrives("k:5")).toBeNull();
        expect(() => scope.dispose()).not.toThrow();
    });

    // Typing: presence records where someone starts and stops being drawn and why. Her message that
    // ends the last typing in its place is told in the same pass as its arrival, which it turns into
    // the handoff: the view draws the bubble growing into it rather than the row rising.
    test("her message ending the last typing in its place is a handoff, claimed in place, adjacent to its arrival", () => {
        const motion = createMessageMotion();
        const feed = motion.scope();
        motion.typingStarted("c1");

        motion.arrived(annas);
        motion.typingEnded("c1", "sent", annas);

        const arrival = motion.of("k:3")?.arrival;
        const ended = motion.typingOf("c1")?.ended;
        expect(arrival?.how).toBe("handoff");
        expect(ended?.cause).toBe("sent");
        expect(ended?.seq).toBe((arrival?.seq ?? 0) + 1);
        expect(feed.arrives("k:3")).toBe("inPlace");
        expect(feed.arrives("k:3")).toBeNull();
    });

    test("a typing end for any other cause leaves her arrival a rise, and names its cause", () => {
        const motion = createMessageMotion();
        const feed = motion.scope();
        motion.typingStarted("c1");
        motion.arrived(annas);

        motion.typingEnded("c1", "stopped");

        expect(motion.of("k:3")?.arrival?.how).toBe("live");
        expect(motion.typingOf("c1")?.ended?.cause).toBe("stopped");
        expect(motion.typingOf("c1")?.started?.seq).toBeLessThan(motion.typingOf("c1")?.ended?.seq ?? 0);
        expect(feed.arrives("k:3")).toBe("rise");
    });

    // The views watch one count before the render, so they read where things stand on screen
    // before any of them move: it moves for what changes the tail, and for nothing else.
    test("the tail's count moves for a send, an arrival and typing starting or ending, never for a row's own change", async () => {
        const motion = createMessageMotion();
        const woken = jest.fn();
        watch(() => motion.tail(), woken, { flush: "pre" });

        motion.changed(annas, "message.edited");
        motion.reacted(annas, "heart", "pop");
        motion.threadStarted(annas);
        await nextTick();
        expect(woken).not.toHaveBeenCalled();

        for (const write of [() => motion.sent("k1"), () => motion.arrived(annas), () => motion.typingStarted("c1"), () => motion.typingEnded("c1", "expired")]) {
            const before = motion.tail();
            write();
            expect(motion.tail()).toBeGreaterThan(before);
        }
        await nextTick();
        expect(woken).toHaveBeenCalledTimes(1);
    });

    test("a view gone lets go of typing no open view could play; once stopped typing records nothing", () => {
        const motion = createMessageMotion();
        const old = motion.scope();
        motion.typingStarted("c1");
        old.dispose();
        expect(motion.typingOf("c1")).toBeUndefined();

        motion.stop();
        motion.typingStarted("c2");
        motion.typingEnded("c2", "sent", annas);
        expect(motion.typingOf("c2")).toBeUndefined();
        expect(motion.of("k:3")).toBeUndefined();
    });

    test("schedules nothing: no timer is left after every kind of record", () => {
        jest.useFakeTimers();
        try {
            const motion = createMessageMotion();
            const scope = motion.scope();
            motion.sent("k1");
            motion.arrived(annas);
            motion.changed(annas, "message.visibility");
            motion.reacted(annas, "heart", "roll");
            motion.threadStarted(annas);
            motion.typingStarted("c1");
            motion.typingEnded("c1", "sent", annas);
            scope.arrives("k:3");
            scope.dispose();

            expect(jest.getTimerCount()).toBe(0);
        }
        finally {
            jest.useRealTimers();
        }
    });
});
