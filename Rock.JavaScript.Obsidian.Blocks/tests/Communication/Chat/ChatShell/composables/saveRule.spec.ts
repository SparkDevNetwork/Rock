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
// The one save rule every setting saves by. One save of a setting is out at a time: a change made
// while one is out, or while its pause runs, shows at once and replaces any change still waiting,
// whose promise resolves false, and the newest waiting is asked once the one out is answered and
// its pause has passed. A pause is counted on the page's own steady clock from when the change was
// made, so a system clock moved back never stretches it. A save that names a wait is asked again
// after it, outside the flight, and neither puts the control back nor says anything; one that
// names none and fails puts back the value last known saved, which is what the ask said it sent.
import { createSaveRule, SaveChange, SaveOutcome } from "../../../../../src/Communication/Chat/ChatShell/composables/saveRule.partial";
import { failedWords, finalWords } from "../../../../../src/Communication/Chat/ChatShell/errors.partial";
import { FailureWords } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(res => {
        resolve = res;
    });
    return { promise, resolve };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 20; i++) {
        await Promise.resolve();
    }
}

const refusedWords: FailureWords = finalWords("Chat could not understand that request.");

/** A save rule over one setting whose control is a plain value, with every ask held until answered. */
function setup(): {
    rule: ReturnType<typeof createSaveRule>,
    shown: { value: string },
    asks: Array<{ value: string, answer: Deferred<SaveOutcome> }>,
    refusals: FailureWords[],
    landings: unknown[],
    stopped: { value: boolean },
    change: (value: string, extra?: Partial<SaveChange<string>>) => SaveChange<string>
} {
    const shown = { value: "auto" };
    const asks: Array<{ value: string, answer: Deferred<SaveOutcome> }> = [];
    const refusals: FailureWords[] = [];
    const landings: unknown[] = [];
    const stopped = { value: false };
    const rule = createSaveRule(() => stopped.value);

    const change = (value: string, extra: Partial<SaveChange<string>> = {}): SaveChange<string> => ({
        was: shown.value,
        value,
        show: v => {
            shown.value = v;
        },
        ask: () => {
            const answer = deferred<SaveOutcome>();
            asks.push({ value, answer });
            return answer.promise;
        },
        landed: data => landings.push(data),
        refused: failure => refusals.push(failure),
        ...extra
    });

    return { rule, shown, asks, refusals, landings, stopped, change };
}

// #endregion Fixtures

describe("one save of a setting at a time", () => {
    test("two changes inside one flight show the newer at once and ask it once the first is answered, ending on the newer", async () => {
        const s = setup();

        const first = s.rule.save("status", s.change("busy"));
        const second = s.rule.save("status", s.change("auto"));

        expect(s.shown.value).toBe("auto");
        expect(s.asks.map(a => a.value)).toEqual(["busy"]);
        expect(s.rule.isOut("status")).toBe(true);

        s.asks[0].answer.resolve({ isSaved: true, data: "busy saved" });
        await settle();

        // The older's landing says nothing and moves nothing: the newer is shown and is asked now.
        expect(await first).toBe(false);
        expect(s.landings).toEqual([]);
        expect(s.shown.value).toBe("auto");
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);

        s.asks[1].answer.resolve({ isSaved: true, data: "auto saved" });

        expect(await second).toBe(true);
        expect(s.landings).toEqual(["auto saved"]);
        expect(s.rule.isOut("status")).toBe(false);
    });

    test("of three quick changes only the newest is asked after the first, the skipped one resolving false without asking", async () => {
        const s = setup();

        const first = s.rule.save("status", s.change("busy"));
        const skipped = s.rule.save("status", s.change("appear_offline"));
        const newest = s.rule.save("status", s.change("auto"));

        expect(await skipped).toBe(false);

        s.asks[0].answer.resolve({ isSaved: true });
        await settle();
        s.asks[1].answer.resolve({ isSaved: true });

        expect(await first).toBe(false);
        expect(await newest).toBe(true);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
    });

    test("two settings save side by side, each with its own flight", () => {
        const s = setup();

        void s.rule.save("a", s.change("busy"));
        void s.rule.save("b", s.change("auto"));

        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
    });

    test("the older refused while a newer waits says nothing and puts nothing back; the newer's refusal puts back where it started", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        void s.rule.save("status", s.change("appear_offline"));
        s.asks[0].answer.resolve({ isSaved: false, failure: refusedWords });
        await settle();

        expect(s.refusals).toEqual([]);
        expect(s.shown.value).toBe("appear_offline");

        s.asks[1].answer.resolve({ isSaved: false, failure: refusedWords });
        await settle();

        expect(s.shown.value).toBe("auto");
        expect(s.refusals).toEqual([refusedWords]);
    });
});

