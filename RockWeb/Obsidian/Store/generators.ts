import { defineComponent } from '../Vendor/Vue/vue.js';
import DropDownList from '../Elements/DropDownList.js';
import { Module } from '../Vendor/Vuex/index.js';
import { RootState } from './index.js';
import Entity from '../Types/Models/entity.js';
import cache from '../Util/cache.js';
import http from '../Util/http.js';
import { CommonEntity } from './commonEntities.js';

export type CommonEntityOption = {
    key: string;
    value: string;
    text: string;
};

/**
* Generate and add a common entity picker. Common entities are those stored in the Obsidian store.
* @param {any} entityName The entity name (ex: Campus)
* @param {any} getOptionsFunc A function called with the store as a parameter that should return the
* options object list for the drop down list.
*/
export function createCommonEntityPicker(entityName: string, getOptionsFunc: () => CommonEntityOption[]) {
    return defineComponent({
        name: `${entityName}Picker`,
        components: {
            DropDownList
        },
        props: {
            modelValue: {
                type: String,
                required: true
            },
            label: {
                type: String,
                required: true
            },
            required: {
                type: Boolean,
                default: false
            }
        },
        emits: [
            'update:modelValue'
        ],
        data: function () {
            return {
                internalValue: '',
                isLoading: false
            };
        },
        computed: {
            options() {
                return getOptionsFunc();
            }
        },
        methods: {
            onChange: function () {
                this.$emit('update:modelValue', this.internalValue);
            }
        },
        watch: {
            value: function () {
                this.internalValue = this.modelValue;
            }
        },
        template:
            `<DropDownList v-model="internalValue" @change="onChange" :disabled="isLoading" :label="label" :options="options" />`
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
                return guid => state.items.find(i => i.Guid === guid);
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