System.register(["vue", "./utils", "../Elements/dateTimePicker", "../Services/boolean"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, utils_1, dateTimePicker_1, boolean_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (dateTimePicker_1_1) {
                dateTimePicker_1 = dateTimePicker_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DateTimeField.Edit",
                components: {
                    DateTimePicker: dateTimePicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup() {
                    return {};
                },
                data() {
                    return {
                        internalValue: "",
                        formattedString: ""
                    };
                },
                methods: {
                    syncModelValue() {
                        var _a;
                        return __awaiter(this, void 0, void 0, function* () {
                            this.internalValue = (_a = this.modelValue) !== null && _a !== void 0 ? _a : "";
                        });
                    },
                },
                computed: {
                    dateFormatTemplate() {
                        const formatConfig = this.configurationValues["format"];
                        return formatConfig || "MM/dd/yyyy";
                    },
                    configAttributes() {
                        const attributes = {};
                        const displayCurrentConfig = this.configurationValues["displayCurrentOption"];
                        const displayCurrent = boolean_1.asBoolean(displayCurrentConfig);
                        attributes.displayCurrentOption = displayCurrent;
                        attributes.isCurrentDateOffset = displayCurrent;
                        return attributes;
                    }
                },
                watch: {
                    internalValue() {
                        var _a;
                        if (this.internalValue !== this.modelValue) {
                            const d1 = Date.parse(this.internalValue);
                            const d2 = Date.parse((_a = this.modelValue) !== null && _a !== void 0 ? _a : "");
                            if (isNaN(d1) || isNaN(d2) || d1 !== d2) {
                                this.$emit("update:modelValue", this.internalValue);
                            }
                        }
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            return __awaiter(this, void 0, void 0, function* () {
                                yield this.syncModelValue();
                            });
                        }
                    }
                },
                template: `
<DateTimePicker v-model="internalValue" v-bind="configAttributes" />
`
            }));
        }
    };
});
//# sourceMappingURL=dateTimeFieldComponents.js.map