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
import { defineComponent, PropType } from 'vue';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import { Module } from 'vuex';
import { RootState } from './Index';
import cache from '../Util/Cache';
import http from '../Util/Http';
import { CommonEntity } from './CommonEntities';
import { splitCamelCase } from '../Services/String';
import { Guid } from '../Util/Guid';
import Entity from '../ViewModels/Entity';

export type CommonEntityOption = {
    Guid: Guid;
    Id: number;
    Text: string;
};

/**
* Generate and add a common entity picker. Common entities are those stored in the Obsidian store.
* @param {any} entityName The entity name (ex: Campus)
* @param {any} getOptionsFunc A function called with the store as a parameter that should return the
* options object list for the drop down list.
*/
export function createCommonEntityPicker(entityName: string, getOptionsFunc: () => CommonEntityOption[]) {
    const entityNameForDisplay = splitCamelCase(entityName);

    return defineComponent({
        name: `${entityName}Picker`,
        components: {
            DropDownList
        },
        props: {
            modelValue: {
                type: String as PropType<Guid>,
                required: true
            },
            label: {
                type: String as PropType<string>,
                default: entityNameForDisplay
            }
        },
        data() {
            return {
                providedOptions: getOptionsFunc(),
                selectedGuid: '',
                isLoading: false
            };
        },
        computed: {
            options(): DropDownListOption[] {
                return getOptionsFunc().map(o => ({
                    key: o.Guid,
                    text: o.Text,
                    value: o.Guid
                } as DropDownListOption));
            }
        },
        watch: {
            modelValue: {
                immediate: true,
                handler() {
                    this.selectedGuid = this.modelValue;
                }
            },
            selectedGuid: {
                immediate: true,
                handler() {
                    this.$emit('update:modelValue', this.selectedGuid);
                }
            }
        },
        template: `
<DropDownList v-model="selectedGuid" :disabled="isLoading" :label="label" :options="options" />`
    });
}

/**
* Generate a Vuex module that fetches, caches, and stores common entities for use across all controls and blocks.
* Provide the namespace (ex: campuses) that will serve as the Vuex namespace.
* Also provide the apiUrl (ex: api/campuses) that allows the module to hydrate its items when needed.
*/
export function generateCommonEntityModule<TEntity extends Entity>(commonEntity: CommonEntity): Module<{ items: TEntity[] }, RootState> {
    return {
        namespaced: true as const,
        state: {
            items: []
        },
        mutations: {
            setItems(state, { items }: { items: TEntity[] }) {
                state.items = items;
            }
        },
        getters: {
            all(state) {
                return state.items;
            },
            getByGuid(state) {
                return (guid: Guid) => {
                    return state.items.find(i => i.guid === guid) || null;
                };
            },
            getById(state) {
                return (id: number) => {
                    return state.items.find(i => i.id === id) || null;
                };
            }
        },
        actions: {
            async initialize(context) {
                const cacheKey = `common-entity-${commonEntity.namespace}`;
                let items = cache.get<TEntity[]>(cacheKey) || [];

                if (!items || !items.length) {
                    const response = await http.get<TEntity[]>(commonEntity.apiUrl);
                    items = response.data || [];
                    cache.set(cacheKey, items);
                }

                context.commit('setItems', { items });
            }
        }
    };
}