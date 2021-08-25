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
import { DebugTimingViewModel } from '../Controls/PageDebugTimings';
import { PageConfig } from '../Index';
import { Guid } from '../Util/Guid';
import { createStore, Store } from 'vuex';
import Group from '../ViewModels/CodeGenerated/GroupViewModel';
import Person from '../ViewModels/CodeGenerated/PersonViewModel';
import Entity from '../ViewModels/Entity';
import { commonEntities, commonEntityModules } from './CommonEntities';

export interface RootState {
    areSecondaryBlocksShown: boolean;
    currentPerson: Person | null;
    pageParameters: Record<string, unknown>;
    contextEntities: Record<string, Entity>;
    pageId: number;
    pageGuid: Guid;
    executionStartTime: Date;
    debugTimings: DebugTimingViewModel[],
    loginUrlWithReturnUrl: string
}

declare module '@vue/runtime-core' {
    // provide typings for `this.$store`
    interface ComponentCustomProperties {
        $store: Store<RootState>
    }
}

export type ReportDebugTimingArgs = {
    Title: string;
    Subtitle: string;
    StartTimeMs: number;
    FinishTimeMs: number;
};

// Declare the Vuex store
export default createStore<RootState>({
    state: {
        areSecondaryBlocksShown: true,
        currentPerson: null,
        pageParameters: {},
        contextEntities: {},
        pageId: 0,
        pageGuid: '' as Guid,
        executionStartTime: new Date(),
        debugTimings: [],
        loginUrlWithReturnUrl: ''
    },
    getters: {
        isAuthenticated(state) {
            return !!state.currentPerson;
        },
        contextEntity(state): (type: string) => Entity | null {
            return (type: string) => (state.contextEntities[type] || null);
        },
        personContext(state, getters): Person | null {
            return getters.contextEntity('Person');
        },
        groupContext(state, getters): Group | null {
            return getters.contextEntity('Group');
        },
        pageParameter(state): (key: string) => unknown {
            return (key: string) => (state.pageParameters[key]);
        }
    },
    mutations: {
        setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
            state.areSecondaryBlocksShown = areSecondaryBlocksShown;
        },
        setPageInitializationData(state, pageConfig: PageConfig) {
            state.currentPerson = pageConfig.currentPerson || null;
            state.pageParameters = pageConfig.pageParameters || {};
            state.contextEntities = pageConfig.contextEntities || {};
            state.pageId = pageConfig.pageId || 0;
            state.pageGuid = pageConfig.pageGuid || '';
            state.executionStartTime = pageConfig.executionStartTime;
            state.loginUrlWithReturnUrl = pageConfig.loginUrlWithReturnUrl;
        },
        reportOnLoadDebugTiming(state, payload: ReportDebugTimingArgs) {
            const pageStartTime = state.executionStartTime.getTime();
            const timestampMs = payload.StartTimeMs - pageStartTime;
            const durationMs = payload.FinishTimeMs - payload.StartTimeMs;

            state.debugTimings.push({
                TimestampMs: timestampMs,
                DurationMs: durationMs,
                IndentLevel: 1,
                IsTitleBold: false,
                SubTitle: payload.Subtitle,
                Title: payload.Title
            });
        }
    },
    actions: {
        initialize(context, { pageConfig }: { pageConfig: PageConfig }) {
            context.commit('setPageInitializationData', pageConfig);

            // Initialize each common entity module
            for (const commonEntity of commonEntities) {
                context.dispatch(`${commonEntity.namespace}/initialize`);
            }
        },
        redirectToLogin ( context )
        {
            if ( context.state.loginUrlWithReturnUrl )
            {
                window.location.href = context.state.loginUrlWithReturnUrl;
            }
        }
    },
    modules: {
        ...commonEntityModules
    }
});