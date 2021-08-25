System.register(["vue", "./Index", "../Elements/DropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DropDownList_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '2E28779B-4C76-4142-AE8D-49EA31DDB503';
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'GenderField',
                components: {
                    DropDownList: DropDownList_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    displayValue: function () {
                        if (this.internalValue === '0') {
                            return 'Unknown';
                        }
                        else if (this.internalValue === '1') {
                            return 'Male';
                        }
                        else if (this.internalValue === '2') {
                            return 'Female';
                        }
                        else {
                            return '';
                        }
                    },
                    dropDownListOptions: function () {
                        return [
                            { key: '0', text: 'Unknown', value: '0' },
                            { key: '1', text: 'Male', value: '1' },
                            { key: '2', text: 'Female', value: '2' }
                        ];
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue || '';
                        }
                    }
                },
                template: "\n<DropDownList v-if=\"isEditMode\" v-model=\"internalValue\" :options=\"dropDownListOptions\" />\n<span v-else>{{ displayValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=GenderField.js.map