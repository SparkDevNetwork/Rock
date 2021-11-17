System.register(["../Rules/index", "vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var index_1, vue_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "EmailBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    allowLava: {
                        type: Boolean,
                        default: false
                    },
                    allowMultiple: {
                        type: Boolean,
                        default: false
                    },
                    rules: {
                        type: String,
                        default: ""
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        if (rules.indexOf("email") === -1 && !this.allowLava && !this.allowMultiple) {
                            rules.push("email");
                        }
                        return index_1.ruleArrayToString(rules);
                    },
                    computedType() {
                        return this.allowLava || this.allowMultiple ? "text" : "email";
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue() {
                        this.internalValue = this.modelValue;
                    }
                },
                template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-text-box"
    name="textbox"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, tabIndex, disabled}">
        <div class="control-wrapper">
            <div class="input-group">
                <span class="input-group-addon">
                    <i class="fa fa-envelope"></i>
                </span>
                <input :id="uniqueId" class="form-control" v-bind="field" :disabled="disabled" :tabindex="tabIndex" :type="computedType" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=emailBox.js.map