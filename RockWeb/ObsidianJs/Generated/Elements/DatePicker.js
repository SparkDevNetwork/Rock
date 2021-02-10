System.register(["vue", "../Util/Guid.js", "./RockLabel.js", "../Util/RockDate.js"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Guid_js_1, RockLabel_js_1, RockDate_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (RockLabel_js_1_1) {
                RockLabel_js_1 = RockLabel_js_1_1;
            },
            function (RockDate_js_1_1) {
                RockDate_js_1 = RockDate_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'DatePicker',
                components: {
                    RockLabel: RockLabel_js_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        default: null
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    help: {
                        type: String,
                        default: ''
                    },
                    rules: {
                        type: String,
                        default: ''
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-datepicker-" + Guid_js_1.newGuid(),
                        internalValue: null
                    };
                },
                computed: {
                    isRequired: function () {
                        return this.rules.includes('required');
                    },
                    asRockDateOrNull: function () {
                        return this.internalValue ? RockDate_js_1.default.toRockDate(new Date(this.internalValue)) : null;
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
                            var month = RockDate_js_1.default.getMonth(this.modelValue);
                            var day = RockDate_js_1.default.getDay(this.modelValue);
                            var year = RockDate_js_1.default.getYear(this.modelValue);
                            this.internalValue = month + "/" + day + "/" + year;
                        }
                    }
                },
                mounted: function () {
                    var _this = this;
                    window['Rock'].controls.datePicker.initialize({
                        id: this.uniqueId,
                        startView: 0,
                        showOnFocus: true,
                        format: 'mm/dd/yyyy',
                        todayHighlight: true,
                        forceParse: true,
                        onChangeScript: function () {
                            _this.internalValue = window['$']("#" + _this.uniqueId).val();
                        }
                    });
                },
                template: "\n<div class=\"form-group date-picker required\">\n    <RockLabel :for=\"uniqueId\" :help=\"help\">{{label}}</RockLabel>\n    <div class=\"control-wrapper\">\n        <div class=\"input-group input-width-md js-date-picker date\">\n            <input type=\"text\" :id=\"uniqueId\" class=\"form-control\" v-model.lazy=\"internalValue\" />\n            <span class=\"input-group-addon\">\n                <i class=\"fa fa-calendar\"></i>\n            </span>\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=DatePicker.js.map