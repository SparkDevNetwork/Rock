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
// The threads list and the thread drawer's wiring to the shell: the list's tab, refresh and
// paging, a follow change from either and where its refusal is said, and the drawer's older
// replies. Which panel shows is the frame's; this is what the two panels ask of the shell.

import { computed, Ref } from "vue";
import { ToastNotice } from "../components/errorToast.partial.obs";
import type { ChatShell } from "../shell.partial";
import { FollowError, OpenThreadState, ThreadListState, ThreadListTab, unfollowNotice } from "./useThreads.partial";

/** What the threads panel reads from the block. */
export type ThreadsPanelDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The thread the drawer draws, whose follow change and replies are the drawer's. */
    drawnThread: Readonly<Ref<OpenThreadState | null>>;

    /** Shows a toast that is not a failure, one per code. */
    showNotice: (notice: ToastNotice) => void;
};

/**
 * Creates what the threads list and the drawer draw of the shell and the handlers they call.
 *
 * @param dependencies What the panel reads from the block.
 *
 * @returns The panel's values and handlers.
 */
export function useThreadsPanel(dependencies: ThreadsPanelDependencies) {
    const { shell, activeChannelId, drawnThread, showNotice } = dependencies;

    /** The threads list the panel draws, as the shell's threads hold it. */
    const threadList = computed((): ThreadListState => shell.value.threads.state.list);

    /**
     * A refused follow change is said once: by the drawer while it holds that thread, the drawer
     * it went back from included, and otherwise by the list's row.
     */
    const drawerFollowError = computed((): FollowError | null => {
        const error = shell.value.threads.state.followError;
        return error && error.rootId === drawnThread.value?.rootId ? error : null;
    });
    const listFollowError = computed((): FollowError | null => drawerFollowError.value ? null : shell.value.threads.state.followError);

    function onThreadsTab(tab: ThreadListTab): void {
        // A thread opened from the All tab may have moved the conversation, so the list's own
        // tab is opened for the conversation open now.
        if (tab === "channel" && activeChannelId.value && threadList.value.channelId !== activeChannelId.value) {
            void shell.value.threads.openList(activeChannelId.value, tab);
            return;
        }

        void shell.value.threads.switchTab(tab);
    }

    function onThreadsRefresh(): void {
        void shell.value.threads.refresh();
    }

    function onThreadsLoadMore(): void {
        void shell.value.threads.loadMore();
    }

    /**
     * A follow change from a row or the drawer: the bell changes at once and a refusal comes back
     * beside it. Following says nothing more; an unfollow that lands says what it means.
     */
    async function onFollowFromList(rootId: number, isFollowing: boolean): Promise<void> {
        if (!await shell.value.threads.follow(rootId, isFollowing) || isFollowing) {
            return;
        }

        showNotice(unfollowNotice);
    }

    /** The drawer's toggle, saved as the list's is. */
    function onFollowThread(isFollowing: boolean): void {
        if (drawnThread.value) {
            void onFollowFromList(drawnThread.value.rootId, isFollowing);
        }
    }

    function onLoadOlderReplies(): void {
        void shell.value.threads.loadOlderReplies();
    }

    return { threadList, drawerFollowError, listFollowError, onThreadsTab, onThreadsRefresh, onThreadsLoadMore, onFollowFromList, onFollowThread, onLoadOlderReplies };
}
