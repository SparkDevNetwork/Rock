import { PageConfig } from '../Index.js';
import { Guid } from '../Util/Guid.js';
import { createStore } from '../Vendor/Vuex/index.js';
import Group from '../ViewModels/CodeGenerated/GroupViewModel.js';
import Person from '../ViewModels/CodeGenerated/PersonViewModel.js';
import Entity from '../ViewModels/Entity.js';
import { commonEntities, commonEntityModules } from './CommonEntities.js';

export interface RootState {
    areSecondaryBlocksShown: boolean;
    currentPerson: Person | null;
    pageParameters: Record<string, unknown>;
    contextEntities: Record<string, Entity>;
    pageId: number;
    pageGuid: Guid;
}

// Declare the Vuex store
export default createStore<RootState>({
    state: {
        areSecondaryBlocksShown: true,
        currentPerson: null,
        pageParameters: {},
        contextEntities: {},
        pageId: 0,
        pageGuid: '' as Guid,
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
        }
    },
    actions: {
        initialize(context, { pageConfig }: { pageConfig: PageConfig }) {
            context.commit('setPageInitializationData', pageConfig);

            // Initialize each common entity module
            for (const commonEntity of commonEntities) {
                context.dispatch(`${commonEntity.namespace}/initialize`);
            }
        }
    },
    modules: {
        ...commonEntityModules
    }
});