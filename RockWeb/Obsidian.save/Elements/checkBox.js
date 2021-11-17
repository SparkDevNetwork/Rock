System.register(["vue", "../Util/guid", "../Rules/index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, guid_1, index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "CheckBox",
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    inline: {
                        type: Boolean,
                        default: true
                    },
                    rules: {
                        type: String,
                        default: ""
                    }
                },
                data: function () {
                    return {
                        uniqueId: `rock-checkbox-${guid_1.newGuid()}`,
                        internalValue: this.modelValue
                    };
                },
                methods: {
                    toggle() {
                        this.internalValue = !this.internalValue;
                    }
                },
                computed: {
                    isRequired() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        return rules.indexOf("required") !== -1;
                    }
                },
                watch: {
                    modelValue() {
                        this.internalValue = this.modelValue;
                    },
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    }
                },
                template: `
<div v-if="inline" class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>
<div v-else class="form-group rock-check-box" :class="isRequired ? 'required' : ''">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <div class="rock-checkbox-icon" @click="toggle">
            <i v-if="modelValue" class="fa fa-check-square-o fa-lg"></i>
            <i v-else class="fa fa-square-o fa-lg"></i>
        </div>
    </div>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=checkBox.js.map