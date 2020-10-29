Obsidian.Elements.RockTextBox = {
    name: 'RockTextBox',
    props: {
        value: {
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
    data: function() {
        return {
            uniqueId: `rock-textbox-${Obsidian.Util.newGuid()}`,
            internalValue: this.value
        };
    },
    methods: {
        handleInput: function() {
            this.$emit('input', this.internalValue);
        },
        handleChange: function() {
            this.$emit('change', this.internalValue);
        }
    },
    watch: {
        value: function() {
            this.internalValue = this.value;
        }
    },
    template:
`<div class="form-group rock-text-box">
    <label class="control-label" :for="uniqueId">
        {{label}}
    </label>
    <div class="control-wrapper">
        <input :id="uniqueId" :type="type" class="form-control" v-model="internalValue" @change="handleChange" @input="handleInput" />
    </div>
</div>`
};