describe("the pause", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("Busy then Automatic inside a second sends one call, Automatic, once the newer's pause has passed", async () => {
        const s = setup();

        const busy = s.rule.save("status", s.change("busy", { settleMs: 1000 }));
        jest.advanceTimersByTime(400);
        const auto = s.rule.save("status", s.change("auto", { settleMs: 1000 }));

        expect(await busy).toBe(false);
        expect(s.shown.value).toBe("auto");
        // the pause is part of the save, so a load inside it leaves the choice shown
        expect(s.rule.isOut("status")).toBe(true);

        jest.advanceTimersByTime(999);
        expect(s.asks).toEqual([]);

        jest.advanceTimersByTime(1);
        expect(s.asks.map(a => a.value)).toEqual(["auto"]);

        s.asks[0].answer.resolve({ isSaved: true });
        expect(await auto).toBe(true);
    });

    test("is counted on the page's steady clock from when the change was made, so a system clock moved back ten minutes still waits one second", () => {
        const s = setup();
        jest.setSystemTime(Date.parse("2026-11-11T15:00:00.000Z"));

        void s.rule.save("status", s.change("busy", { settleMs: 1000 }));
        jest.setSystemTime(Date.parse("2026-11-11T14:50:00.000Z"));
        jest.advanceTimersByTime(1000);

        expect(s.asks.map(a => a.value)).toEqual(["busy"]);
    });

    test("is counted from the time the change says it was made, not from when it reached the rule", () => {
        const s = setup();

        const madeAt = performance.now();
        jest.advanceTimersByTime(700);
        void s.rule.save("status", s.change("busy", { settleMs: 1000, madeAt }));
        jest.advanceTimersByTime(300);

        expect(s.asks.map(a => a.value)).toEqual(["busy"]);
    });

    test("a change with no pause is asked at once", () => {
        const s = setup();

        void s.rule.save("star", s.change("on"));

        expect(s.asks.map(a => a.value)).toEqual(["on"]);
    });

    test("a change that waits behind a flight still waits out its own pause after the flight lands", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        void s.rule.save("status", s.change("auto", { settleMs: 1000 }));
        jest.advanceTimersByTime(200);
        s.asks[0].answer.resolve({ isSaved: true });
        await settle();

        expect(s.asks.map(a => a.value)).toEqual(["busy"]);

        jest.advanceTimersByTime(800);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
    });
});

describe("what is known saved", () => {
    test("is what the ask said it sent, not the change's own value", async () => {
        const s = setup();

        // The ask sent another tab's newer choice, so that is what the server now holds.
        void s.rule.save("status", s.change("busy"));
        void s.rule.save("status", s.change("appear_offline"));
        s.asks[0].answer.resolve({ isSaved: true, sent: "busy_until_8" });
        await settle();
        s.asks[1].answer.resolve({ isSaved: false, failure: refusedWords });
        await settle();

        expect(s.shown.value).toBe("busy_until_8");
    });

    test("an ask that sent nothing leaves what is known saved as it was", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        void s.rule.save("status", s.change("appear_offline"));
        s.asks[0].answer.resolve({ isSaved: true, sent: null });
        await settle();
        s.asks[1].answer.resolve({ isSaved: false, failure: refusedWords });
        await settle();

        // Nothing was sent, so the last known saved is still where it started.
        expect(s.shown.value).toBe("auto");
        expect(s.refusals).toEqual([refusedWords]);
    });

    test("an ask that says nothing about what it sent sent the change's own value", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        void s.rule.save("status", s.change("appear_offline"));
        s.asks[0].answer.resolve({ isSaved: true });
        await settle();
        s.asks[1].answer.resolve({ isSaved: false, failure: refusedWords });
        await settle();

        expect(s.shown.value).toBe("busy");
    });
});

