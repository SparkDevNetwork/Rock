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
// Keeping what the client holds current: the live connection's routes, which apply each change as
// it arrives, and the sidebar read, which restates the whole list. The two share one piece of
// state: a change that arrives while a read is out is applied at once and journaled, then replayed
// over the read's answer, which may predate it. When neither is enough, resync reads everything on
// screen again.

import { classifyPlatformError, inRoomKind } from "./errors.partial";
import type { CallResult } from "./platformCall.partial";
import { isTimelineMessage } from "./heldMessage.partial";
import type { RealtimeHubDependencies } from "./composables/useRealtimeHub.partial";
import type { MeSettings } from "./composables/usePrefs.partial";
import type { ChatShell, ShellState } from "./shell.partial";
import { ChannelReadEvent, ChannelUnreadEvent, ChatBadge, ChatError, MarkReadResult, MessageCreatedEvent, ReactionFavorite, SidebarRow, UnreadMention } from "./types.partial";

/**
 * The longest a viewer waits before fetching a new token when the channel on screen changed, or
 * before reading the sidebar after a rejoin, an estimate. Everyone watching is told at once, and
 * the wait spreads their requests at Rock and Realtime.
 */
const channelChangedWaitMs = 5_000;

/** The live signals that state a row's value, and so can be replayed over a sidebar's answer. */
const replayedEvents = new Set(["channel.unread", "channel.mention", "dm.preview", "channel.read"]);

/** A change kept while a sidebar read is out: a personal event, or a save of a read position. */
type JournaledChange = { event: string, payload: unknown } | { channelId: string, result: MarkReadResult, sentAt: number | undefined };

/** What keeping the client current reads from the shell. */
export type ShellLiveDependencies = {
    state: ShellState;

    /** The shell's modules, each made before this. */
    modules: Pick<ChatShell, "reveal" | "channels" | "timelines" | "threads" | "messageActions" | "reactions" | "mentions" | "rooms" | "roomKindOf" | "members" | "prefs" | "blocks" | "sender" | "directMessages" | "problems" | "presence">;

    /** The shell's one call path. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** Tells the person about a failure. */
    report: (error: ChatError) => void;

    /** A refusal in the shape the classifier reads. */
    thrown: (result: Extract<CallResult, { ok: false }>) => unknown;

    /** A save of the read position, applied to the row and the unread mentions. */
    applySave: (channelId: string, result: MarkReadResult, sentAt: number | undefined) => void;

    /** The person's favorite reactions, filled in place from each sidebar answer. */
    favorites: ReactionFavorite[];

    /** Whether a failure is the connection itself, which its banner explains. */
    isConnectionFailure: (error: ChatError) => boolean;

    /** Asks again, with a new token, whether the person may still read what they have open. */
    recheck: () => void;

    /** Takes the open channel off the screen. */
    closeActive: () => void;

    /** Whether the sidebar is narrowed to the person's campus, as the block setting says. */
    isCampusLimited: boolean;

    /** Marks a step on the page's performance timeline. */
    mark: (name: string) => void;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;
};

/** Keeping the client current, as the shell joins it to the rest. */
export type ShellLive = {
    /** Fetches the sidebar, or while a read is out, has one more read follow it. */
    loadSidebar: () => Promise<SidebarRow[]>;

    /** Reads the sidebar again, unless a rejoin's wait is pending, whose read this then is. */
    reloadSidebar: () => void;

    /** Keeps a save that landed while a sidebar read is out, to replay over its answer. */
    noteSave: (channelId: string, result: MarkReadResult, sentAt: number | undefined) => void;

    /** The people blocked changed: what was released is let go and what is on screen read again. */
    onBlocksChanged: (isInSidebarAnswer: boolean, removed: readonly string[]) => void;

    /** The people blocked gained these aliases: every holder masks again what it holds. */
    remask: (added: readonly string[]) => void;

    /** What the live connection's hub calls as each word arrives. */
    handlers: Pick<RealtimeHubDependencies, "onChannelEvent" | "onPersonalEvent" | "onPersonalRejoined" | "onJoined" | "onUnknownError" | "onStatus">;

    /** Cancels the recheck's and the rejoin's waits. */
    stop: () => void;
};

/**
 * Creates what keeps the client's rooms, sidebar and settings current.
 *
 * @param dependencies What it reads from the shell.
 *
 * @returns The sidebar read, the live routes and the resync.
 */
