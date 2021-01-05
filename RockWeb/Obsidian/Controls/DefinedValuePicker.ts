import { defineComponent, inject, PropType } from '../Vendor/Vue/vue.js';
import DropDownList from '../Elements/DropDownList.js';
import { BlockHttp } from './RockBlock.js';
import { CommonEntityOption } from '../Store/Generators.js';
import DefinedValue from '../Types/Models/DefinedValue.js';

export default defineComponent({
    name: 'DefinedValuePicker',
    components: {
        DropDownList
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: 'Defined Value'
        },
        definedTypeGuid: {
            type: String as PropType<string>,
            default: ''
        },
        required: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    setup() {
        return {
            http: inject('http') as BlockHttp
        };
    },
    emits: [
        'update:modelValue',
        'update:model'
    ],
    data() {
        return {
            internalValue: this.modelValue,
            definedValues: [] as DefinedValue[],
            isLoading: false
        };
    },
    computed: {
        isEnabled(): boolean {
            return !!this.definedTypeGuid && !this.isLoading;
        },
        options(): CommonEntityOption[] {
            return this.definedValues.map(dv => ({
                key: dv.Guid,
                value: dv.Guid,
                text: dv.Value
            } as CommonEntityOption));
        }
    },
    methods: {
        onChange: function (): void {
            this.$emit('update:modelValue', this.internalValue);

            const definedValue = this.definedValues.find(dv => dv.Guid === this.internalValue) || null;
            this.$emit('update:model', definedValue);
        }
    },
    watch: {
        value: function (): void {
            this.internalValue = this.modelValue;
        },
        definedTypeGuid: {
            immediate: true,
            handler: async function (): Promise<void> {
                if (!this.definedTypeGuid) {
                    this.definedValues = [];
                }
                else {
                    this.isLoading = true;
                    const result = await this.http.get<DefinedValue[]>(`/api/obsidian/v1/controls/definedvaluepicker/${this.definedTypeGuid}`);

                    if (result && result.data) {
                        this.definedValues = result.data;
                    }

                    this.isLoading = false;
                }

                this.internalValue = '';
                this.onChange();
            }
        }
    },
    template:
        `<DropDownList v-model="internalValue" @change="onChange" :disabled="!isEnabled" :label="label" :options="options" />`
});
