System.register(["vue", "../Services/number", "../Services/string"], function (exports_1, context_1) {
    "use strict";
    var vue_1, number_1, string_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "BasicTimePicker",
                components: {},
                props: {
                    modelValue: {
                        type: Object,
                        default: {}
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                data() {
                    return {
                        internalHour: null,
                        internalMinute: null,
                        internalMeridiem: "AM",
                        internalValue: ""
                    };
                },
                methods: {
                    keyPress(e) {
                        if (e.key === "a" || e.key === "p" || e.key === "A" || e.key == "P") {
                            this.internalMeridiem = e.key === "a" || e.key === "A" ? "AM" : "PM";
                            this.maybeUpdateValue();
                            e.preventDefault();
                            return false;
                        }
                        if (/^[0-9:]$/.test(e.key) === false) {
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
                    },
                    updateValue() {
                        const values = /(\d+):(\d+)/.exec(this.internalValue);
                        const value = {};
                        if (values !== null) {
                            value.hour = number_1.toNumber(values[1]) + (this.internalMeridiem === "PM" ? 12 : 0);
                            value.minute = number_1.toNumber(values[2]);
                        }
                        this.$emit("update:modelValue", value);
                    },
                    maybeUpdateValue() {
                        const values = /(\d+):(\d+)/.exec(this.internalValue);
                        if (values !== null) {
                            this.updateValue();
                        }
                    },
                    toggleMeridiem(e) {
                        e.preventDefault();
                        this.internalMeridiem = this.internalMeridiem === "AM" ? "PM" : "AM";
                        this.maybeUpdateValue();
                        return false;
                    }
                },
                computed: {},
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            if (this.modelValue.hour) {
                                if (this.modelValue.hour > 12) {
                                    this.internalHour = this.modelValue.hour - 12;
                                }
                                else {
                                    this.internalHour = this.modelValue.hour;
                                }
                                if (this.modelValue.hour >= 12) {
                                    this.internalMeridiem = "PM";
                                }
                            }
                            else {
                                this.internalHour = null;
                            }
                            if (this.modelValue.minute) {
                                this.internalMinute = this.modelValue.minute;
                            }
                            else if (this.internalHour != null) {
                                this.internalMinute = 0;
                            }
                            else {
                                this.internalMinute = null;
                            }
                            if (this.internalHour === null || this.internalMinute === null) {
                                return;
                            }
                            this.internalValue = `${this.internalHour}:${string_1.padLeft(this.internalMinute.toString(), 2, "0")}`;
                        }
                    }
                },
                template: `
<div class="input-group input-width-md">
    <input class="form-control" type="text" v-model="internalValue" v-on:change="updateValue" v-on:keypress="keyPress" :disabled="disabled" />
    <span class="input-group-btn"><button class="btn btn-default" v-on:click="toggleMeridiem" :disabled="disabled">{{ internalMeridiem }}</button></span>
</div>
`
            }));
        }
    };
});
//# sourceMappingURL=basicTimePicker.js.map