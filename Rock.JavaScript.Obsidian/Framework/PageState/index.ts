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
import { State } from "./state";
import { shallowReadonly, reactive } from "vue";
import { PageConfig } from "@Obsidian/Utility/page";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

// This needs to move elsewhere probably.
export type PageDebugTiming = {
    title: string;
    subtitle: string;
    startTimeMs: number;
    finishTimeMs: number;
};

// This is the private state that we can modify.
const state: State = reactive({
    areSecondaryBlocksShown: true,
    currentPerson: null,
    isAnonymousVisitor: false,
    pageParameters: {},
    contextEntities: {},
    pageId: 0,
    pageGuid: "",
    executionStartTime: RockDateTime.now().toMilliseconds(),
    debugTimings: [],
    loginUrlWithReturnUrl: ""
});

export class Store {
    public state: Readonly<State>;

    constructor() {
        this.state = shallowReadonly(state);
    }

    setAreSecondaryBlocksShown(areSecondaryBlocksShown: boolean): void {
        state.areSecondaryBlocksShown = areSecondaryBlocksShown;
    }

    initialize(pageConfig: PageConfig): void {
        state.currentPerson = pageConfig.currentPerson || null;
        state.isAnonymousVisitor = pageConfig.isAnonymousVisitor;
        state.pageParameters = pageConfig.pageParameters || {};
        state.pageId = pageConfig.pageId || 0;
        state.pageGuid = pageConfig.pageGuid || "";
        state.executionStartTime = pageConfig.executionStartTime;
        state.loginUrlWithReturnUrl = pageConfig.loginUrlWithReturnUrl;
    }

    addPageDebugTiming(timing: PageDebugTiming): void {
        const pageStartTime = state.executionStartTime;
        const timestampMs = timing.startTimeMs - pageStartTime;
        const durationMs = timing.finishTimeMs - timing.startTimeMs;

        state.debugTimings.push({
            timestampMs: timestampMs,
            durationMs: durationMs,
            indentLevel: 1,
            isTitleBold: false,
            subTitle: timing.subtitle,
            title: timing.title
        });
    }

    // This should be replaced with something else, doesn't really fit as a store action.
    redirectToLogin(): void {
        if (state.loginUrlWithReturnUrl) {
            window.location.href = state.loginUrlWithReturnUrl;
        }
    }

    get isAuthenticated(): boolean {
        return !!state.currentPerson;
    }

    getPageParameter(key: string): unknown {
        return state.pageParameters[key];
    }
}

const store = new Store();

export function useStore(): Store {
    return store;
}