export function createShellLive(dependencies: ShellLiveDependencies): ShellLive {
    const { state, modules, call, report, thrown, applySave, favorites, isConnectionFailure, recheck, closeActive, mark, isStopped } = dependencies;
    const { reveal, channels, timelines, threads, messageActions, reactions, mentions, rooms, roomKindOf, members, prefs, blocks, sender, directMessages, problems, presence } = modules;

    /** The pending wait before a recheck, so a stopped or ended shell can cancel it. */
    let recheckTimer: ReturnType<typeof setTimeout> | null = null;

    // A sidebar read is a snapshot taken some time after it is asked for, so only the newest asked
    // for may land. One read is out at a time: an ask while one is out overtakes it, its answer is
    // dropped, and one more read follows however many ask meanwhile. Each overtaking ask moves the
    // generation on.
    let sidebarGeneration = 0;

    /** The sidebar read out, which settles with its rows, or null once a later ask overtook it. */
    let sidebarRead: Promise<SidebarRow[] | null> | null = null;

    // What the newest ask answers with: the read out, or the one queued behind it, so a caller
    // waiting on the sidebar, the page load above all, always gets the sidebar that landed.
    let sidebarAnswer: Promise<SidebarRow[]> = Promise.resolve([]);
    let isSidebarQueued = false;

    /** The changes that arrived while a read is out, replayed over its answer; null while none is out. */
    let sidebarJournal: JournaledChange[] | null = null;

    /** The wait before a rejoin's sidebar read, pending; null when none is. */
    let sidebarWait: ReturnType<typeof setTimeout> | null = null;

    /** Cancels a recheck still waiting. */
    function cancelRecheck(): void {
        if (recheckTimer !== null) {
            clearTimeout(recheckTimer);
            recheckTimer = null;
        }
    }

    /** Cancels a rejoin's wait still pending. */
    function cancelSidebarWait(): void {
        if (sidebarWait !== null) {
            clearTimeout(sidebarWait);
            sidebarWait = null;
        }
    }

    /**
     * Fetches the sidebar and replaces the rows with it, or, while a read is out, has one more read
     * follow it. A read a newer ask overtook answers with the newer one's sidebar.
     */
    function loadSidebar(): Promise<SidebarRow[]> {
        if (isSidebarQueued) {
            return sidebarAnswer;
        }

        const overtaken = sidebarRead;
        if (overtaken === null) {
            sidebarAnswer = startSidebarRead();
            return sidebarAnswer;
        }

        isSidebarQueued = true;
        sidebarGeneration++;
        const next = (): Promise<SidebarRow[]> | SidebarRow[] => {
            isSidebarQueued = false;
            return isStopped() ? channels.rows : startSidebarRead();
        };
        sidebarAnswer = overtaken.then(next, next);
        return sidebarAnswer;
    }

    /** Sends one sidebar read, the only one out until it settles. A read overtaken answers with the newer one's sidebar. */
    function startSidebarRead(): Promise<SidebarRow[]> {
        const read = readSidebar();
        sidebarRead = read;
        const settled = (): void => {
            if (sidebarRead === read) {
                sidebarRead = null;
            }
        };
        read.then(settled, settled);
        return read.then(rows => rows ?? sidebarAnswer);
    }

    /** One sidebar read, which lands unless a later ask overtook it, and then answers null. */
    async function readSidebar(): Promise<SidebarRow[] | null> {
        const generation = sidebarGeneration;
        sidebarJournal = [];
        // what each timeline held as the read was asked, so a row that live events have since
        // passed is not taken for a room whose history went backwards
        const heldAtAsk = timelines.revsHeld();
        const result = await call("chat_get_bootstrap", { p_campus_only: dependencies.isCampusLimited });
        if (isStopped()) {
            return channels.rows;
        }
        if (generation !== sidebarGeneration) {
            return null;
        }
        const journal = sidebarJournal;
        sidebarJournal = null;
        mark("chat:sidebar");

        if (!result.ok) {
            // The list's own line tells the failure, so the first load does not wait on it.
            reveal.sidebarAnswered();
            reveal.failed();

            // Before the first sidebar has landed, the list's line with Try again tells it, so the
            // toast is kept for a session that ended, which the line cannot mend, and a later read.
            if (channels.loadState === "loaded" || result.error.severity === "session") {
                report(result.error);
            }
            channels.setLoadFailed();
            throw thrown(result);
        }

        // the bootstrap's own key names
        /* eslint-disable-next-line @typescript-eslint/naming-convention */
        const answer = result.data as { channels?: SidebarRow[], reactions?: ReactionFavorite[], badges?: ChatBadge[], me?: Partial<MeSettings> & { unread_mentions?: UnreadMention[], blocked_aliases?: string[] } } | null;
        const rows = answer?.channels ?? [];
        // through the settings, which keep any room setting whose save is still out
        prefs.setRooms(rows);
        // every load restates the unread mentions, so one read elsewhere stops lighting the dot
        mentions.setUnread(answer?.me?.unread_mentions ?? []);
        // the person's own switches, as another device may have changed them since
        prefs.setMe(answer?.me);
        // Only the sidebar call names whom they blocked: chat_set_prefs answers its own part of me
        // and never this, so the set is the blocks' own.
        blocks.setAliases(answer?.me?.blocked_aliases);
        if (Array.isArray(answer?.reactions)) {
            favorites.splice(0, favorites.length, ...answer.reactions);
        }
        // a profile card names a person's badge keys by these
        channels.setBadges(Array.isArray(answer?.badges) ? answer.badges : []);

        // In arrival order, through the same handlers; each states a value, and a mention the
        // answer already counted is not counted again.
        for (const change of journal) {
            if ("event" in change) {
                channels.applyPersonalEvent(change.event, change.payload, true);
                mentions.applyPersonalEvent(change.event, change.payload);
            }
            else {
                applySave(change.channelId, change.result, change.sentAt);
            }
        }

        // each row's revision against the room's timeline, which a room without one never reads
        for (const row of rows) {
            if (typeof row.rev === "number") {
                timelines.noteSidebar(row.channel_id, row.rev, heldAtAsk.get(row.channel_id) ?? null, row.channel_id === state.activeChannelId);
            }
        }

        // a join still settling is settled by the reload that shows the room as the person's
        rooms.sidebarLoaded();
        reveal.sidebarAnswered();
        return channels.rows;
    }

    /**
     * Reads the sidebar again, unless a rejoin's wait is pending, whose read this then is. A failed
     * read is reported where it fails, so nothing is left to catch here.
     */
    function reloadSidebar(): void {
        if (sidebarWait === null) {
            void loadSidebar().catch(() => undefined);
        }
    }

    /**
     * Reads the sidebar again after the personal topic was joined again, after a random wait so a
     * Realtime restart that rejoins every client at once does not read every sidebar in the same
     * second. What the members hold of a room's rights may have missed a change too.
     */
    function reloadAfterRejoin(): void {
        if (sidebarWait !== null || isStopped()) {
            return;
        }
        sidebarWait = setTimeout(() => {
            sidebarWait = null;
            members.rejoined();
            void loadSidebar().catch(() => undefined);
        }, Math.random() * channelChangedWaitMs);
    }

    /**
     * What this client holds may be out of date in a way no other event describes: the platform
     * says so, or the people blocked changed. Everything on screen is read again: the sidebar, the
     * open channel, its facts once the sidebar has said whether it still lists the room, the open
     * thread, and the panel shown. Nothing is reminted: what the person may read has not changed.
     *
     * @param landingSidebar The sidebar read whose answer is landing and asked for this, which
     * stands for the sidebar step; null reads the sidebar again.
     */
    function resync(landingSidebar: Promise<unknown> | null = null): void {
        const channelId = state.activeChannelId;
        void (landingSidebar ?? loadSidebar())
            .catch(() => undefined)
            .then(() => {
                if (channelId && state.activeChannelId === channelId) {
                    rooms.roomChanged(channelId);
                }
            });
        mentions.resync();
        threads.resync();
        members.rejoined();
        if (channelId) {
            // The newest page redraws only the latest messages, so the channel then catches up from
            // it, but only while it is still on screen: one left meanwhile has nobody to catch up for.
            void timelines.loadNewest(channelId)
                .then(() => state.activeChannelId === channelId ? timelines.onJoined(channelId) : undefined)
                .catch(error => report(classifyPlatformError(error)));
        }
    }

    /**
     * Every holder of messages masks again what it holds, in place. A holder added here is added to
     * resync beside it when what it holds can be read again; the found conversation's look and a
     * quote waiting in a composer are snapshots resync does not read, so after an unblock they stay
     * withheld, the safe side, until the person looks again.
     */
    function remask(added: readonly string[]): void {
        timelines.remask(added);
        threads.remask();
        mentions.remask();
        directMessages.remask();
        messageActions.remask();
        presence.remask();
    }

    const handlers: ShellLive["handlers"] = {
        onChannelEvent: (channelId, event, rawPayload) => {
            // The revision first, whatever the event's name and whatever a handler then makes of it,
            // so a name this release does not know, or a message collapsed below, leaves no hole.
            timelines.advance(channelId, rawPayload);
            // Typing changes nothing held, so nothing that holds messages reads it.
            if (presence.owns(event)) {
                presence.heard(event, rawPayload, channelId);
                return;
            }
            // A room's events are one payload for every subscriber, so a message by someone the
            // person blocked is collapsed, and their face blanked, before anything holds it.
            const payload = blocks.maskEvent(event, rawPayload);
            if (event === "resync") {
                resync();
                return;
            }
            if (event === "channel.changed") {
                if (channelId === state.activeChannelId) {
                    cancelRecheck();
                    recheckTimer = setTimeout(recheck, Math.random() * channelChangedWaitMs);
                }
                return;
            }
            // The threads go first: they set the root's marks, which the timeline then records with
            // the counts so a page read earlier cannot undo them. A reaction reaches the open
            // thread's copies at once; only the conversation's copy waits for the conversation's page.
            threads.applyEvent(channelId, event, payload);
            if (event === "reaction.added" || event === "reaction.removed") {
                reactions.applyThreadEvent(channelId, event, payload);
            }
            timelines.applyEvent(channelId, event, payload);

            // Any copy settles the send, after the views have taken theirs: a reply kept to a thread
            // that is not open is kept by no view. A message of a channel still loading settles at
            // its page instead, so its row is never taken away before the message is drawn.
            const created = payload as Partial<MessageCreatedEvent> | null;
            // a message ends its sender's typing in its place
            if (event === "message.created" && created) {
                presence.messageArrived(channelId, created);
            }
            if (event === "message.created" && typeof created?.id === "number" && created.client_key
                && (timelines.state(channelId).isLoaded || !isTimelineMessage(created))) {
                sender.landed({ id: created.id, client_key: created.client_key });
            }
        },
        onPersonalEvent: (event, payload) => {
            // A typist a block keeps off the conversation's topic is told on each other member's own.
            if (presence.owns(event)) {
                presence.heard(event, payload);
                return;
            }
            if (event === "resync") {
                resync();
                return;
            }
            // Another page or device changed whom the person blocked; the set is read again.
            if (event === "blocks.changed") {
                void blocks.changed();
                return;
            }
            channels.applyPersonalEvent(event, payload);
            mentions.applyPersonalEvent(event, payload);
            // how far they read on another device reaches the conversation's held page too
            const read = payload as Partial<Record<keyof ChannelReadEvent, unknown>> | null;
            if (event === "channel.read" && typeof read?.channel_id === "string" && typeof read.read_cursor === "number") {
                timelines.readSaved(read.channel_id, read.read_cursor);
            }
            if (replayedEvents.has(event)) {
                sidebarJournal?.push({ event, payload });
            }

            const changed = (payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null)?.channel_id;

            // A conversation Rock made before the platform had taken it may have arrived.
            if (event === "membership.changed") {
                void directMessages.membershipChanged(typeof changed === "string" ? changed : null);
            }
            // A room's facts change with its members and with its own name, description or kind.
            if (event === "membership.changed") {
                rooms.roomChanged(typeof changed === "string" ? changed : null);
            }
            if (event === "channel.updated") {
                rooms.channelUpdated(typeof changed === "string" ? changed : null);
            }
            if (event === "membership.changed" || event === "channel.updated") {
                members.roomChanged(typeof changed === "string" ? changed : null);
            }
            // Only the channel on screen is joined, so only a change to it can need a cut.
            if (event === "session.recheck" || (event === "membership.changed" && changed === state.activeChannelId)) {
                recheck();
            }
        },
        onPersonalRejoined: reloadAfterRejoin,
        onJoined: channelId => {
            mark("chat:join");
            timelines.onJoined(channelId).catch(error => report(classifyPlatformError(error)));
        },
        onUnknownError: () => {
            problems.report("realtime", "rt.unknown");
            // Words this client does not know may be a lost read it was not built to recognise. A
            // new token makes Realtime decide again, and the sidebar read again shows what the
            // person may still see.
            recheck();
            void loadSidebar().catch(() => undefined);
        },
        onStatus: error => {
            // The hub has already left a channel whose read was revoked; the person is told, and the
            // list is loaded again without it.
            if (error?.code === "rt.read_revoked") {
                // Only the shell knows which room was open, so it names its kind before closing it;
                // choosing it again, once the person may read it, opens it afresh.
                const roomKind = state.activeChannelId ? roomKindOf(state.activeChannelId) : null;
                closeActive();
                report(inRoomKind(error, roomKind));
                void loadSidebar().catch(() => undefined);
                return;
            }
            state.connection = error;

            // A call that failed just before the drop was noticed was the same outage, which the
            // banner now explains.
            if (error) {
                state.errors = state.errors.filter(e => !isConnectionFailure(e));
            }
        }
    };

    return {
        loadSidebar,
        reloadSidebar,
        noteSave: (channelId, result, sentAt) => {
            sidebarJournal?.push({ channelId, result, sentAt });
        },
        // A change named by a sidebar answer comes in the newest sidebar there is, so that read is
        // the resync's sidebar step rather than another.
        onBlocksChanged: (isInSidebarAnswer, removed) => {
            if (removed.length > 0) {
                timelines.release(removed);
            }
            resync(isInSidebarAnswer ? sidebarRead : null);
        },
        remask,
        handlers,
        stop: () => {
            cancelRecheck();
            cancelSidebarWait();
        }
    };
}
