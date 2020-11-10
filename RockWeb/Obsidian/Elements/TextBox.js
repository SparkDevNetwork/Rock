Obsidian.Elements.registerElement({
    name: 'TextBox',
    props: {
        modelValue: {
            type: String,
            required: true
        },
        label: {
            type: String,
            required: true
        },
        type: {
            type: String,
            default: 'text'
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function() {
        return {
            uniqueId: `rock-textbox-${Obsidian.Util.newGuid()}`,
            internalValue: this.modelValue
        };
    },
    methods: {
        handleInput: function() {
            this.$emit('update:modelValue', this.internalValue);
        },
    },
    watch: {
        value: function() {
            this.internalValue = this.modelValue;
        }
    },
    template:
`<div class="form-group rock-text-box">
    <label class="control-label" :for="uniqueId">
        {{label}}
    </label>
    <div class="control-wrapper">
        <input :id="uniqueId" :type="type" class="form-control" v-model="internalValue" @input="handleInput" />
    </div>
</div>`
});
