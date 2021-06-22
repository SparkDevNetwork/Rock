System.register(["vue", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockFormField_1;
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
            exports_1("default", vue_1.defineComponent({
                name: 'RadioButtonList',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    options: {
                        type: Array,
                        default: []
                    },
                    modelValue: {
                        type: String,
                        default: ''
                    },
                    repeatColumns: {
                        type: Number,
                        default: 0
                    },
                    horizontal: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    containerClasses: function () {
                        var classes = [];
                        if (this.repeatColumns > 0) {
                            classes.push("in-columns in-columns-" + this.repeatColumns);
                        }
                        if (this.horizontal) {
                            classes.push('rockradiobuttonlist-horizontal');
                        }
                        else {
                            classes.push('rockradiobuttonlist-vertical');
                        }
                        return classes.join(' ');
                    }
                },
                methods: {
                    getOptionUniqueId: function (uniqueId, option) {
                        return uniqueId + "-" + option.key;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue;
                        }
                    }
                },
                template: "\n<RockFormField formGroupClasses=\"rock-radio-button-list\" #default=\"{uniqueId}\" name=\"radiobuttonlist\" v-model=\"internalValue\">\n    <div class=\"control-wrapper\">\n        <div class=\"controls rockradiobuttonlist\" :class=\"containerClasses\">\n            <span>\n                <template v-if=\"horizontal\">\n                    <label v-for=\"option in options\" class=\"radio-inline\" :for=\"getOptionUniqueId(uniqueId, option)\">\n                        <input :id=\"getOptionUniqueId(uniqueId, option)\" :name=\"uniqueId\" type=\"radio\" :value=\"option.value\" v-model=\"internalValue\" />\n                        <span class=\"label-text\">{{option.text}}</span>\n                    </label>\n                </template>\n                <template v-else>\n                    <div v-for=\"option in options\" class=\"radio\">\n                        <label :for=\"getOptionUniqueId(uniqueId, option)\">\n                            <input :id=\"getOptionUniqueId(uniqueId, option)\" :name=\"uniqueId\" type=\"radio\" :value=\"option.value\" v-model=\"internalValue\" />\n                            <span class=\"label-text\">{{option.text}}</span>\n                        </label>\n                    </div>\n                </template>\n            </span>\n        </div>\n    </div>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=RadioButtonList.js.map