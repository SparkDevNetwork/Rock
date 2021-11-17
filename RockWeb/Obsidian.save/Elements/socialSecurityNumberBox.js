System.register(["vue", "../Rules/index", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "SocialSecurityNumberBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ""
                    },
                    modelValue: {
                        type: String,
                        default: ""
                    }
                },
                data() {
                    return {
                        internalArea: "",
                        internalGroup: "",
                        internalSerial: "",
                        internalValue: ""
                    };
                },
                methods: {
                    getValue() {
                        const value = `${this.internalArea}${this.internalGroup}${this.internalSerial}`;
                        return value;
                    },
                    keyPress(e) {
                        if (/^[0-9]$/.test(e.key) === false) {
                            e.preventDefault();
                            return false;
                        }
                        return true;
                    },
                    keyUp(e) {
                        const area = this.$refs.area;
                        const group = this.$refs.group;
                        const serial = this.$refs.serial;
                        if (/^[0-9]$/.test(e.key) === false) {
                            return true;
                        }
                        if (area === e.target && area.selectionStart === 3) {
                            this.$nextTick(() => {
                                group.focus();
                                group.setSelectionRange(0, 2);
                            });
                        }
                        else if (group === e.target && group.selectionStart === 2) {
                            this.$nextTick(() => {
                                serial.focus();
                                serial.setSelectionRange(0, 4);
                            });
                        }
                        return true;
                    }
                },
                computed: {
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        rules.push("ssn");
                        return index_1.ruleArrayToString(rules);
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            const strippedValue = this.modelValue.replace(/[^0-9]/g, "");
                            if (strippedValue.length !== 9) {
                                this.internalArea = "";
                                this.internalGroup = "";
                                this.internalSerial = "";
                            }
                            else {
                                this.internalArea = strippedValue.substr(0, 3);
                                this.internalGroup = strippedValue.substr(3, 2);
                                this.internalSerial = strippedValue.substr(5, 4);
                            }
                            this.internalValue = this.getValue();
                        }
                    },
                    internalArea() {
                        this.internalValue = this.getValue();
                        if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                            this.$emit("update:modelValue", this.internalValue);
                        }
                    },
                    internalGroup() {
                        this.internalValue = this.getValue();
                        if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                            this.$emit("update:modelValue", this.internalValue);
                        }
                    },
                    internalSerial() {
                        this.internalValue = this.getValue();
                        if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                            this.$emit("update:modelValue", this.internalValue);
                        }
                    },
                },
                template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="social-security-number-box"
    name="social-security-number-box"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <input ref="area" class="form-control ssn-part ssn-area" type="password" pattern="[0-9]*" maxlength="3" v-model="internalArea" v-on:keypress="keyPress" v-on:keyup="keyUp" />
                <span class="separator">-</span>
                <input ref="group" class="form-control ssn-part ssn-group" type="password" pattern="[0-9]*" maxlength="2" v-model="internalGroup" v-on:keypress="keyPress" v-on:keyup="keyUp" />
                <span class="separator">-</span>
                <input ref="serial" class="form-control ssn-part ssn-serial" type="text" pattern="[0-9]*" maxlength="4" v-model="internalSerial" v-on:keypress="keyPress" v-on:keyup="keyUp" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=socialSecurityNumberBox.js.map