System.register(["vue", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockFormField_1, NumberUpDownInternal;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("NumberUpDownInternal", NumberUpDownInternal = vue_1.defineComponent({
                name: 'NumberUpDownInternal',
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    },
                    min: {
                        type: Number,
                        default: 1
                    },
                    max: {
                        type: Number,
                        default: 10
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                data: function () {
                    return {
                        internalValue: 0
                    };
                },
                methods: {
                    goUp: function () {
                        if (!this.isUpDisabled) {
                            this.internalValue++;
                        }
                    },
                    goDown: function () {
                        if (!this.isDownDisabled) {
                            this.internalValue--;
                        }
                    }
                },
                computed: {
                    isUpDisabled: function () {
                        return this.internalValue >= this.max;
                    },
                    isDownDisabled: function () {
                        return this.internalValue <= this.min;
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue;
                        }
                    },
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<div class=\"numberincrement\">\n    <a @click=\"goDown\" class=\"numberincrement-down\" :class=\"{disabled: disabled || isDownDisabled}\" :disabled=\"disabled || isDownDisabled\">\n        <i class=\"fa fa-minus \"></i>\n    </a>\n    <span class=\"numberincrement-value\">{{modelValue}}</span>\n    <a @click=\"goUp\" class=\"numberincrement-up\" :class=\"{disabled: disabled || isUpDisabled}\" :disabled=\"disabled || isUpDisabled\">\n        <i class=\"fa fa-plus \"></i>\n    </a>\n</div>"
            }));
            exports_1("default", vue_1.defineComponent({
                name: 'NumberUpDown',
                components: {
                    RockFormField: RockFormField_1.default,
                    NumberUpDownInternal: NumberUpDownInternal
                },
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    },
                    min: {
                        type: Number,
                        default: 1
                    },
                    max: {
                        type: Number,
                        default: 10
                    },
                    numberIncrementClasses: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        internalValue: 0
                    };
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue;
                        }
                    },
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<RockFormField\n    :modelValue=\"internalValue\"\n    formGroupClasses=\"number-up-down\"\n    name=\"numberupdown\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <NumberUpDownInternal v-model=\"internalValue\" :min=\"min\" :max=\"max\" :class=\"numberIncrementClasses\" />\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=NumberUpDown.js.map