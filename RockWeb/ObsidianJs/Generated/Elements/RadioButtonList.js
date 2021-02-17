// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
                template: "\n<RockFormField formGroupClasses=\"rock-radio-button-list\" #default=\"{uniqueId}\" name=\"radiobuttonlist\" v-model=\"internalValue\">\n    <div class=\"control-wrapper\">\n        <div class=\"controls rockradiobuttonlist rockradiobuttonlist-vertical\">\n            <span>\n                <div v-for=\"option in options\" class=\"radio\">\n                    <label class=\"\" :for=\"getOptionUniqueId(uniqueId, option)\">\n                        <input :id=\"getOptionUniqueId(uniqueId, option)\" :name=\"uniqueId\" type=\"radio\" :value=\"option.value\" v-model=\"internalValue\" />\n                        <span class=\"label-text\">{{option.text}}</span>\n                    </label>\n                </div>\n            </span>\n        </div>\n    </div>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=RadioButtonList.js.map