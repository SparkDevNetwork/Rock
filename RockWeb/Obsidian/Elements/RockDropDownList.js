Obsidian.Elements.RockDropDownList = {
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
        },
        disabled: {
            type: Boolean,
            default: false
        },
        options: {
            type: Array,
            required: true
        }
    },
    data: function() {
        return {
            uniqueId: `rock-dropdownlist-${Obsidian.Util.newGuid()}`,
            internalValue: this.value
        };
    },
    methods: {
        onChange: function () {
            this.$emit('input', this.internalValue);
            this.$emit('change', this.internalValue);
        }
    },
    watch: {
        value: function() {
            this.internalValue = this.value;
        }
    },
    template:
`<div class="form-group rock-drop-down-list" :class="{required: required}">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <select :id="uniqueId" class="form-control" v-model="internalValue" @change="onChange" :disabled="disabled">
            <option value=""></option>
            <option v-for="o in options" :key="o.key" :value="o.value">{{o.text}}</option>
        </select>
    </div>
</div>`
};
