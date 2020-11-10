Obsidian.Elements.registerElement({
    name: 'DropDownList',
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
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-dropdownlist-${Obsidian.Util.newGuid()}`,
            internalValue: this.modelValue
        };
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
`<div class="form-group rock-drop-down-list" :class="{required: required}">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <select :id="uniqueId" class="form-control" v-model="internalValue" @change="onChange" :disabled="disabled">
            <option value=""></option>
            <option v-for="o in options" :key="o.key" :value="o.value">{{o.text}}</option>
        </select>
    </div>
</div>`
});
