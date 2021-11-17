System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "TextBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    type: {
                        type: String,
                        default: "text"
                    },
                    maxLength: {
                        type: Number,
                        default: 524288
                    },
                    showCountDown: {
                        type: Boolean,
                        default: false
                    },
                    placeholder: {
                        type: String,
                        default: ""
                    },
                    inputClasses: {
                        type: String,
                        default: ""
                    },
                    rows: {
                        type: Number,
                        default: 3
                    },
                    textMode: {
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
                    isTextarea() {
                        var _a;
                        return ((_a = this.textMode) === null || _a === void 0 ? void 0 : _a.toLowerCase()) === "multiline";
                    },
                    charsRemaining() {
                        return this.maxLength - this.modelValue.length;
                    },
                    countdownClass() {
                        if (this.charsRemaining >= 10) {
                            return "badge-default";
                        }
                        if (this.charsRemaining >= 0) {
                            return "badge-warning";
                        }
                        return "badge-danger";
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
    name="textbox">
    <template #pre>
        <em v-if="showCountDown" class="pull-right badge" :class="countdownClass">
            {{charsRemaining}}
        </em>
    </template>
    <template #default="{uniqueId, field, errors, disabled, tabIndex}">
        <div class="control-wrapper">
            <textarea v-if="isTextarea" :rows="rows" cols="20" :maxlength="maxLength" :id="uniqueId" class="form-control" v-bind="field"></textarea>
            <input v-else :id="uniqueId" :type="type" class="form-control" :class="inputClasses" v-bind="field" :disabled="disabled" :maxlength="maxLength" :placeholder="placeholder" :tabindex="tabIndex" />
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=textBox.js.map