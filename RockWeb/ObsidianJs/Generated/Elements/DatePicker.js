System.register(["vue", "../Util/RockDate", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockDate_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockDate_1_1) {
                RockDate_1 = RockDate_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'DatePicker',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        default: null
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        internalValue: null
                    };
                },
                computed: {
                    asRockDateOrNull: function () {
                        return this.internalValue ? RockDate_1.default.toRockDate(new Date(this.internalValue)) : null;
                    }
                },
                methods: {
                    onChange: function (arg) {
                        console.log('change', arg);
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.asRockDateOrNull);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            if (!this.modelValue) {
                                this.internalValue = null;
                                return;
                            }
                            var month = RockDate_1.default.getMonth(this.modelValue);
                            var day = RockDate_1.default.getDay(this.modelValue);
                            var year = RockDate_1.default.getYear(this.modelValue);
                            this.internalValue = month + "/" + day + "/" + year;
                        }
                    }
                },
                mounted: function () {
                    var _this = this;
                    var input = this.$refs['input'];
                    var inputId = input.id;
                    window['Rock'].controls.datePicker.initialize({
                        id: inputId,
                        startView: 0,
                        showOnFocus: true,
                        format: 'mm/dd/yyyy',
                        todayHighlight: true,
                        forceParse: true,
                        onChangeScript: function () {
                            _this.internalValue = input.value;
                        }
                    });
                },
                template: "\n<RockFormField formGroupClasses=\"date-picker\" #default=\"{uniqueId}\" name=\"datepicker\" v-model.lazy=\"internalValue\">\n    <div class=\"control-wrapper\">\n        <div class=\"input-group input-width-md js-date-picker date\">\n            <input ref=\"input\" type=\"text\" :id=\"uniqueId\" class=\"form-control\" v-model.lazy=\"internalValue\" />\n            <span class=\"input-group-addon\">\n                <i class=\"fa fa-calendar\"></i>\n            </span>\n        </div>\n    </div>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=DatePicker.js.map