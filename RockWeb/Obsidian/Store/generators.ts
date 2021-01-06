import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList.js';
import { Module } from '../Vendor/Vuex/index.js';
import { RootState } from './index.js';
import Entity from '../Types/Models/entity.js';
import cache from '../Util/cache.js';
import http from '../Util/http.js';
import { CommonEntity } from './commonEntities.js';
import { splitCamelCase } from '../Filters/String.js';
import { Guid } from '../Util/Guid.js';

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
                default: ''
            },
            id: {
                type: Number as PropType<number>,
                default: 0
            },
            label: {
                type: String,
                default: entityNameForDisplay
            },
            required: {
                type: Boolean,
                default: false
            }
        },
        emits: [
            'update:modelValue',
            'update:id'
        ],
        data: function () {
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
            },
            currentCommonEntity(): CommonEntityOption | null {
                return this.providedOptions.find(o => o.Guid === this.selectedGuid) || null;
            }
        },
        methods: {
            onChange: function () {
                this.$emit('update:modelValue', this.selectedGuid);
                this.$emit('update:id', this.currentCommonEntity ? this.currentCommonEntity.Id : null);
            }
        },
        watch: {
            value: {
                immediate: true,
                handler() {
                    this.selectedGuid = this.modelValue;
                }
            },
            id: {
                immediate: true,
                handler() {
                    const option = this.providedOptions.find(o => o.Id === this.id) || null;
                    this.selectedGuid = option ? option.Guid : '';
                }
            }
        },
        template: `
<DropDownList v-model="selectedGuid" @change="onChange" :disabled="isLoading" :label="label" :options="options" />`
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
                    return state.items.find(i => i.Guid === guid) || null;
                };
            },
            getById(state) {
                return (id: number) => {
                    return state.items.find(i => i.Id === id) || null;
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