System.register(["vue", "./Index", "../Services/Date", "../Elements/DatePicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, Date_1, DatePicker_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (Date_1_1) {
                Date_1 = Date_1_1;
            },
            function (DatePicker_1_1) {
                DatePicker_1 = DatePicker_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'DateField',
                components: {
                    DatePicker: DatePicker_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    valueAsDateString: function () {
                        return Date_1.asDateString(this.modelValue);
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<DatePicker v-if=\"isEditMode\" v-model=\"internalValue\" />\n<span v-else>{{ valueAsDateString }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=DateField.js.map