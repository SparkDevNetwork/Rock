System.register(["vue", "./utils", "../Elements/dropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, dropDownList_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "GenderField.Edit",
                components: {
                    DropDownList: dropDownList_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: ""
                    };
                },
                computed: {
                    dropDownListOptions() {
                        const hideUnknownGenderConfig = this.configurationValues["hideUnknownGender"];
                        const hideUnknownGender = hideUnknownGenderConfig.toLowerCase() === "true";
                        if (hideUnknownGender === false) {
                            return [
                                { text: "Unknown", value: "0" },
                                { text: "Male", value: "1" },
                                { text: "Female", value: "2" }
                            ];
                        }
                        else {
                            return [
                                { text: "Male", value: "1" },
                                { text: "Female", value: "2" }
                            ];
                        }
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue || "";
                        }
                    }
                },
                template: `
<DropDownList v-model="internalValue" :options="dropDownListOptions" formControlClasses="input-width-md" />
`
            }));
        }
    };
});
//# sourceMappingURL=genderFieldComponents.js.map