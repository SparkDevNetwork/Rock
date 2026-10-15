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
// Acting on a message: editing the person's own message, deleting it behind a confirm, copying
// it, quoting it into the next message, and running an action the server listed on it. Each call
// answers in its own time, so an answer that a newer edit or a conversation switch overtook is
// dropped rather than laid over what the person sees now, and nothing resolves into state once
// the actions are stopped. A server-listed action names the function it calls; only a function
// under the chat's action prefix is ever called, so a listed action cannot reach any other
// function the platform exposes.
//
// The composable these tests are written to, in useMessageActions.partial.ts:
//
//   createMessageActions(dependencies: {
//       call: (name: string, args: Record<string, unknown>) => Promise<CallResult>,  the shell's call path
//       timelines: Pick<Timelines, "state" | "applyEvent">,  where an answer is laid over the message
//       personAliasGuid: string,                the session's alias, for a live message's own check
//       notify: (notice: ToastNotice) => void,  a toast: { code, text, icon }
//       writeText?: (text: string) => Promise<void>   the clipboard; absent where there is none
//   }): MessageActions
//
//   MessageActions = {
//       state: {
//           editing: { channelId: string, messageId: number, isSaving: boolean, error: ChatError | null } | null,
//           confirmingDelete: { channelId: string, message: TimelineMessage } | null,
//           quotes: Record<string, TimelineMessage>   the message being quoted, by conversation or thread key
//       },
//       startEdit(channelId: string, messageId: number): void,
//       startEditNewest(channelId: string): boolean,   Up in an empty composer
//       cancelEdit(): void,
//       saveEdit(body: string): Promise<boolean>,
//       askDelete(channelId: string, message: TimelineMessage): void,   opens the confirm, calls nothing
//       cancelDelete(): void,
//       confirmDelete(): Promise<boolean>,
//       runAction(channelId: string, messageId: number, action: { id, label, call }): Promise<void>,
//       copy(message: TimelineMessage): Promise<void>,
//       startQuote(key: string, message: TimelineMessage): void,
//       cancelQuote(key: string): void,
//       switchConversation(channelId: string | null): void,   drops the editor and the confirm
//       stop(): void
//   }
//
// An edit answers { id, edited_at }, a delete { id, deleted_at }, as chat_edit_message and
// chat_delete_message do; a server action answers { notice }, the sentence the person is shown.
// A refusal is the call path's own: { ok: false, status, error: { code, severity, text }, retryAfterSeconds }.
import { createMessageActions } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const channel = "c0000001-0000-4000-8000-000000000000";
const otherChannel = "c0000002-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";
const meMerged = "a00000aa-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

/** A message in the shape a page carries it, with is_own and actions. Anna's unless told otherwise. */
function message(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        quoted_message_id: null,
        quoted: null,
        shown_in_channel: false,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: `message ${id}`,
        pieces: null,
        blocks: null,
        metadata: null,
        visibility: "visible",
        visibility_notice: null,
        is_own: false,
        actions: [],
        created_at: "2026-10-15T09:00:00Z",
        edited_at: null,
        deleted_at: null,
        ...extra
    } as unknown as TimelineMessage;
}

