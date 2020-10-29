Obsidian.Controls.RockDefinedTypePicker = {
    name: 'RockDefinedTypePicker',
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
        required: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            internalValue: this.value,
            definedTypes: [],
            isLoading: false
        };
    },
    computed: {
        options: function () {
            return this.definedTypes.map(dt => ({
                key: dt.Guid,
                value: dt.Guid,
                text: dt.Name
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
        }
    },
    created: async function () {
        this.isLoading = true;
        const result = await this.http.get('/api/obsidian/v1/controls/definedtypepicker');

        if (result && Array.isArray(result.data)) {
            this.definedTypes = result.data;
        }

        this.isLoading = false;
    },
    template:
`<RockDropDownList v-model="internalValue" @change="onChange" :disabled="isLoading" :label="label" :options="options" />`
};
