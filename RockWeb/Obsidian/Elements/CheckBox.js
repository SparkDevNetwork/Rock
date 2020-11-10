Obsidian.Elements.registerElement({
    name: 'CheckBox',
    props: {
        modelValue: {
            type: Boolean,
            required: true
        },
        label: {
            type: String,
            required: true
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-checkbox-${Obsidian.Util.newGuid()}`,
            internalValue: this.modelValue
        };
    },
    methods: {
        handleInput: function () {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    watch: {
        value: function () {
            this.internalValue = this.modelValue;
        }
    },
    template:
`<div class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>`
});