function own(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return message(id, { person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis", is_own: true, ...extra });
}

const ok = (data: unknown): CallResult => ({ ok: true, data });

const refused = (code: string, text: string): CallResult => ({
    ok: false,
    status: 400,
    error: { code, severity: "failed", text },
    retryAfterSeconds: null
});

type Call = { name: string, args: Record<string, unknown> };

/**
 * The actions over real timelines holding the given messages in the open channel, a call path
 * answering each call with the next answer, a deferred one when the test resolves it by hand.
 */
async function setup(messages: TimelineMessage[], answers: Array<CallResult | Deferred<CallResult>> = [], options: { writeText?: (text: string) => Promise<void> } = {}): Promise<{
    actions: ReturnType<typeof createMessageActions>,
    timelines: ReturnType<typeof createTimelines>,
    calls: Call[],
    notices: jest.Mock
}> {
    const timelines = createTimelines({
        fetchPage: async channelId => ({ messages: channelId === channel ? messages : [message(900, { body: "elsewhere" })], read_cursor: null, unread_count: 0, has_more: false })
    });
    await timelines.loadNewest(channel);
    await timelines.loadNewest(otherChannel);

    const calls: Call[] = [];
    const notices = jest.fn();
    const actions = createMessageActions({
        call: (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            const answer = answers.shift() ?? ok({});
            return "promise" in answer ? answer.promise : Promise.resolve(answer);
        },
        timelines,
        personAliasGuid: me,
        notify: notices,
        ...options
    } as unknown as Parameters<typeof createMessageActions>[0]);

    return { actions, timelines, calls, notices };
}

function held(timelines: ReturnType<typeof createTimelines>, id: number, channelId = channel): TimelineMessage {
    return timelines.state(channelId).messages.find(m => m.id === id) as TimelineMessage;
}

async function flush(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

function noticeTexts(notices: jest.Mock): string[] {
    return notices.mock.calls.map(([notice]) => (notice as { text: string }).text);
}

// #endregion Fixtures

describe("editing the person's own message", () => {
    test("calls chat_edit_message with the message and the new body, and lays the answer over the message", async () => {
        const { actions, timelines, calls } = await setup([own(42, { body: "See you at 8" })], [ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" })]);

        actions.startEdit(channel, 42);
        expect(actions.state.editing).toEqual(expect.objectContaining({ channelId: channel, messageId: 42 }));

        expect(await actions.saveEdit("See you at 9")).toBe(true);

        expect(calls).toEqual([{ name: "chat_edit_message", args: { p_message_id: 42, p_body: "See you at 9" } }]);
        expect(held(timelines, 42).body).toBe("See you at 9");
        expect(held(timelines, 42).edited_at).toBe("2026-10-15T09:05:00Z");
        expect(actions.state.editing).toBeNull();
    });

    test("a refusal keeps the editor open with the server's sentence, and the message as it was", async () => {
        const { actions, timelines, calls } = await setup([own(42, { body: "See you at 8" })], [refused("rpc.not_editable", "This message can't be edited.")]);

        actions.startEdit(channel, 42);
        expect(await actions.saveEdit("See you at 9")).toBe(false);

        expect(calls.map(c => c.name)).toEqual(["chat_edit_message"]);
        expect(actions.state.editing).toEqual(expect.objectContaining({ channelId: channel, messageId: 42, isSaving: false }));
        expect(actions.state.editing?.error?.text).toBe("This message can't be edited.");
        expect(held(timelines, 42).body).toBe("See you at 8");
        expect(held(timelines, 42).edited_at).toBeNull();
    });

    test("is saving while the call is out", async () => {
        const answer = deferred<CallResult>();
        const { actions } = await setup([own(42)], [answer]);

        actions.startEdit(channel, 42);
        const saving = actions.saveEdit("changed");

        expect(actions.state.editing?.isSaving).toBe(true);
        answer.resolve(ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" }));
        await saving;
    });

    // The second edit's answer landing first must not be undone by the first's arriving late, and
    // the first's landing early must not close the editor the second edit still has open.
    test.each([["the second answer first", "second-first"], ["the first answer first", "first-first"]] as const)(
        "an answer overtaken by a second edit of the same message is dropped: %s",
        async (_order, order) => {
            const first = deferred<CallResult>();
            const second = deferred<CallResult>();
            const { actions, timelines, calls } = await setup([own(42, { body: "original" })], [first, second]);

            actions.startEdit(channel, 42);
            const firstSave = actions.saveEdit("first try");
            actions.startEdit(channel, 42);
            const secondSave = actions.saveEdit("second try");
            expect(calls.map(c => c.args.p_body)).toEqual(["first try", "second try"]);

            if (order === "second-first") {
                second.resolve(ok({ id: 42, edited_at: "2026-10-15T09:06:00Z" }));
                await secondSave;
                first.resolve(ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" }));
                await firstSave;
            }
            else {
                first.resolve(ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" }));
                await firstSave;
                expect(actions.state.editing).toEqual(expect.objectContaining({ messageId: 42, isSaving: true }));
                second.resolve(ok({ id: 42, edited_at: "2026-10-15T09:06:00Z" }));
                await secondSave;
            }

            expect(held(timelines, 42).body).toBe("second try");
            expect(held(timelines, 42).edited_at).toBe("2026-10-15T09:06:00Z");
            expect(actions.state.editing).toBeNull();
        }
    );

    test.each([["a refusal", "refused"], ["an acceptance", "accepted"]] as const)(
        "%s overtaken by a conversation switch does not touch the editor open in the next conversation",
        async (_what, outcome) => {
            const first = deferred<CallResult>();
            const { actions, calls } = await setup([own(42)], [first]);
            await Promise.resolve();

            actions.startEdit(channel, 42);
            const saving = actions.saveEdit("changed");
            expect(calls.map(c => c.name)).toEqual(["chat_edit_message"]);

            actions.switchConversation(otherChannel);
            actions.startEdit(otherChannel, 900);
            first.resolve(outcome === "refused" ? refused("authz.not_author", "You can only edit your own messages.") : ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" }));
            await saving;

            expect(actions.state.editing).toEqual({ channelId: otherChannel, messageId: 900, isSaving: false, error: null });
        }
    );

    test("cancel closes the editor and calls nothing", async () => {
        const { actions, calls } = await setup([own(42)]);

        actions.startEdit(channel, 42);
        expect(actions.state.editing).not.toBeNull();
        actions.cancelEdit();

        expect(actions.state.editing).toBeNull();
        expect(calls).toEqual([]);
    });
});

// Up in an empty composer edits the newest message the person could edit: their own, live,
// shown and not a block post, which the database would refuse.
describe("editing the newest own message", () => {
    test("skips others' messages, deleted, hidden and block posts, and opens the newest own sendable one", async () => {
        const { actions } = await setup([
            own(10, { body: "older own" }),
            own(11, { is_own: undefined, person_alias_guid: me, body: "own by the session's alias, from a live copy" }),
            own(12, { deleted_at: "2026-10-15T09:01:00Z", body: null }),
            own(13, { visibility: "hidden", visibility_notice: "Hidden by a moderator.", body: null }),
            own(14, { message_type: "system", blocks: [{ type: "text", text: "Card", fallback: { type: "text", text: "Card" } }] }),
            message(15, { body: "Anna's, newest" })
        ]);

        expect(actions.startEditNewest(channel)).toBe(true);

        expect(actions.state.editing).toEqual(expect.objectContaining({ channelId: channel, messageId: 11 }));
    });

    test("an own message sent under a merged alias counts, by the page's is_own", async () => {
        const { actions } = await setup([own(20, { person_alias_guid: meMerged, is_own: true }), message(21)]);

        expect(actions.startEditNewest(channel)).toBe(true);

        expect(actions.state.editing?.messageId).toBe(20);
    });

    test("with nothing the person could edit, opens nothing", async () => {
        const { actions } = await setup([own(30), message(31)]);
        expect(actions.startEditNewest(channel)).toBe(true);
        actions.cancelEdit();

        const empty = await setup([message(32), own(33, { deleted_at: "2026-10-15T09:01:00Z", body: null })]);

        expect(empty.actions.startEditNewest(channel)).toBe(false);
        expect(empty.actions.state.editing).toBeNull();
    });
});

describe("deleting the person's own message", () => {
    test("asking opens the confirm and calls nothing; only the confirm calls chat_delete_message", async () => {
        const target = own(42);
        const { actions, timelines, calls, notices } = await setup([target], [ok({ id: 42, deleted_at: "2026-10-15T09:07:00Z" })]);

        actions.askDelete(channel, target);
        expect(calls).toEqual([]);
        expect(actions.state.confirmingDelete).toEqual(expect.objectContaining({ channelId: channel }));

        expect(await actions.confirmDelete()).toBe(true);

        expect(calls).toEqual([{ name: "chat_delete_message", args: { p_message_id: 42 } }]);
        expect(held(timelines, 42).deleted_at).toBe("2026-10-15T09:07:00Z");
        expect(held(timelines, 42).body).toBeNull();
        expect(actions.state.confirmingDelete).toBeNull();
        expect(noticeTexts(notices)).toEqual(["Message deleted"]);
    });

    test("cancelling the confirm calls nothing and leaves the message", async () => {
        const target = own(42);
        const { actions, timelines, calls } = await setup([target]);

        actions.askDelete(channel, target);
        expect(actions.state.confirmingDelete).not.toBeNull();
        actions.cancelDelete();
        await flush();

        expect(calls).toEqual([]);
        expect(actions.state.confirmingDelete).toBeNull();
        expect(held(timelines, 42).deleted_at).toBeNull();
    });

    test("a refusal leaves the message and says the server's sentence", async () => {
        const target = own(42);
        const { actions, timelines, notices } = await setup([target], [refused("authz.not_author", "You can only delete your own messages.")]);

        actions.askDelete(channel, target);
        expect(await actions.confirmDelete()).toBe(false);

        expect(held(timelines, 42).deleted_at).toBeNull();
        expect(noticeTexts(notices)).toEqual(["You can only delete your own messages."]);
    });
});

describe("a server-listed action", () => {
    const approve = { id: "approve", label: "Approve", call: "chat_action_approve" };

    test("calls its function with the message and the action, and toasts the answer's sentence", async () => {
        const { actions, calls, notices } = await setup([message(42)], [ok({ notice: "Approved. Anna will be told." })]);

        await actions.runAction(channel, 42, approve);

        expect(calls).toEqual([{ name: "chat_action_approve", args: { p_message_id: 42, p_action_id: "approve" } }]);
        expect(noticeTexts(notices)).toEqual(["Approved. Anna will be told."]);
    });

    test("a refusal toasts the refusal's sentence", async () => {
        const { actions, notices } = await setup([message(42)], [refused("rpc.not_allowed", "Only leaders can approve this.")]);

        await actions.runAction(channel, 42, approve);

        expect(noticeTexts(notices)).toEqual(["Only leaders can approve this."]);
    });

    test.each([
        ["another platform function", "chat_delete_message"],
        ["the prefix alone", "chat_action_"],
        ["capital letters", "chat_action_Approve"],
        ["a path", "chat_action_x/../chat_delete_message"],
        ["a space and more", "chat_action_x; select 1"],
        ["a prefix not at the start", "x_chat_action_approve"]
    ])("a call outside the action prefix, %s, is never called", async (_what, call) => {
        const { actions, calls } = await setup([message(42)], [ok({ notice: "Done." }), ok({ notice: "Done." })]);

        await actions.runAction(channel, 42, { id: "x", label: "Do it", call });
        await actions.runAction(channel, 42, approve);

        expect(calls.map(c => c.name)).toEqual(["chat_action_approve"]);
    });
});

describe("copying a message's text", () => {
    test("writes to the clipboard and says so", async () => {
        const writeText = jest.fn(async () => undefined);
        const { actions, notices } = await setup([message(42, { body: "Doors open at 6" })], [], { writeText });

        await actions.copy(message(42, { body: "Doors open at 6" }));

        expect(writeText).toHaveBeenCalledTimes(1);
        expect(noticeTexts(notices)).toEqual(["Copied to clipboard"]);
    });

    test("where there is no clipboard, says copy isn't available", async () => {
        const { actions, notices } = await setup([message(42)]);

        await actions.copy(message(42));

        expect(noticeTexts(notices)).toEqual(["Copy isn't available here"]);
    });
});

describe("the message being quoted", () => {
    test("is held per conversation, so a quote started in one is not drawn in another, and cancel drops it", async () => {
        const { actions } = await setup([message(42)]);

        actions.startQuote(channel, message(42));

        expect(actions.state.quotes[channel]?.id).toBe(42);
        expect(actions.state.quotes[otherChannel]).toBeUndefined();

        actions.cancelQuote(channel);
        expect(actions.state.quotes[channel]).toBeUndefined();
    });
});

describe("after stop", () => {
    test.each([["an edit", "edit"], ["a delete", "delete"], ["a server action", "action"]] as const)("%s answering resolves nothing into state", async (_what, kind) => {
        const answer = deferred<CallResult>();
        const target = own(42, { body: "original" });
        const { actions, timelines, calls, notices } = await setup([target], [answer]);

        let pending: Promise<unknown>;
        if (kind === "edit") {
            actions.startEdit(channel, 42);
            pending = actions.saveEdit("changed");
        }
        else if (kind === "delete") {
            actions.askDelete(channel, target);
            pending = actions.confirmDelete();
        }
        else {
            pending = actions.runAction(channel, 42, { id: "approve", label: "Approve", call: "chat_action_approve" });
        }
        expect(calls).toHaveLength(1);
        const editingBefore = JSON.stringify(actions.state.editing);

        actions.stop();
        answer.resolve(kind === "edit"
            ? ok({ id: 42, edited_at: "2026-10-15T09:05:00Z" })
            : kind === "delete" ? ok({ id: 42, deleted_at: "2026-10-15T09:07:00Z" }) : ok({ notice: "Approved." }));
        await pending;
        await flush();

        expect(held(timelines, 42).body).toBe("original");
        expect(held(timelines, 42).edited_at).toBeNull();
        expect(held(timelines, 42).deleted_at).toBeNull();
        expect(JSON.stringify(actions.state.editing)).toBe(editingBefore);
        expect(notices).not.toHaveBeenCalled();
    });

    test("starts no new call", async () => {
        const target = own(42);
        const { actions, calls } = await setup([target]);
        actions.startEdit(channel, 42);
        expect(actions.state.editing).not.toBeNull();

        actions.stop();
        await actions.saveEdit("changed");
        actions.askDelete(channel, target);
        await actions.confirmDelete();
        await actions.runAction(channel, 42, { id: "approve", label: "Approve", call: "chat_action_approve" });

        expect(calls).toEqual([]);
    });
});
