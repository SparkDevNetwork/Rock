import { defineComponent } from "../Vendor/Vue/vue.js";
import DropDownList from "../Elements/DropDownList.js";

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