Obsidian.Controls.RockDefinedValuePicker = {
    name: 'RockDefinedValuePicker',
    components: {
        RockDropDownList: Obsidian.Elements.RockDropDownList
    },
    inject: [
        'http'
    ],
    props: {
        value: {
            type: String,
            required: true
        },
        label: {
            type: String,
            required: true
        },
        definedTypeGuid: {
            type: String,
            default: ''
        },
        required: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            internalValue: this.value,
            definedValues: [],
            isLoading: false
        };
    },
    computed: {
        isEnabled: function () {
            return !!this.definedTypeGuid && !this.isLoading;
        },
        options: function () {
            return this.definedValues.map(dv => ({
                key: dv.Guid,
                value: dv.Guid,
                text: dv.Value
            }));
        }
    },
    methods: {
        onChange: function () {
            this.$emit('input', this.internalValue);
            this.$emit('change', this.internalValue);
        }
    },
    watch: {
        value: function () {
            this.internalValue = this.value;
        },
        definedTypeGuid: {
            immediate: true,
            handler: async function () {
                if (!this.definedTypeGuid) {
                    this.definedValues = [];
                }
                else {
                    this.isLoading = true;
                    const result = await this.http.get(`/api/obsidian/v1/controls/definedvaluepicker/${this.definedTypeGuid}`);

                    if (result && Array.isArray(result.data)) {
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
`<RockDropDownList v-model="internalValue" @change="onChange" :disabled="!isEnabled" :label="label" :options="options" />`
};
