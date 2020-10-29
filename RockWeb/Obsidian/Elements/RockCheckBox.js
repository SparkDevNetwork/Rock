Obsidian.Elements.RockCheckBox = {
    props: {
        value: {
            type: Boolean,
            required: true
        },
        label: {
            type: String,
            required: true
        }
    },
    data: function() {
        return {
            uniqueId: `rock-checkbox-${Obsidian.Util.newGuid()}`,
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
`<div class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>`
};