describe("a save that names a wait", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("waits outside the flight, then asks again, neither putting the control back nor saying anything nor resolving", async () => {
        const s = setup();
        let isHeld = false;
        const heldWhileAsking: boolean[] = [];
        const exclusive = async (ask: () => Promise<SaveOutcome>): Promise<SaveOutcome> => {
            heldWhileAsking.push(isHeld);
            isHeld = true;
            try {
                return await ask();
            }
            finally {
                isHeld = false;
            }
        };
        let isResolved = false;

        void s.rule.save("status", s.change("busy", { exclusive })).then(() => {
            isResolved = true;
        });
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();

        expect(s.shown.value).toBe("busy");
        expect(s.refusals).toEqual([]);
        expect(isResolved).toBe(false);
        expect(s.rule.isOut("status")).toBe(true);
        // the wait holds nothing: the lock the ask runs under is free through it
        expect(isHeld).toBe(false);

        jest.advanceTimersByTime(9_999);
        expect(s.asks).toHaveLength(1);

        jest.advanceTimersByTime(1);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "busy"]);
        expect(heldWhileAsking).toEqual([false, false]);

        s.asks[1].answer.resolve({ isSaved: true });
        await settle();
        expect(isResolved).toBe(true);
    });

    test("a change made during the wait replaces the waiting one and is what is asked when the wait ends, not before", async () => {
        const s = setup();

        const first = s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();
        jest.advanceTimersByTime(4_000);
        const second = s.rule.save("status", s.change("auto"));

        expect(await first).toBe(false);
        jest.advanceTimersByTime(5_999);
        expect(s.asks).toHaveLength(1);

        jest.advanceTimersByTime(1);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
        s.asks[1].answer.resolve({ isSaved: true });
        expect(await second).toBe(true);
    });

    // A change made while the failing one was out goes into the same pace or outage, so it waits too.
    test("answered while a newer change waits: the older resolves false at once and the newer waits it out before it is asked", async () => {
        const s = setup();
        let isOlderResolved: boolean | null = null;

        void s.rule.save("status", s.change("busy")).then(saved => {
            isOlderResolved = saved;
        });
        const newer = s.rule.save("status", s.change("auto"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();

        expect(isOlderResolved).toBe(false);
        jest.advanceTimersByTime(9_999);
        expect(s.asks.map(a => a.value)).toEqual(["busy"]);
        jest.advanceTimersByTime(1);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
        s.asks[1].answer.resolve({ isSaved: true });
        expect(await newer).toBe(true);
    });

    test("is read from the wait alone, never from the words: final words with a wait are asked again, retryable words without one put the control back", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: finalWords("Chat is down for maintenance."), retryInMs: 1_000 });
        await settle();
        jest.advanceTimersByTime(1_000);

        expect(s.asks).toHaveLength(2);
        s.asks[1].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save.") });
        await settle();

        expect(s.shown.value).toBe("auto");
        expect(s.refusals).toEqual([failedWords("Couldn't save.")]);
    });
});

describe("a named wait that is no wait", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    // An owner that miscounts must never make the rule ask in a loop: a wait that is not a finite
    // number of milliseconds from zero up is final, and puts the control back once.
    test.each([
        ["not a number", NaN],
        ["endless", Infinity],
        ["before now", -1]
    ])("a wait %s is final: the control goes back, said once, and nothing is asked again", async (_name, retryInMs) => {
        const s = setup();

        const saved = s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs });
        await settle();
        jest.advanceTimersByTime(60_000);
        await settle();

        expect(s.asks).toHaveLength(1);
        expect(s.shown.value).toBe("auto");
        expect(s.refusals).toEqual([failedWords("Couldn't save.")]);
        expect(await saved).toBe(false);
    });
});

describe("a named wait longer than a timer holds", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
        jest.restoreAllMocks();
    });

    // A browser fires a timer longer than 2^31 - 1 ms at once; the rule would find the wait still
    // due and set it again, a timer spinning for the page's life.
    test("is kept by timers no longer than a browser holds, so none is set again and again", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 3_000_000_000 });
        await settle();
        const timers = jest.spyOn(global, "setTimeout");
        jest.advanceTimersByTime(60_000);

        expect(timers).not.toHaveBeenCalled();
        expect(s.asks).toHaveLength(1);
        expect(s.shown.value).toBe("busy");
    });
});

describe("a wake", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("ends a named wait at once, and the change waiting is asked", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();
        jest.advanceTimersByTime(3_000);
        s.rule.wake("status");

        expect(s.asks.map(a => a.value)).toEqual(["busy", "busy"]);
    });

    test("keeps a change's own pause, and with no named wait does nothing", async () => {
        const s = setup();

        void s.rule.save("status", s.change("busy", { settleMs: 1_000 }));
        s.rule.wake("status");
        s.rule.wake("another");
        jest.advanceTimersByTime(999);
        expect(s.asks).toHaveLength(0);

        jest.advanceTimersByTime(1);
        expect(s.asks).toHaveLength(1);
        s.rule.wake("status");
        expect(s.asks).toHaveLength(1);
    });
});

describe("hastening", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("ends a change's pause, so it is asked at once", () => {
        const s = setup();

        void s.rule.save("status", s.change("busy", { settleMs: 1_000 }));
        jest.advanceTimersByTime(200);
        s.rule.hasten("status");

        expect(s.asks.map(a => a.value)).toEqual(["busy"]);
    });

    test("leaves a named wait standing, and a setting with nothing waiting as it was", async () => {
        const s = setup();
        s.rule.hasten("status");

        void s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();
        s.rule.hasten("status");
        jest.advanceTimersByTime(9_999);
        expect(s.asks).toHaveLength(1);

        jest.advanceTimersByTime(1);
        expect(s.asks).toHaveLength(2);
    });
});

describe("a drop", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("drops the change waiting out a named wait, resolving false, and ends the wait, so the next change waits only its own pause", async () => {
        const s = setup();
        const first = s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 10_000 });
        await settle();

        s.rule.drop("status");
        expect(await first).toBe(false);
        expect(s.rule.isOut("status")).toBe(false);
        jest.advanceTimersByTime(60_000);
        expect(s.asks).toHaveLength(1);

        void s.rule.save("status", s.change("auto", { settleMs: 1_000 }));
        jest.advanceTimersByTime(1_000);
        expect(s.asks.map(a => a.value)).toEqual(["busy", "auto"]);
    });

    test("leaves another setting's change waiting", async () => {
        const s = setup();
        void s.rule.save("status", s.change("busy", { settleMs: 1_000 }));
        void s.rule.save("other", s.change("auto", { settleMs: 1_000 }));

        s.rule.drop("status");
        jest.advanceTimersByTime(1_000);

        expect(s.asks.map(a => a.value)).toEqual(["auto"]);
    });
});

describe("a failure that names no wait", () => {
    test("puts the control back and says why", async () => {
        const s = setup();

        const saving = s.rule.save("status", s.change("busy"));
        s.asks[0].answer.resolve({ isSaved: false, failure: refusedWords });

        expect(await saving).toBe(false);
        expect(s.shown.value).toBe("auto");
        expect(s.refusals).toEqual([refusedWords]);
        expect(s.rule.isOut("status")).toBe(false);
    });

    test("a call that throws is a failed save like any other, never left out", async () => {
        const s = setup();

        const saving = s.rule.save("status", s.change("busy", { ask: () => Promise.reject(new Error("lost")) }));

        expect(await saving).toBe(false);
        expect(s.shown.value).toBe("auto");
        expect(s.refusals.map(r => r.besideRetry)).toEqual(["Couldn't save."]);
        expect(s.rule.isOut("status")).toBe(false);
    });
});

describe("once stopped", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("a change waiting out its pause, one waiting behind a flight and one waiting out a named wait send nothing, and each resolves false", async () => {
        const s = setup();

        const paused = s.rule.save("pause", s.change("busy", { settleMs: 1000 }));
        void s.rule.save("flight", s.change("busy"));
        const behind = s.rule.save("flight", s.change("auto"));
        const retried = s.rule.save("retry", s.change("busy"));
        s.asks[1].answer.resolve({ isSaved: false, failure: failedWords("Couldn't save."), retryInMs: 5_000 });
        await settle();

        s.stopped.value = true;
        s.rule.stop();
        s.asks[0].answer.resolve({ isSaved: true });
        await settle();
        jest.advanceTimersByTime(60_000);

        expect(s.asks.map(a => a.value)).toEqual(["busy", "busy"]);
        expect(await paused).toBe(false);
        expect(await behind).toBe(false);
        expect(await retried).toBe(false);
        expect(s.landings).toEqual([]);
        expect(s.refusals).toEqual([]);
    });
});
